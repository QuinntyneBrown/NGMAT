# Script Execution Service Roadmap

## Overview

This roadmap outlines the implementation phases for the Script Execution Service, which provides GMAT-compatible scripting.

---

## Phase 1: Parser

**Goal:** Script parsing and AST generation.

### Milestone 1.1: Project Setup
- [ ] Configure project structure (Core, Infrastructure, Api)
- [ ] Select parser library (ANTLR, Sprache, etc.)
- [ ] Set up Serilog logging
- [ ] Add health check endpoints

### Milestone 1.2: Lexer & Parser (MS-SE-1)
- [ ] Define grammar for GMAT syntax
- [ ] Implement lexer
- [ ] Implement parser
- [ ] Generate AST
- [ ] Create parse API endpoint

**Deliverables:**
- Script parser
- AST generation

---

## Phase 2: Basic Execution

**Goal:** Script execution engine.

### Milestone 2.1: Interpreter (MS-SE-2, MS-SE-4)
- [ ] Implement AST interpreter
- [ ] Support Create commands
- [ ] Support property assignment
- [ ] Support Propagate command
- [ ] Support Maneuver command

### Milestone 2.2: Job Management (MS-SE-3)
- [ ] Create execution job entity
- [ ] Track execution status
- [ ] Store output log
- [ ] Create status API endpoint

**Deliverables:**
- Script interpreter
- Job management

---

## Phase 3: Variables & Functions

**Goal:** Script programming features.

### Milestone 3.1: Variables (MS-SE-5)
- [ ] Implement variable storage
- [ ] Support arithmetic operations
- [ ] Type inference

### Milestone 3.2: Functions (MS-SE-6)
- [ ] Implement built-in math functions
- [ ] Implement coordinate functions
- [ ] Implement time functions

**Deliverables:**
- Variables
- Built-in functions

---

## Phase 4: Validation & Debugging

**Goal:** Script validation and debugging tools.

### Milestone 4.1: Validation (MS-SE-9)
- [ ] Implement semantic validation
- [ ] Check object existence
- [ ] Validate types
- [ ] Create validation endpoint

### Milestone 4.2: Debugging (MS-SE-7)
- [ ] Implement breakpoints
- [ ] Step execution
- [ ] Variable inspection

**Deliverables:**
- Script validation
- Debugging support

---

## Phase 5: Library

**Goal:** Script storage and sharing.

### Milestone 5.1: Script Library (MS-SE-8)
- [ ] Create script entity
- [ ] CRUD operations
- [ ] Sharing capabilities

**Deliverables:**
- Script library

---

## Timeline Summary

| Phase | Description | Priority |
|-------|-------------|----------|
| Phase 1 | Parser | P0 - Critical |
| Phase 2 | Basic Execution | P0 - Critical |
| Phase 3 | Variables & Functions | P1 - High |
| Phase 4 | Validation & Debugging | P1 - High |
| Phase 5 | Library | P2 - Medium |

---

## Success Metrics

- [ ] All 9 requirements (MS-SE-1 through MS-SE-9) implemented
- [ ] GMAT script compatibility tested
- [ ] 80%+ unit test coverage
