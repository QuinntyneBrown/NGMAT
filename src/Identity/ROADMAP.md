# Identity Service Roadmap

## Overview

This roadmap outlines the implementation phases for the Identity Service, which provides authentication, authorization, and user management for NGMAT.

---

## Phase 1: Core Authentication (Foundation)

**Goal:** Basic user registration and login functionality.

### Milestone 1.1: Project Setup
- [ ] Configure project structure (Core, Infrastructure, Api)
- [ ] Set up Entity Framework Core with SQL Server
- [ ] Configure dependency injection
- [ ] Set up Serilog logging
- [ ] Configure Redis Pub/Sub connection
- [ ] Add health check endpoints

### Milestone 1.2: User Registration (MS-ID-1)
- [ ] Create User entity and database schema
- [ ] Implement password hashing (Argon2 or bcrypt)
- [ ] Create registration endpoint
- [ ] Add email uniqueness validation
- [ ] Publish UserRegisteredEvent

### Milestone 1.3: User Login (MS-ID-2)
- [ ] Implement JWT token generation
- [ ] Create login endpoint with password verification
- [ ] Implement refresh token mechanism
- [ ] Add login attempt logging
- [ ] Configure token expiration settings

### Milestone 1.4: Token Management (MS-ID-3, MS-ID-4)
- [ ] Implement token refresh endpoint
- [ ] Implement logout with token invalidation
- [ ] Add refresh token rotation

**Deliverables:**
- Working registration and login flow
- JWT-based authentication
- Unit tests for all endpoints

---

## Phase 2: Account Security

**Goal:** Password reset, email verification, and MFA.

### Milestone 2.1: Email Verification (MS-ID-6)
- [ ] Create email verification token generation
- [ ] Implement verification endpoint
- [ ] Integrate with notification service for email sending
- [ ] Add account activation flow

### Milestone 2.2: Password Reset (MS-ID-5)
- [ ] Create password reset request endpoint
- [ ] Generate secure reset tokens with expiration
- [ ] Implement password reset confirmation
- [ ] Invalidate all sessions on password change

### Milestone 2.3: Multi-Factor Authentication (MS-ID-7)
- [ ] Implement TOTP generation
- [ ] Create QR code generation for authenticator apps
- [ ] Generate backup codes
- [ ] Add MFA challenge to login flow
- [ ] Create MFA enable/disable endpoints

**Deliverables:**
- Complete account security features
- Email verification flow
- MFA support with TOTP

---

## Phase 3: Authorization & RBAC

**Goal:** Role-based access control and permissions.

### Milestone 3.1: Role Management (MS-ID-8)
- [ ] Create Role and Permission entities
- [ ] Implement role CRUD operations
- [ ] Define default roles (Admin, User, ReadOnly)
- [ ] Create role assignment endpoints

### Milestone 3.2: Claims & Policies
- [ ] Add role claims to JWT tokens
- [ ] Implement policy-based authorization
- [ ] Create authorization middleware
- [ ] Document authorization policies for other services

**Deliverables:**
- Complete RBAC system
- Role management API
- Authorization middleware

---

## Phase 4: User Profile & Preferences

**Goal:** User profile management and preferences.

### Milestone 4.1: Profile Management (MS-ID-9)
- [ ] Create profile entity with extended user data
- [ ] Implement profile get/update endpoints
- [ ] Add avatar URL support
- [ ] Store user preferences (timezone, units)

### Milestone 4.2: User Settings
- [ ] Create settings schema
- [ ] Implement settings persistence
- [ ] Add default settings configuration

**Deliverables:**
- User profile API
- Preferences storage

---

## Phase 5: External Authentication

**Goal:** OAuth2 and OpenID Connect integration.

### Milestone 5.1: OAuth2 Integration (MS-ID-10)
- [ ] Configure OAuth2 authorization code flow
- [ ] Implement Google authentication
- [ ] Implement Microsoft authentication
- [ ] Implement GitHub authentication

### Milestone 5.2: Account Linking
- [ ] Allow linking external accounts to existing users
- [ ] Handle account merging scenarios
- [ ] Add external account management endpoints

**Deliverables:**
- Social login support
- Account linking functionality

---

## Phase 6: API Keys & Programmatic Access

**Goal:** API key management for service-to-service and programmatic access.

### Milestone 6.1: API Key Management (MS-ID-11)
- [ ] Create API key entity
- [ ] Implement key generation with hashing
- [ ] Add key expiration support
- [ ] Create key CRUD endpoints
- [ ] Implement API key authentication middleware

**Deliverables:**
- API key management system
- Alternative authentication method for automation

---

## Phase 7: Audit & Compliance

**Goal:** Complete audit trail and compliance features.

### Milestone 7.1: Audit Logging (MS-ID-12)
- [ ] Create audit log entity
- [ ] Log all authentication events
- [ ] Log authorization decisions
- [ ] Implement audit query endpoint
- [ ] Add retention policies

### Milestone 7.2: Security Hardening
- [ ] Implement rate limiting
- [ ] Add brute force protection
- [ ] Configure CORS policies
- [ ] Security audit and penetration testing

**Deliverables:**
- Complete audit trail
- Security hardened service

---

## Timeline Summary

| Phase | Description | Priority |
|-------|-------------|----------|
| Phase 1 | Core Authentication | P0 - Critical |
| Phase 2 | Account Security | P0 - Critical |
| Phase 3 | Authorization & RBAC | P0 - Critical |
| Phase 4 | User Profile | P1 - High |
| Phase 5 | External Authentication | P2 - Medium |
| Phase 6 | API Keys | P2 - Medium |
| Phase 7 | Audit & Compliance | P1 - High |

---

## Technical Dependencies

- **StackExchange.Redis** - Redis Pub/Sub client
- **Microsoft.AspNetCore.Authentication.JwtBearer** - JWT authentication
- **Microsoft.AspNetCore.Identity** - Identity framework (optional)
- **BCrypt.Net-Next** or **Konscious.Security.Cryptography** - Password hashing
- **OtpNet** - TOTP implementation
- **QRCoder** - QR code generation for MFA

---

## Success Metrics

- [ ] All 12 requirements (MS-ID-1 through MS-ID-12) implemented
- [ ] 80%+ unit test coverage
- [ ] All API endpoints documented in OpenAPI
- [ ] Security audit passed
- [ ] Performance: < 100ms average response time for auth endpoints
