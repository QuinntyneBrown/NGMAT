using System.Text;
using System.Text.Json;
using Visualization.Core.Models;

namespace Visualization.Core.Services;

/// <summary>
/// Service for exporting 3D visualization scenes to various formats
/// </summary>
public sealed class SceneExportService
{
    private const double EarthRadiusKm = 6378.137;
    private const int SphereSegments = 32;
    private const int SphereRings = 16;
    private const double OrbitLineWidth = 0.01; // Normalized

    /// <summary>
    /// Export orbit data to the specified format
    /// </summary>
    public SceneExportData ExportOrbit(
        OrbitPlotData orbitData,
        ExportFormat format,
        bool includeCentralBody = true,
        double scaleFactor = 0.001) // Convert km to scene units
    {
        return format switch
        {
            ExportFormat.Gltf => ExportToGltf(orbitData, includeCentralBody, scaleFactor),
            ExportFormat.Obj => ExportToObj(orbitData, includeCentralBody, scaleFactor),
            _ => ExportToJson(orbitData)
        };
    }

    /// <summary>
    /// Export orbit data to glTF 2.0 format
    /// </summary>
    private SceneExportData ExportToGltf(OrbitPlotData orbitData, bool includeCentralBody, double scaleFactor)
    {
        var gltf = new GltfDocument();

        // Add scene
        gltf.Scenes.Add(new GltfScene { Nodes = new List<int> { 0 } });
        gltf.Scene = 0;

        var nodeIndex = 0;
        var meshIndex = 0;
        var accessorIndex = 0;
        var bufferViewIndex = 0;
        var bufferData = new List<byte>();

        // Add central body (Earth) if requested
        if (includeCentralBody)
        {
            var (sphereVertices, sphereIndices) = GenerateSphereMesh(
                orbitData.CentralBody.RadiusKm * scaleFactor,
                SphereSegments,
                SphereRings);

            // Add sphere vertices to buffer
            var sphereVertexOffset = bufferData.Count;
            foreach (var v in sphereVertices)
            {
                bufferData.AddRange(BitConverter.GetBytes((float)v.X));
                bufferData.AddRange(BitConverter.GetBytes((float)v.Y));
                bufferData.AddRange(BitConverter.GetBytes((float)v.Z));
            }

            // Add sphere indices to buffer
            var sphereIndexOffset = bufferData.Count;
            foreach (var idx in sphereIndices)
            {
                bufferData.AddRange(BitConverter.GetBytes((ushort)idx));
            }

            // Calculate bounds
            var (minPos, maxPos) = CalculateBounds(sphereVertices);

            // Buffer views for sphere
            gltf.BufferViews.Add(new GltfBufferView
            {
                Buffer = 0,
                ByteOffset = sphereVertexOffset,
                ByteLength = sphereVertices.Count * 12,
                Target = 34962 // ARRAY_BUFFER
            });
            var sphereVertexBufferView = bufferViewIndex++;

            gltf.BufferViews.Add(new GltfBufferView
            {
                Buffer = 0,
                ByteOffset = sphereIndexOffset,
                ByteLength = sphereIndices.Count * 2,
                Target = 34963 // ELEMENT_ARRAY_BUFFER
            });
            var sphereIndexBufferView = bufferViewIndex++;

            // Accessors for sphere
            gltf.Accessors.Add(new GltfAccessor
            {
                BufferView = sphereVertexBufferView,
                ByteOffset = 0,
                ComponentType = 5126, // FLOAT
                Count = sphereVertices.Count,
                Type = "VEC3",
                Min = new List<double> { minPos.X, minPos.Y, minPos.Z },
                Max = new List<double> { maxPos.X, maxPos.Y, maxPos.Z }
            });
            var sphereVertexAccessor = accessorIndex++;

            gltf.Accessors.Add(new GltfAccessor
            {
                BufferView = sphereIndexBufferView,
                ByteOffset = 0,
                ComponentType = 5123, // UNSIGNED_SHORT
                Count = sphereIndices.Count,
                Type = "SCALAR",
                Min = new List<double> { 0 },
                Max = new List<double> { sphereVertices.Count - 1 }
            });
            var sphereIndexAccessor = accessorIndex++;

            // Add earth material
            gltf.Materials.Add(new GltfMaterial
            {
                Name = "Earth",
                PbrMetallicRoughness = new GltfPbrMetallicRoughness
                {
                    BaseColorFactor = new List<double> { 0.2, 0.4, 0.8, 1.0 },
                    MetallicFactor = 0.0,
                    RoughnessFactor = 0.8
                }
            });
            var earthMaterialIndex = 0;

            // Add sphere mesh
            gltf.Meshes.Add(new GltfMesh
            {
                Name = orbitData.CentralBody.Name,
                Primitives = new List<GltfPrimitive>
                {
                    new GltfPrimitive
                    {
                        Attributes = new Dictionary<string, int> { { "POSITION", sphereVertexAccessor } },
                        Indices = sphereIndexAccessor,
                        Material = earthMaterialIndex,
                        Mode = 4 // TRIANGLES
                    }
                }
            });

            // Add earth node
            gltf.Nodes.Add(new GltfNode
            {
                Name = orbitData.CentralBody.Name,
                Mesh = meshIndex++
            });
            nodeIndex++;
        }

        // Add orbit path
        var orbitVertices = orbitData.Points.Select(p => new Vertex3D
        {
            X = p.X * scaleFactor,
            Y = p.Y * scaleFactor,
            Z = p.Z * scaleFactor
        }).ToList();

        if (orbitVertices.Count > 1)
        {
            // Add orbit vertices to buffer
            var orbitVertexOffset = bufferData.Count;
            foreach (var v in orbitVertices)
            {
                bufferData.AddRange(BitConverter.GetBytes((float)v.X));
                bufferData.AddRange(BitConverter.GetBytes((float)v.Y));
                bufferData.AddRange(BitConverter.GetBytes((float)v.Z));
            }

            // Calculate orbit bounds
            var (orbitMin, orbitMax) = CalculateBounds(orbitVertices);

            // Buffer view for orbit
            gltf.BufferViews.Add(new GltfBufferView
            {
                Buffer = 0,
                ByteOffset = orbitVertexOffset,
                ByteLength = orbitVertices.Count * 12,
                Target = 34962
            });
            var orbitBufferView = bufferViewIndex++;

            // Accessor for orbit
            gltf.Accessors.Add(new GltfAccessor
            {
                BufferView = orbitBufferView,
                ByteOffset = 0,
                ComponentType = 5126,
                Count = orbitVertices.Count,
                Type = "VEC3",
                Min = new List<double> { orbitMin.X, orbitMin.Y, orbitMin.Z },
                Max = new List<double> { orbitMax.X, orbitMax.Y, orbitMax.Z }
            });
            var orbitAccessor = accessorIndex++;

            // Add orbit material
            gltf.Materials.Add(new GltfMaterial
            {
                Name = "OrbitPath",
                PbrMetallicRoughness = new GltfPbrMetallicRoughness
                {
                    BaseColorFactor = new List<double> { 1.0, 0.8, 0.0, 1.0 },
                    MetallicFactor = 0.0,
                    RoughnessFactor = 1.0
                }
            });
            var orbitMaterialIndex = gltf.Materials.Count - 1;

            // Add orbit mesh (as line strip)
            gltf.Meshes.Add(new GltfMesh
            {
                Name = "OrbitPath",
                Primitives = new List<GltfPrimitive>
                {
                    new GltfPrimitive
                    {
                        Attributes = new Dictionary<string, int> { { "POSITION", orbitAccessor } },
                        Material = orbitMaterialIndex,
                        Mode = 3 // LINE_STRIP
                    }
                }
            });

            // Add orbit node
            gltf.Nodes.Add(new GltfNode
            {
                Name = "OrbitPath",
                Mesh = meshIndex++
            });

            // Update scene to include orbit node
            gltf.Scenes[0].Nodes.Add(nodeIndex++);
        }

        // Add spacecraft marker at current position (first point)
        if (orbitData.Points.Count > 0)
        {
            var firstPoint = orbitData.Points[0];
            var (spacecraftVerts, spacecraftIndices) = GenerateSphereMesh(
                50 * scaleFactor, // Small marker
                8, 4);

            var scVertexOffset = bufferData.Count;
            foreach (var v in spacecraftVerts)
            {
                bufferData.AddRange(BitConverter.GetBytes((float)v.X));
                bufferData.AddRange(BitConverter.GetBytes((float)v.Y));
                bufferData.AddRange(BitConverter.GetBytes((float)v.Z));
            }

            var scIndexOffset = bufferData.Count;
            foreach (var idx in spacecraftIndices)
            {
                bufferData.AddRange(BitConverter.GetBytes((ushort)idx));
            }

            var (scMin, scMax) = CalculateBounds(spacecraftVerts);

            gltf.BufferViews.Add(new GltfBufferView
            {
                Buffer = 0,
                ByteOffset = scVertexOffset,
                ByteLength = spacecraftVerts.Count * 12,
                Target = 34962
            });
            var scVertexBV = bufferViewIndex++;

            gltf.BufferViews.Add(new GltfBufferView
            {
                Buffer = 0,
                ByteOffset = scIndexOffset,
                ByteLength = spacecraftIndices.Count * 2,
                Target = 34963
            });
            var scIndexBV = bufferViewIndex++;

            gltf.Accessors.Add(new GltfAccessor
            {
                BufferView = scVertexBV,
                ByteOffset = 0,
                ComponentType = 5126,
                Count = spacecraftVerts.Count,
                Type = "VEC3",
                Min = new List<double> { scMin.X, scMin.Y, scMin.Z },
                Max = new List<double> { scMax.X, scMax.Y, scMax.Z }
            });
            var scVertexAcc = accessorIndex++;

            gltf.Accessors.Add(new GltfAccessor
            {
                BufferView = scIndexBV,
                ByteOffset = 0,
                ComponentType = 5123,
                Count = spacecraftIndices.Count,
                Type = "SCALAR",
                Min = new List<double> { 0 },
                Max = new List<double> { spacecraftVerts.Count - 1 }
            });
            var scIndexAcc = accessorIndex++;

            gltf.Materials.Add(new GltfMaterial
            {
                Name = "Spacecraft",
                PbrMetallicRoughness = new GltfPbrMetallicRoughness
                {
                    BaseColorFactor = new List<double> { 1.0, 0.2, 0.2, 1.0 },
                    MetallicFactor = 0.5,
                    RoughnessFactor = 0.3
                }
            });
            var scMaterialIndex = gltf.Materials.Count - 1;

            gltf.Meshes.Add(new GltfMesh
            {
                Name = "Spacecraft",
                Primitives = new List<GltfPrimitive>
                {
                    new GltfPrimitive
                    {
                        Attributes = new Dictionary<string, int> { { "POSITION", scVertexAcc } },
                        Indices = scIndexAcc,
                        Material = scMaterialIndex,
                        Mode = 4
                    }
                }
            });

            gltf.Nodes.Add(new GltfNode
            {
                Name = "Spacecraft",
                Mesh = meshIndex++,
                Translation = new List<double>
                {
                    firstPoint.X * scaleFactor,
                    firstPoint.Y * scaleFactor,
                    firstPoint.Z * scaleFactor
                }
            });

            gltf.Scenes[0].Nodes.Add(nodeIndex++);
        }

        // Add buffer
        gltf.Buffers.Add(new GltfBuffer
        {
            ByteLength = bufferData.Count
        });

        // Serialize glTF to JSON
        var gltfJson = SerializeGltf(gltf);
        var gltfBytes = Encoding.UTF8.GetBytes(gltfJson);

        // Create GLB (binary glTF)
        var glbData = CreateGlb(gltfBytes, bufferData.ToArray());

        return new SceneExportData
        {
            Format = ExportFormat.Gltf,
            FileName = $"orbit_{orbitData.SpacecraftId:N}_{DateTime.UtcNow:yyyyMMddHHmmss}.glb",
            MimeType = "model/gltf-binary",
            Data = glbData
        };
    }

    /// <summary>
    /// Export orbit data to OBJ format
    /// </summary>
    private SceneExportData ExportToObj(OrbitPlotData orbitData, bool includeCentralBody, double scaleFactor)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# NGMAT Orbit Visualization Export");
        sb.AppendLine($"# Generated: {DateTime.UtcNow:O}");
        sb.AppendLine($"# Spacecraft ID: {orbitData.SpacecraftId}");
        sb.AppendLine();

        var vertexOffset = 1; // OBJ indices are 1-based

        // Add central body if requested
        if (includeCentralBody)
        {
            sb.AppendLine($"# Central Body: {orbitData.CentralBody.Name}");
            sb.AppendLine($"o {orbitData.CentralBody.Name}");

            var (sphereVerts, sphereIndices) = GenerateSphereMesh(
                orbitData.CentralBody.RadiusKm * scaleFactor,
                SphereSegments,
                SphereRings);

            // Write vertices
            foreach (var v in sphereVerts)
            {
                sb.AppendLine($"v {v.X:F6} {v.Y:F6} {v.Z:F6}");
            }
            sb.AppendLine();

            // Write faces
            for (int i = 0; i < sphereIndices.Count; i += 3)
            {
                sb.AppendLine($"f {sphereIndices[i] + vertexOffset} {sphereIndices[i + 1] + vertexOffset} {sphereIndices[i + 2] + vertexOffset}");
            }
            sb.AppendLine();

            vertexOffset += sphereVerts.Count;
        }

        // Add orbit path
        if (orbitData.Points.Count > 1)
        {
            sb.AppendLine("# Orbit Path");
            sb.AppendLine("o OrbitPath");

            foreach (var point in orbitData.Points)
            {
                sb.AppendLine($"v {point.X * scaleFactor:F6} {point.Y * scaleFactor:F6} {point.Z * scaleFactor:F6}");
            }
            sb.AppendLine();

            // Write line segments
            sb.Append("l");
            for (int i = 0; i < orbitData.Points.Count; i++)
            {
                sb.Append($" {i + vertexOffset}");
            }
            // Close the orbit loop
            sb.AppendLine($" {vertexOffset}");
            sb.AppendLine();

            vertexOffset += orbitData.Points.Count;
        }

        // Add spacecraft marker
        if (orbitData.Points.Count > 0)
        {
            var firstPoint = orbitData.Points[0];
            sb.AppendLine("# Spacecraft Position");
            sb.AppendLine("o Spacecraft");

            var (scVerts, scIndices) = GenerateSphereMesh(50 * scaleFactor, 8, 4);

            foreach (var v in scVerts)
            {
                sb.AppendLine($"v {(v.X + firstPoint.X * scaleFactor):F6} {(v.Y + firstPoint.Y * scaleFactor):F6} {(v.Z + firstPoint.Z * scaleFactor):F6}");
            }
            sb.AppendLine();

            for (int i = 0; i < scIndices.Count; i += 3)
            {
                sb.AppendLine($"f {scIndices[i] + vertexOffset} {scIndices[i + 1] + vertexOffset} {scIndices[i + 2] + vertexOffset}");
            }
        }

        var objContent = sb.ToString();
        var objBytes = Encoding.UTF8.GetBytes(objContent);

        return new SceneExportData
        {
            Format = ExportFormat.Obj,
            FileName = $"orbit_{orbitData.SpacecraftId:N}_{DateTime.UtcNow:yyyyMMddHHmmss}.obj",
            MimeType = "model/obj",
            Data = objBytes
        };
    }

    /// <summary>
    /// Export orbit data to JSON format
    /// </summary>
    private SceneExportData ExportToJson(OrbitPlotData orbitData)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(orbitData, jsonOptions);

        return new SceneExportData
        {
            Format = ExportFormat.Json,
            FileName = $"orbit_{orbitData.SpacecraftId:N}_{DateTime.UtcNow:yyyyMMddHHmmss}.json",
            MimeType = "application/json",
            Data = jsonBytes
        };
    }

    /// <summary>
    /// Generate a UV sphere mesh
    /// </summary>
    private (List<Vertex3D> vertices, List<int> indices) GenerateSphereMesh(double radius, int segments, int rings)
    {
        var vertices = new List<Vertex3D>();
        var indices = new List<int>();

        // Generate vertices
        for (int ring = 0; ring <= rings; ring++)
        {
            var phi = Math.PI * ring / rings;
            var y = Math.Cos(phi) * radius;
            var ringRadius = Math.Sin(phi) * radius;

            for (int seg = 0; seg <= segments; seg++)
            {
                var theta = 2 * Math.PI * seg / segments;
                var x = Math.Cos(theta) * ringRadius;
                var z = Math.Sin(theta) * ringRadius;

                vertices.Add(new Vertex3D { X = x, Y = y, Z = z });
            }
        }

        // Generate indices
        for (int ring = 0; ring < rings; ring++)
        {
            for (int seg = 0; seg < segments; seg++)
            {
                var current = ring * (segments + 1) + seg;
                var next = current + segments + 1;

                // First triangle
                indices.Add(current);
                indices.Add(next);
                indices.Add(current + 1);

                // Second triangle
                indices.Add(current + 1);
                indices.Add(next);
                indices.Add(next + 1);
            }
        }

        return (vertices, indices);
    }

    /// <summary>
    /// Calculate bounding box for vertices
    /// </summary>
    private (Vertex3D min, Vertex3D max) CalculateBounds(List<Vertex3D> vertices)
    {
        if (vertices.Count == 0)
            return (new Vertex3D(), new Vertex3D());

        var min = new Vertex3D
        {
            X = vertices.Min(v => v.X),
            Y = vertices.Min(v => v.Y),
            Z = vertices.Min(v => v.Z)
        };

        var max = new Vertex3D
        {
            X = vertices.Max(v => v.X),
            Y = vertices.Max(v => v.Y),
            Z = vertices.Max(v => v.Z)
        };

        return (min, max);
    }

    /// <summary>
    /// Serialize glTF document to JSON
    /// </summary>
    private string SerializeGltf(GltfDocument gltf)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        return JsonSerializer.Serialize(gltf, options);
    }

    /// <summary>
    /// Create GLB (binary glTF) from JSON and binary data
    /// </summary>
    private byte[] CreateGlb(byte[] jsonChunk, byte[] binChunk)
    {
        // Pad JSON to 4-byte boundary
        var jsonPadding = (4 - (jsonChunk.Length % 4)) % 4;
        var paddedJsonLength = jsonChunk.Length + jsonPadding;

        // Pad binary to 4-byte boundary
        var binPadding = (4 - (binChunk.Length % 4)) % 4;
        var paddedBinLength = binChunk.Length + binPadding;

        // Calculate total length
        // Header: 12 bytes (magic + version + length)
        // JSON chunk header: 8 bytes (length + type)
        // Binary chunk header: 8 bytes (length + type)
        var totalLength = 12 + 8 + paddedJsonLength + 8 + paddedBinLength;

        using var ms = new MemoryStream(totalLength);
        using var writer = new BinaryWriter(ms);

        // GLB Header
        writer.Write(0x46546C67); // "glTF" magic
        writer.Write((uint)2);    // Version 2
        writer.Write((uint)totalLength);

        // JSON Chunk
        writer.Write((uint)paddedJsonLength);
        writer.Write(0x4E4F534A); // "JSON"
        writer.Write(jsonChunk);
        for (int i = 0; i < jsonPadding; i++)
            writer.Write((byte)0x20); // Space padding

        // Binary Chunk
        writer.Write((uint)paddedBinLength);
        writer.Write(0x004E4942); // "BIN\0"
        writer.Write(binChunk);
        for (int i = 0; i < binPadding; i++)
            writer.Write((byte)0x00);

        return ms.ToArray();
    }

    // Internal data structures for glTF

    private class Vertex3D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    private class GltfDocument
    {
        public GltfAsset Asset { get; set; } = new();
        public int Scene { get; set; }
        public List<GltfScene> Scenes { get; set; } = new();
        public List<GltfNode> Nodes { get; set; } = new();
        public List<GltfMesh> Meshes { get; set; } = new();
        public List<GltfMaterial> Materials { get; set; } = new();
        public List<GltfAccessor> Accessors { get; set; } = new();
        public List<GltfBufferView> BufferViews { get; set; } = new();
        public List<GltfBuffer> Buffers { get; set; } = new();
    }

    private class GltfAsset
    {
        public string Version { get; set; } = "2.0";
        public string Generator { get; set; } = "NGMAT Visualization Service";
    }

    private class GltfScene
    {
        public string? Name { get; set; }
        public List<int> Nodes { get; set; } = new();
    }

    private class GltfNode
    {
        public string? Name { get; set; }
        public int? Mesh { get; set; }
        public List<double>? Translation { get; set; }
        public List<double>? Rotation { get; set; }
        public List<double>? Scale { get; set; }
        public List<int>? Children { get; set; }
    }

    private class GltfMesh
    {
        public string? Name { get; set; }
        public List<GltfPrimitive> Primitives { get; set; } = new();
    }

    private class GltfPrimitive
    {
        public Dictionary<string, int> Attributes { get; set; } = new();
        public int? Indices { get; set; }
        public int? Material { get; set; }
        public int Mode { get; set; } = 4; // TRIANGLES by default
    }

    private class GltfMaterial
    {
        public string? Name { get; set; }
        public GltfPbrMetallicRoughness? PbrMetallicRoughness { get; set; }
        public bool DoubleSided { get; set; } = true;
    }

    private class GltfPbrMetallicRoughness
    {
        public List<double>? BaseColorFactor { get; set; }
        public double MetallicFactor { get; set; }
        public double RoughnessFactor { get; set; }
    }

    private class GltfAccessor
    {
        public int BufferView { get; set; }
        public int ByteOffset { get; set; }
        public int ComponentType { get; set; }
        public int Count { get; set; }
        public string Type { get; set; } = "SCALAR";
        public List<double>? Min { get; set; }
        public List<double>? Max { get; set; }
    }

    private class GltfBufferView
    {
        public int Buffer { get; set; }
        public int ByteOffset { get; set; }
        public int ByteLength { get; set; }
        public int? ByteStride { get; set; }
        public int? Target { get; set; }
    }

    private class GltfBuffer
    {
        public int ByteLength { get; set; }
        public string? Uri { get; set; }
    }
}
