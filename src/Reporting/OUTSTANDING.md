# Reporting Service - Outstanding Work

**Current Completion: 50%**  
**Priority: HIGH**

---

## Overview

The Reporting Service has a comprehensive service implementation (738 lines) and API endpoints, but lacks database persistence, storage mechanisms, and export format implementations. The service can generate reports but cannot store or retrieve them.

---

## Implemented Components

✅ **Core Services:**
- ReportingService (738 lines, comprehensive logic)
- Report generation methods

✅ **Core Models:**
- Report definitions and data structures
- Report events

✅ **API Endpoints:**
- ReportingEndpoints (basic structure)

---

## Outstanding Requirements

### 🔴 High Priority (Critical Gaps)

#### Database Layer Implementation
**Status:** ❌ Not Implemented

**Missing Components:**
- [ ] ReportDbContext
- [ ] Report entity with EF configuration
- [ ] ReportRepository implementation
- [ ] ReportTemplate entity
- [ ] ReportHistory entity
- [ ] Database migrations
- [ ] Unit of work pattern

**Implementation Tasks:**
1. Create Report.Infrastructure project files
2. Define Report entity model
3. Create ReportDbContext
4. Implement ReportRepository
5. Add ReportTemplate repository
6. Configure entity relationships
7. Add database migrations
8. Wire up dependency injection

**Entities Needed:**
```csharp
- Report (id, name, type, status, created, etc.)
- ReportTemplate (id, name, format, content)
- ReportHistory (id, reportId, generatedDate, etc.)
- ReportSchedule (id, cronExpression, parameters)
- ReportFile (id, reportId, fileName, contentType, data)
```

---

#### MS-RP-1: Generate Mission Report (Storage)
**Status:** ⚠️ Service exists, no persistence

**Missing Components:**
- [ ] Report file storage (Azure Blob, file system)
- [ ] Report metadata persistence
- [ ] Job ID tracking
- [ ] Async report generation queue
- [ ] Status updates during generation
- [ ] Report file retrieval endpoint
- [ ] Report download endpoint

**Implementation Tasks:**
1. Implement file storage provider
2. Add report metadata to database
3. Create job tracking system
4. Implement async report queue
5. Add status update mechanism
6. Create download endpoints
7. Add file cleanup jobs

**Acceptance Criteria:**
- Generate report returns job ID
- Report saved to storage
- Metadata saved to database
- Status queryable by job ID
- Report downloadable when complete

---

#### MS-RP-2: Export State Vectors (File Formats)
**Status:** ⚠️ Service logic exists, no file generation

**Missing Components:**
- [ ] CSV export implementation
- [ ] JSON export implementation
- [ ] XML export implementation
- [ ] File streaming for large datasets
- [ ] Coordinate system selection in export
- [ ] Column header customization
- [ ] Compression support (zip)

**Implementation Tasks:**
1. Implement CSV writer service
2. Implement JSON serializer service
3. Implement XML writer service
4. Add streaming support for large files
5. Create file download controller
6. Add compression middleware
7. Implement format negotiation

**Acceptance Criteria:**
- Export returns downloadable file
- CSV, JSON, XML formats supported
- Large files streamed efficiently
- Coordinate system metadata included
- Files compressed if requested

---

#### MS-RP-3: Export Orbital Elements (File Formats)
**Status:** ⚠️ Service logic exists, no file generation

**Missing Components:**
- [ ] Keplerian elements formatter
- [ ] CSV export for elements
- [ ] JSON export for elements
- [ ] Custom column selection
- [ ] Epoch formatting options

**Implementation Tasks:**
1. Implement elements formatter
2. Add CSV export for elements
3. Add JSON export for elements
4. Create custom column selection
5. Add epoch format configuration

---

#### MS-RP-4: TLE Generation
**Status:** ⚠️ Partial logic, needs implementation

**Missing Components:**
- [ ] SGP4/SDP4 format generation
- [ ] Mean orbital elements calculation
- [ ] Checksum calculation
- [ ] NORAD catalog number management
- [ ] TLE validation
- [ ] TLE export endpoint

**Implementation Tasks:**
1. Implement mean elements calculator
2. Create TLE formatter
3. Add checksum algorithm
4. Implement validation
5. Create TLE export endpoint
6. Add catalog number assignment

**Acceptance Criteria:**
- Generate valid TLE format
- Checksums correct
- Compatible with SGP4 propagator
- Validation prevents invalid TLEs

---

#### MS-RP-5: Delta-V Budget Report (Formats)
**Status:** ⚠️ Logic exists, no PDF/Excel export

**Missing Components:**
- [ ] PDF generation (using QuestPDF or similar)
- [ ] Excel generation (using ClosedXML or EPPlus)
- [ ] Report template rendering
- [ ] Charts and graphs in reports
- [ ] Summary statistics
- [ ] Multi-page reports

**Implementation Tasks:**
1. Install PDF generation library
2. Install Excel generation library
3. Create report templates
4. Implement chart generation
5. Add template rendering
6. Create formatted report exports

**Acceptance Criteria:**
- PDF report with formatting
- Excel with multiple sheets
- Charts and graphs included
- Professional appearance
- Downloadable files

---

### 🟡 Medium Priority

#### MS-RP-6: Event Timeline Report
**Status:** ⚠️ Needs implementation

**Missing Components:**
- [ ] Event aggregation from multiple services
- [ ] Timeline sorting and formatting
- [ ] Event categorization
- [ ] PDF/HTML export
- [ ] Interactive timeline (HTML)

**Implementation Tasks:**
1. Query events from multiple services
2. Aggregate and sort events
3. Categorize events by type
4. Create timeline template
5. Implement PDF export
6. Create interactive HTML export

---

#### MS-RP-7: Conjunction Report
**Status:** ⚠️ Needs implementation

**Missing Components:**
- [ ] Close approach data aggregation
- [ ] Miss distance calculations
- [ ] Risk assessment
- [ ] Report formatting
- [ ] Export to PDF

**Implementation Tasks:**
1. Integrate with Propagation service
2. Calculate conjunction data
3. Implement risk assessment
4. Create report template
5. Add PDF export

---

#### MS-RP-8: Custom Report Template
**Status:** ❌ Not Implemented

**Missing Components:**
- [ ] Template storage (database)
- [ ] Template engine (Razor, Scriban, or Liquid)
- [ ] Variable substitution
- [ ] Template validation
- [ ] Template versioning
- [ ] Template CRUD endpoints

**Implementation Tasks:**
1. Choose template engine
2. Create template entity
3. Implement template storage
4. Add template CRUD endpoints
5. Implement variable substitution
6. Add template preview

**Acceptance Criteria:**
- Upload custom templates
- Variables substituted correctly
- Templates validated before save
- PDF/HTML output from templates

---

#### MS-RP-9: Scheduled Reports
**Status:** ❌ Not Implemented

**Missing Components:**
- [ ] Report schedule entity
- [ ] Cron expression parser
- [ ] Background job scheduler (Hangfire or Quartz)
- [ ] Email delivery integration
- [ ] Webhook delivery integration
- [ ] Schedule CRUD endpoints
- [ ] Job execution tracking

**Implementation Tasks:**
1. Install Hangfire or Quartz.NET
2. Create ReportSchedule entity
3. Implement cron parser
4. Create background job
5. Integrate with Notification service
6. Add schedule endpoints
7. Implement job monitoring

**Acceptance Criteria:**
- Schedule reports with cron expressions
- Reports generated on schedule
- Delivered via email or webhook
- Failed jobs retried
- Job history tracked

---

### 🔵 Low Priority

#### Additional Features
- [ ] Report favorites/bookmarks
- [ ] Report sharing with other users
- [ ] Report comparison (diff reports)
- [ ] Report versioning
- [ ] Report annotations/comments
- [ ] Report access control
- [ ] Report usage analytics
- [ ] Watermarks on PDF reports
- [ ] Digital signatures for reports

---

## API Endpoints to Implement

| Method | Endpoint | Description | Status |
|--------|----------|-------------|--------|
| POST | /v1/reports/mission/{id} | Generate mission report | ⚠️ Partial |
| GET | /v1/reports/{id} | Get report metadata | ❌ Missing |
| GET | /v1/reports/{id}/download | Download report file | ❌ Missing |
| GET | /v1/reports/{id}/status | Get generation status | ❌ Missing |
| GET | /v1/reports/export/states/{id} | Export state vectors | ⚠️ Partial |
| GET | /v1/reports/export/elements/{id} | Export elements | ⚠️ Partial |
| GET | /v1/reports/tle/{id} | Generate TLE | ⚠️ Partial |
| GET | /v1/reports/delta-v/{id} | Delta-V budget | ⚠️ Partial |
| GET | /v1/reports/timeline/{id} | Event timeline | ❌ Missing |
| GET | /v1/reports/conjunction/{id} | Conjunction report | ❌ Missing |
| POST | /v1/reports/custom | Custom template report | ❌ Missing |
| POST | /v1/reports/schedule | Schedule report | ❌ Missing |
| GET | /v1/reports/schedule | List schedules | ❌ Missing |
| PUT | /v1/reports/schedule/{id} | Update schedule | ❌ Missing |
| DELETE | /v1/reports/schedule/{id} | Delete schedule | ❌ Missing |

---

## Technical Debt

1. **No database persistence** - Reports not stored
2. **No file storage** - Generated reports not saved
3. **No export formats** - PDF/Excel generation missing
4. **No streaming** - Large files may cause memory issues
5. **No job tracking** - Cannot query report status
6. **No scheduled reports** - No automation capability

---

## Implementation Recommendations

### Phase 1: Database Layer (Week 1)
1. Create entity models
2. Set up DbContext
3. Implement repositories
4. Add migrations

### Phase 2: File Storage (Week 2)
1. Implement file storage provider
2. Add blob/file system support
3. Create download endpoints
4. Add file cleanup jobs

### Phase 3: Export Formats (Week 3)
1. Implement PDF generation
2. Implement Excel generation
3. Add CSV/JSON/XML exports
4. Create TLE formatter

### Phase 4: Advanced Features (Week 4)
1. Add custom templates
2. Implement scheduled reports
3. Add report history
4. Integrate with Notification service

---

## Dependencies

**Requires:**
- Database (SQL Server or PostgreSQL)
- File storage (Azure Blob or file system)
- PDF library (QuestPDF, PdfSharpCore)
- Excel library (ClosedXML, EPPlus)
- Background job scheduler (Hangfire, Quartz.NET)
- Template engine (Razor, Scriban, Liquid)

**Integrates With:**
- All microservices (data sources)
- Notification service (delivery)
- Storage service (file storage)

---

## Estimated Effort

- **High Priority:** 3-4 weeks (1 developer)
- **Medium Priority:** 2 weeks
- **Low Priority:** 1 week
- **Total:** 6-7 weeks

---

## Testing Requirements

- [ ] Unit tests for report generation
- [ ] Integration tests for file storage
- [ ] PDF/Excel generation tests
- [ ] Template rendering tests
- [ ] Scheduled job tests
- [ ] Performance tests for large reports
- [ ] End-to-end report workflow tests

---

## Success Criteria

- ✅ Reports stored in database
- ✅ Files saved to storage
- ✅ PDF/Excel exports working
- ✅ CSV/JSON/XML exports working
- ✅ TLE generation valid
- ✅ Custom templates supported
- ✅ Scheduled reports executing
- ✅ Large reports streamed efficiently
- ✅ Reports downloadable
