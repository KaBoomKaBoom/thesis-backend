# Thesis Backend - User Handling Microservice

## Table of contents
- [1. Project title](#1-project-title)
- [2. Project purpose](#2-project-purpose)
- [3. Technologies and instruments used in the project](#3-technologies-and-instruments-used-in-the-project)
- [4. Instructions to run the project](#4-instructions-to-run-the-project)
- [5. Endpoint description](#5-endpoint-description)
- [6. Model description](#6-model-description)
- [7. DTO description](#7-dto-description)
- [8. Project structure](#8-project-structure)

## 1. Project title
### Educational Web Platform for Assessing and Preparing Students for National Examinations 

## 2. Project purpose
This project is a backend API for a thesis platform that handles:
- user authentication and authorization,
- user profile management,
- test session creation and verification,
- test result persistence and retrieval.

The API is designed for role-based access (e.g., `student`, `admin`) and integrates with external services for OTP email flow and test verification.

## 3. Technologies and instruments used in the project
- **Runtime / Framework**: `.NET 10`, `ASP.NET Core Web API`
- **Database**: `PostgreSQL`
- **ORM**: `Entity Framework Core 10` (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- **Authentication**: `JWT Bearer`
- **Password hashing**: `BCrypt.Net-Next`
- **Caching / OTP temporary storage**: `Redis` (`Microsoft.Extensions.Caching.StackExchangeRedis`)
- **Email sending**: `MailKit` / `MimeKit`
- **API docs**: `OpenAPI` + `Scalar.AspNetCore`
- **Environment management**: `DotNetEnv`

## 4. Instructions to run the project
### Prerequisites
- .NET SDK 10
- PostgreSQL running and reachable
- Redis running and reachable

### Environment variables
Create a `.env` file in the API project root (`ThesisBackend`) and set at least:
- `DB_CONNECTION_STRING`
- `REDIS_CONNECTION_STRING` (optional, defaults to `localhost:6379`)
- `JWT_SECRET`
- `JWT_ISSUER`
- `JWT_AUDIENCE`
- `SMTP_SERVER`
- `SMTP_PORT`
- `SENDER_EMAIL`
- `SENDER_NAME`
- `SENDER_PASSWORD`
- `TEST_VERIFICATION_SERVICE_URL` (optional in service, defaults to `http://localhost:8070`)

### Restore and run
From the `ThesisBackend` directory:
1. `dotnet restore`
2. `dotnet build`
3. Apply EF migrations per context:
   - `dotnet ef database update --context UserContext`
   - `dotnet ef database update --context TestSessionContext`
4. Start API:
   - `dotnet run`

By default the API is bound to `http://0.0.0.0:8080`.

### Documentation and health endpoints
- Scalar docs (development): `http://localhost:8080/docs`
- OpenAPI JSON: `http://localhost:8080/openapi/v1.json`
- API health: `GET /api/auth/api-health`
- DB health: `GET /api/auth/db-health`

## 5. Endpoint description
### `AuthentificationController` (`/api/auth`)
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/auth/api-health` | No | Returns API running status |
| `GET` | `/api/auth/db-health` | No | Checks database connectivity |
| `POST` | `/api/auth/register` | No | Starts registration flow and sends OTP |
| `POST` | `/api/auth/verify-otp` | No | Validates OTP and completes user creation |
| `POST` | `/api/auth/login` | No | Authenticates user and returns JWT |
| `POST` | `/api/auth/refresh` | No | Refreshes JWT token |

### `UserController` (`/api/user`)
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/user/profile` | Yes (`Bearer`) | Returns current user profile |
| `PUT` | `/api/user/profile` | Yes (`Bearer`) | Updates current user profile |

### `TestSessionController` (`/api/testSession`)
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/testSession/testSession` | Yes (`student`, `admin`) | Creates a test session and triggers verification |
| `POST` | `/api/testSession/verifyTest/{sessionId}` | Yes (`student`, `admin`) | Verifies submitted test answers |

## 6. Model description
### User domain (`Models/UserModels`)
- `User`: Base user entity (`UserId`, names, email, password hash, role, profile fields).
- `Student`: Inherits `User`; adds `Institution`, `Grade`.
- `Teacher`: Inherits `User`; adds `Institution`, `Course`.
- `Parent`: Inherits `User`; adds `ChildrenIds`.
- `Admin`: Inherits `User` without extra fields.

### Test session domain (`Models/TestSessionModels`)
- `TestSession`: Stores a test attempt (`SessionId`, `UserId`, `TestId`, `TestTakenTime`, `Score`, `TestComponents`).
- `TestComponent`: One submitted answer item for a session (`Id`, `answer_id`, `question_id`).
- `TestResult`: Persisted verification output (`ResultId`, counts, score percentage, `VerifiedAt`, `DetailedResults`).
- `DetailedQuestionResult`: Per-question verification detail (`Id`, question, submitted/correct answer, correctness).

## 7. DTO description
### User DTOs (`DTOs/UserDTOs`)
- `UserToRegisterDTO`: Registration payload (`FirstName`, `LastName`, `Email`, `Password`, `Role`).
- `VerifyOTPDTO`: OTP verification payload (`Email`, `OTP`).
- `UserToLoginDTO`: Login payload (`Email`, `Password`).
- `UserToUpdateDTO`: Generic profile update payload.
- `StudentToUpdateDTO`: Student-specific profile update extension.
- `TeacherToUpdateDTO`: Teacher-specific profile update extension.
- `ParentToUpdateDTO`: Parent-specific profile update extension.
- `UserProfileDTO`: Profile response DTO returned by user endpoints.

### Test session DTOs (`DTOs/TestSessionDTOs`)
- `TestSessionToSave`: Incoming test submission payload (`TestId`, `TestComponents`).
- `TestComponentToSave`: Submitted answer item (`answer_id`, `question_id`) without client-side `Id`.
- `TestVerificationRequestDTO`: Payload sent to verification microservice.
- `AnswerSubmission`: Single answer item inside verification request.
- `TestVerificationResponseDTO`: Verification microservice response model.
- `QuestionResult`: Single question result from verification response.
- `TestResultDTO`: API-friendly result response model.
- `QuestionResultDTO`: Question-level result item for `TestResultDTO`.

## 8. Project structure
```text
thesis-backend/
├─ README.md
└─ ThesisBackend/
   ├─ Controllers/
   │  ├─ AuthentificationController.cs
   │  ├─ UserController.cs
   │  └─ TestSessionController.cs
   ├─ Services/
   │  ├─ AuthServices/
   │  │  ├─ UserService.cs
   │  │  ├─ OTPService.cs
   │  │  └─ EmailService.cs
   │  └─ TestSessionServices/
   │     └─ TestSessionService.cs
   ├─ Data/
   │  ├─ UserContext.cs
   │  └─ TestSessionContext.cs
   ├─ Models/
   │  ├─ UserModels/
   │  └─ TestSessionModels/
   ├─ DTOs/
   │  ├─ UserDTOs/
   │  └─ TestSessionDTOs/
   ├─ Helpers/
   │  └─ AuthHelpers/
   ├─ Migrations/
   │  ├─ (UserContext migrations)
   │  └─ TestSession/
   │     └─ (TestSessionContext migrations)
   ├─ Program.cs
   └─ ThesisBackend.csproj
```

