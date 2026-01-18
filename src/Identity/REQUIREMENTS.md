# Identity Service Requirements

## Overview

**Domain:** Authentication, authorization, and user management.

The Identity Service handles all aspects of user authentication, authorization, session management, and user profile operations for the NGMAT platform.

---

## Requirements

### MS-ID-1: User Registration

**Description:** Create new user account.

**Acceptance Criteria:**
- [ ] Email address required (unique)
- [ ] Password required (min 8 chars, complexity rules)
- [ ] Password hashing (bcrypt or Argon2)
- [ ] Email verification token sent
- [ ] Account inactive until verified
- [ ] UserRegisteredEvent published
- [ ] REST API: POST /v1/identity/register
- [ ] Returns HTTP 201

---

### MS-ID-2: User Login

**Description:** Authenticate user and issue token.

**Acceptance Criteria:**
- [ ] Email and password required
- [ ] Password verification
- [ ] Account active check
- [ ] JWT token issued (access + refresh)
- [ ] Token expiration configurable (default 1 hour)
- [ ] Refresh token stored in secure cookie
- [ ] Login attempt logging
- [ ] Rate limiting to prevent brute force
- [ ] REST API: POST /v1/identity/login
- [ ] Returns HTTP 200 with token

---

### MS-ID-3: Token Refresh

**Description:** Refresh access token using refresh token.

**Acceptance Criteria:**
- [ ] Refresh token required
- [ ] Validate refresh token
- [ ] Issue new access token
- [ ] Optionally rotate refresh token
- [ ] REST API: POST /v1/identity/refresh
- [ ] Returns HTTP 200 with new access token

---

### MS-ID-4: User Logout

**Description:** Invalidate user session.

**Acceptance Criteria:**
- [ ] Invalidate refresh token
- [ ] Blacklist access token (optional, expensive)
- [ ] REST API: POST /v1/identity/logout
- [ ] Returns HTTP 204

---

### MS-ID-5: Password Reset

**Description:** Allow user to reset forgotten password.

**Acceptance Criteria:**
- [ ] User requests reset via email
- [ ] Reset token sent to email (expire in 1 hour)
- [ ] User submits new password with token
- [ ] Password updated
- [ ] All sessions invalidated
- [ ] REST API: POST /v1/identity/password-reset/request
- [ ] REST API: POST /v1/identity/password-reset/confirm

---

### MS-ID-6: Email Verification

**Description:** Verify user email address.

**Acceptance Criteria:**
- [ ] Verification token sent on registration
- [ ] User clicks link with token
- [ ] Account activated
- [ ] REST API: GET /v1/identity/verify-email?token={token}
- [ ] Returns HTTP 200

---

### MS-ID-7: Multi-Factor Authentication (MFA)

**Description:** Two-factor authentication.

**Acceptance Criteria:**
- [ ] TOTP (Time-based One-Time Password)
- [ ] QR code generation for authenticator app
- [ ] Backup codes generated
- [ ] MFA challenge on login
- [ ] REST API: POST /v1/identity/mfa/enable
- [ ] REST API: POST /v1/identity/mfa/verify

---

### MS-ID-8: Role-Based Access Control (RBAC)

**Description:** Assign roles to users.

**Acceptance Criteria:**
- [ ] Roles: Admin, User, ReadOnly, etc.
- [ ] Permissions per role
- [ ] User assigned one or more roles
- [ ] Claims in JWT token
- [ ] Policy-based authorization in services
- [ ] REST API: POST /v1/identity/roles
- [ ] REST API: PUT /v1/identity/users/{id}/roles

---

### MS-ID-9: User Profile Management

**Description:** Update user profile information.

**Acceptance Criteria:**
- [ ] Display name
- [ ] Avatar URL
- [ ] Preferences (timezone, units)
- [ ] REST API: GET /v1/identity/profile
- [ ] REST API: PUT /v1/identity/profile

---

### MS-ID-10: OAuth2 / OpenID Connect

**Description:** Third-party authentication (Google, Microsoft, GitHub).

**Acceptance Criteria:**
- [ ] OAuth2 authorization code flow
- [ ] OIDC support
- [ ] External provider login
- [ ] Link external accounts to user
- [ ] REST API: GET /v1/identity/oauth/{provider}

---

### MS-ID-11: API Key Management

**Description:** Generate API keys for programmatic access.

**Acceptance Criteria:**
- [ ] Generate API key
- [ ] Key hashing before storage
- [ ] Key expiration date
- [ ] Revoke key
- [ ] List user's API keys
- [ ] REST API: POST /v1/identity/api-keys
- [ ] REST API: DELETE /v1/identity/api-keys/{id}

---

### MS-ID-12: Audit User Actions

**Description:** Log authentication and authorization events.

**Acceptance Criteria:**
- [ ] Login/logout events
- [ ] Failed login attempts
- [ ] Password changes
- [ ] Role changes
- [ ] API key usage
- [ ] Searchable audit log
- [ ] REST API: GET /v1/identity/audit

---

## API Endpoints Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /v1/identity/register | Register new user |
| POST | /v1/identity/login | User login |
| POST | /v1/identity/refresh | Refresh access token |
| POST | /v1/identity/logout | User logout |
| POST | /v1/identity/password-reset/request | Request password reset |
| POST | /v1/identity/password-reset/confirm | Confirm password reset |
| GET | /v1/identity/verify-email | Verify email address |
| POST | /v1/identity/mfa/enable | Enable MFA |
| POST | /v1/identity/mfa/verify | Verify MFA code |
| POST | /v1/identity/roles | Create role |
| PUT | /v1/identity/users/{id}/roles | Assign roles to user |
| GET | /v1/identity/profile | Get user profile |
| PUT | /v1/identity/profile | Update user profile |
| GET | /v1/identity/oauth/{provider} | OAuth login |
| POST | /v1/identity/api-keys | Create API key |
| DELETE | /v1/identity/api-keys/{id} | Revoke API key |
| GET | /v1/identity/audit | Get audit log |

---

## Events Published

| Event | Description |
|-------|-------------|
| UserRegisteredEvent | User account created |
| UserLoggedInEvent | User successfully logged in |
| UserLoggedOutEvent | User logged out |
| PasswordChangedEvent | User password changed |
| MfaEnabledEvent | MFA enabled for user |
| RoleAssignedEvent | Role assigned to user |
| ApiKeyCreatedEvent | API key generated |
| ApiKeyRevokedEvent | API key revoked |

---

## Dependencies

- **Event Bus**: Redis Pub/Sub for publishing events
- **Database**: User data storage (SQL Server recommended)
- **Email Service**: For sending verification and reset emails
