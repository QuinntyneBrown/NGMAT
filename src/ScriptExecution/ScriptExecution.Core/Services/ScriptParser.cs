using System.Text.RegularExpressions;
using ScriptExecution.Core.Models;

namespace ScriptExecution.Core.Services;

/// <summary>
/// GMAT-compatible script parser
/// </summary>
public sealed class ScriptParser
{
    private static readonly Regex CreateRegex = new(@"^\s*Create\s+(\w+)\s+(\w+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex SetRegex = new(@"^\s*(\w+)\.(\w+)\s*=\s*(.+)\s*$", RegexOptions.Compiled);
    private static readonly Regex PropagateRegex = new(@"^\s*Propagate\s+(\w+)\s*\(\s*(\w+)\s*\)\s*(?:\{\s*(\w+\.\w+)\s*=\s*([^\}]+)\s*\})?\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ReportRegex = new(@"^\s*Report\s+(.+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex VariableRegex = new(@"^\s*(?:var\s+)?(\w+)\s*=\s*(.+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CommentRegex = new(@"^\s*%.*$", RegexOptions.Compiled);
    private static readonly Regex IfRegex = new(@"^\s*If\s+(.+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ElseRegex = new(@"^\s*Else\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex EndIfRegex = new(@"^\s*EndIf\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex WhileRegex = new(@"^\s*While\s+(.+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex EndWhileRegex = new(@"^\s*EndWhile\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex SaveRegex = new(@"^\s*Save\s+(\w+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Parse a GMAT script
    /// </summary>
    public ParseResult Parse(string script)
    {
        var ast = new List<AstNode>();
        var errors = new List<ScriptError>();
        var warnings = new List<ScriptWarning>();

        var lines = script.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        int commandCount = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var lineNumber = i + 1;

            // Skip empty lines
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Skip comments
            if (CommentRegex.IsMatch(line))
            {
                ast.Add(new AstNode
                {
                    Type = CommandType.Comment,
                    LineNumber = lineNumber,
                    RawText = line.Trim()
                });
                continue;
            }

            var node = ParseLine(line, lineNumber, errors, warnings);
            if (node != null)
            {
                ast.Add(node);
                if (node.Type != CommandType.Comment && node.Type != CommandType.Unknown)
                {
                    commandCount++;
                }
            }
        }

        return new ParseResult
        {
            IsValid = errors.Count == 0,
            Ast = ast,
            Errors = errors,
            Warnings = warnings,
            LineCount = lines.Length,
            CommandCount = commandCount
        };
    }

    private AstNode? ParseLine(string line, int lineNumber, List<ScriptError> errors, List<ScriptWarning> warnings)
    {
        // Try Create
        var createMatch = CreateRegex.Match(line);
        if (createMatch.Success)
        {
            return new CreateNode
            {
                LineNumber = lineNumber,
                RawText = line.Trim(),
                ObjectType = createMatch.Groups[1].Value,
                ObjectName = createMatch.Groups[2].Value
            };
        }

        // Try Propagate
        var propagateMatch = PropagateRegex.Match(line);
        if (propagateMatch.Success)
        {
            return new PropagateNode
            {
                LineNumber = lineNumber,
                RawText = line.Trim(),
                PropagatorName = propagateMatch.Groups[1].Value,
                SpacecraftName = propagateMatch.Groups[2].Value,
                StopCondition = propagateMatch.Groups[3].Success ? propagateMatch.Groups[3].Value : null,
                StopValue = propagateMatch.Groups[4].Success ? ParseDouble(propagateMatch.Groups[4].Value) : null
            };
        }

        // Try Set (ObjectName.Property = Value)
        var setMatch = SetRegex.Match(line);
        if (setMatch.Success)
        {
            return new SetNode
            {
                LineNumber = lineNumber,
                RawText = line.Trim(),
                ObjectName = setMatch.Groups[1].Value,
                PropertyName = setMatch.Groups[2].Value,
                Value = ParseValue(setMatch.Groups[3].Value.Trim())
            };
        }

        // Try Report
        var reportMatch = ReportRegex.Match(line);
        if (reportMatch.Success)
        {
            var parameters = reportMatch.Groups[1].Value
                .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            return new ReportNode
            {
                LineNumber = lineNumber,
                RawText = line.Trim(),
                Parameters = parameters
            };
        }

        // Try Save
        var saveMatch = SaveRegex.Match(line);
        if (saveMatch.Success)
        {
            return new AstNode
            {
                Type = CommandType.Save,
                LineNumber = lineNumber,
                RawText = line.Trim(),
                Properties = { ["ObjectName"] = saveMatch.Groups[1].Value }
            };
        }

        // Try If
        var ifMatch = IfRegex.Match(line);
        if (ifMatch.Success)
        {
            return new AstNode
            {
                Type = CommandType.If,
                LineNumber = lineNumber,
                RawText = line.Trim(),
                Properties = { ["Condition"] = ifMatch.Groups[1].Value.Trim() }
            };
        }

        // Try Else
        if (ElseRegex.IsMatch(line))
        {
            return new AstNode
            {
                Type = CommandType.Else,
                LineNumber = lineNumber,
                RawText = line.Trim()
            };
        }

        // Try EndIf
        if (EndIfRegex.IsMatch(line))
        {
            return new AstNode
            {
                Type = CommandType.EndIf,
                LineNumber = lineNumber,
                RawText = line.Trim()
            };
        }

        // Try While
        var whileMatch = WhileRegex.Match(line);
        if (whileMatch.Success)
        {
            return new AstNode
            {
                Type = CommandType.While,
                LineNumber = lineNumber,
                RawText = line.Trim(),
                Properties = { ["Condition"] = whileMatch.Groups[1].Value.Trim() }
            };
        }

        // Try EndWhile
        if (EndWhileRegex.IsMatch(line))
        {
            return new AstNode
            {
                Type = CommandType.EndWhile,
                LineNumber = lineNumber,
                RawText = line.Trim()
            };
        }

        // Try Variable assignment
        var varMatch = VariableRegex.Match(line);
        if (varMatch.Success)
        {
            return new VariableNode
            {
                LineNumber = lineNumber,
                RawText = line.Trim(),
                VariableName = varMatch.Groups[1].Value,
                Value = ParseValue(varMatch.Groups[2].Value.Trim())
            };
        }

        // Unknown command
        errors.Add(new ScriptError
        {
            LineNumber = lineNumber,
            Column = 1,
            Message = $"Unknown or invalid command: {line.Trim()}",
            Code = "SE001",
            Severity = ScriptErrorSeverity.Error
        });

        return new AstNode
        {
            Type = CommandType.Unknown,
            LineNumber = lineNumber,
            RawText = line.Trim()
        };
    }

    private object ParseValue(string value)
    {
        // Try to parse as number
        if (double.TryParse(value, out var d))
            return d;

        // Remove quotes for strings
        if ((value.StartsWith("'") && value.EndsWith("'")) ||
            (value.StartsWith("\"") && value.EndsWith("\"")))
        {
            return value[1..^1];
        }

        // Return as string
        return value;
    }

    private double? ParseDouble(string value)
    {
        if (double.TryParse(value.Trim(), out var d))
            return d;
        return null;
    }
}
