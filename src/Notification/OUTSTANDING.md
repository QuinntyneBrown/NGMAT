# Notification Service - Outstanding Work

**Current Completion: 30%**  
**Priority: HIGH**

---

## Overview

The Notification Service has basic models and interfaces defined but lacks API endpoints, service implementations, and database persistence. This service is essential for user notifications, alerts, and real-time updates.

---

## Implemented Components

✅ **Core Models:**
- NotificationModels (types, channels, status enums)
- INotificationRepository interface

---

## Outstanding Requirements

### 🔴 High Priority (Core Functionality)

#### MS-NT-1: Send Email Notification
**Status:** ❌ Not Implemented

**Missing Components:**
- [ ] Email notification entity
- [ ] Email service implementation
- [ ] SMTP configuration
- [ ] Template support (Razor or Liquid)
- [ ] Asynchronous sending
- [ ] Delivery status tracking
- [ ] Retry on failure (exponential backoff)
- [ ] Email queue management
- [ ] REST API: POST /v1/notifications/email

**Implementation Tasks:**
1. Create Notification entity model
2. Create EmailNotification entity
3. Implement IEmailService interface
4. Add SMTP/SendGrid configuration
5. Create email templates
6. Implement retry logic with Polly
7. Create EmailController with endpoints
8. Add database context and repository

**Acceptance Criteria:**
- Send email to single recipient
- Support HTML and plain text
- Template variable substitution
- Delivery confirmation
- Failed emails moved to retry queue

---

#### MS-NT-2: Send Webhook Notification
**Status:** ❌ Not Implemented

**Missing Components:**
- [ ] Webhook entity
- [ ] Webhook service implementation
- [ ] HTTP client configuration
- [ ] Event payload serialization (JSON)
- [ ] HTTP headers configurable
- [ ] Retry on failure (exponential backoff)
- [ ] Signature for security (HMAC)
- [ ] Timeout configurable
- [ ] REST API: POST /v1/notifications/webhook

**Implementation Tasks:**
1. Create WebhookNotification entity
2. Implement IWebhookService interface
3. Configure HttpClient with Polly policies
4. Implement HMAC signature generation
5. Create WebhookController
6. Add webhook configuration storage
7. Implement delivery tracking

**Acceptance Criteria:**
- POST to external URL with payload
- HMAC signature in headers
- Retry 3 times with backoff
- 30 second timeout
- Delivery status recorded

---

#### MS-NT-3: Real-Time Notifications
**Status:** ❌ Not Implemented

**Missing Components:**
- [ ] SignalR hub implementation
- [ ] WebSocket support
- [ ] User-specific channels
- [ ] Notification types (info, warning, error, success)
- [ ] Client acknowledgment
- [ ] Fallback to polling if connection lost
- [ ] Connection state management
- [ ] Hub endpoint: /v1/notifications/stream

**Implementation Tasks:**
1. Install SignalR package
2. Create NotificationHub class
3. Configure SignalR in Program.cs
4. Implement user channel management
5. Create notification broadcasting service
6. Add connection state tracking
7. Implement fallback polling endpoint

**Acceptance Criteria:**
- Users receive real-time notifications
- Multiple clients per user supported
- Notifications delivered within 1 second
- Automatic reconnection on disconnect
- Polling fallback when WebSocket unavailable

---

#### Database Layer Implementation
**Status:** ❌ Not Implemented

**Missing Components:**
- [ ] NotificationDbContext
- [ ] Notification entity with EF configuration
- [ ] NotificationRepository implementation
- [ ] Database migrations
- [ ] Unit of work pattern
- [ ] Event store integration

**Implementation Tasks:**
1. Create Notification.Infrastructure project (if not exists)
2. Define Notification entity model
3. Create NotificationDbContext
4. Implement NotificationRepository
5. Configure entity relationships
6. Add database migrations
7. Configure dependency injection

**Entities Needed:**
```csharp
- Notification (base)
- EmailNotification
- WebhookNotification  
- InAppNotification
- NotificationTemplate
- NotificationPreference
- NotificationHistory
```

---

### 🟡 Medium Priority

#### MS-NT-4: Notification Preferences
**Status:** ❌ Not Implemented

**Missing Components:**
- [ ] User preferences entity
- [ ] Preferences service
- [ ] Enable/disable notification types
- [ ] Email vs push vs webhook selection
- [ ] Digest mode (batch notifications)
- [ ] Quiet hours configuration
- [ ] REST API: PUT /v1/notifications/preferences

**Implementation Tasks:**
1. Create NotificationPreference entity
2. Implement preferences service
3. Create preferences controller
4. Add preferences to database
5. Implement preference enforcement

---

#### MS-NT-5: Alert Rules
**Status:** ❌ Not Implemented

**Missing Components:**
- [ ] Alert rule entity
- [ ] Rule engine implementation
- [ ] Condition evaluation (fuel < threshold, etc.)
- [ ] Action triggers (send email, webhook)
- [ ] Evaluation frequency configuration
- [ ] Enable/disable rules
- [ ] REST API: POST /v1/notifications/alerts

**Implementation Tasks:**
1. Create AlertRule entity
2. Implement rule engine
3. Create rule evaluation service
4. Add scheduled background job
5. Create alert rule controller
6. Implement condition parsers

---

#### MS-NT-6: Notification History
**Status:** ❌ Not Implemented

**Missing Components:**
- [ ] Notification history tracking
- [ ] Query by date range, type, status
- [ ] Pagination support
- [ ] Delivery statistics
- [ ] Failed notification reports
- [ ] REST API: GET /v1/notifications/history

**Implementation Tasks:**
1. Add history tracking to notification service
2. Implement history query methods
3. Create history controller
4. Add filtering and pagination
5. Create delivery report endpoints

---

### 🔵 Low Priority

#### Additional Features
- [ ] SMS notifications (Twilio integration)
- [ ] Push notifications (mobile apps)
- [ ] Slack integration
- [ ] Teams integration
- [ ] Discord integration
- [ ] Notification batching/throttling
- [ ] Rich formatting (markdown, HTML)
- [ ] Attachment support for emails
- [ ] Notification analytics dashboard

---

## API Endpoints to Implement

| Method | Endpoint | Description | Priority |
|--------|----------|-------------|----------|
| POST | /v1/notifications/email | Send email | 🔴 High |
| POST | /v1/notifications/webhook | Send webhook | 🔴 High |
| WS | /v1/notifications/stream | Real-time stream | 🔴 High |
| GET | /v1/notifications/history | Get history | 🟡 Medium |
| PUT | /v1/notifications/preferences | Update preferences | 🟡 Medium |
| POST | /v1/notifications/alerts | Create alert rule | 🟡 Medium |
| GET | /v1/notifications/alerts | List alert rules | 🟡 Medium |
| PUT | /v1/notifications/alerts/{id} | Update alert rule | 🟡 Medium |
| DELETE | /v1/notifications/alerts/{id} | Delete alert rule | 🟡 Medium |

---

## Event Subscriptions Needed

The Notification service should subscribe to these events:

- **MissionCreatedEvent** → Send welcome email
- **PropagationCompletedEvent** → Notify user of completion
- **PropagationFailedEvent** → Alert user of failure
- **ManeuverCreatedEvent** → Confirmation notification
- **OptimizationCompletedEvent** → Results ready notification
- **ReportGeneratedEvent** → Report ready notification
- **ScriptExecutionCompletedEvent** → Script finished notification
- **FuelLowEvent** → Critical alert (custom rule)

---

## Technical Debt

1. **No service implementation** - Only interfaces and models
2. **No database layer** - Cannot persist notifications
3. **No API endpoints** - Cannot trigger notifications
4. **No email provider** - Cannot send emails
5. **No SignalR** - No real-time notifications
6. **No event subscriptions** - Not integrated with other services

---

## Implementation Recommendations

### Phase 1: Basic Infrastructure (Week 1)
1. Create notification entities
2. Set up database context
3. Implement repositories
4. Create basic API controller

### Phase 2: Email Notifications (Week 2)
1. Integrate SMTP/SendGrid
2. Create email templates
3. Implement email service
4. Add email endpoints
5. Add retry logic

### Phase 3: Webhooks (Week 3)
1. Implement webhook service
2. Add HMAC signing
3. Create webhook endpoints
4. Add delivery tracking

### Phase 4: Real-Time (Week 4)
1. Install and configure SignalR
2. Create notification hub
3. Implement broadcasting
4. Add client SDK support

### Phase 5: Advanced Features (Week 5)
1. Implement preferences
2. Create alert rules engine
3. Add notification history
4. Implement event subscriptions

---

## Dependencies

**Requires:**
- Redis (for message bus and caching)
- SMTP server or SendGrid account
- SignalR (for real-time notifications)
- Database (SQL Server or PostgreSQL)

**Integrates With:**
- All microservices (event subscriptions)
- Identity service (user information)
- ApiGateway (routing)

---

## Estimated Effort

- **High Priority:** 3-4 weeks (1 developer)
- **Medium Priority:** 2 weeks
- **Low Priority:** 1-2 weeks
- **Total:** 6-8 weeks

---

## Testing Requirements

- [ ] Unit tests for notification services
- [ ] Integration tests for email sending
- [ ] Integration tests for webhooks
- [ ] SignalR connection tests
- [ ] Event subscription tests
- [ ] Database repository tests
- [ ] End-to-end notification delivery tests

---

## Success Criteria

- ✅ Email notifications sent successfully
- ✅ Webhook notifications delivered with retries
- ✅ Real-time notifications via SignalR
- ✅ User preferences respected
- ✅ Alert rules triggered correctly
- ✅ Notification history queryable
- ✅ All notifications logged
- ✅ Failed notifications tracked and retried
