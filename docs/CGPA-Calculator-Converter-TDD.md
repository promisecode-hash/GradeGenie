# Technical Design Document (TDD)
## CGPA Calculator Converter

**Version:** 1.0
**Status:** Draft
**Derived from:** PRD v1.0, SRS v1.0

---

## 1. Purpose

This document describes how the CGPA Calculator Converter will be technically implemented to satisfy the requirements defined in the PRD and SRS. It covers system design, component breakdown, data model, algorithms, API contracts, and technical decisions/trade-offs.

## 2. System Overview

A stateless-by-default web application: a .NET Clean Architecture backend exposing calculation/conversion endpoints, and a frontend SPA consuming them. No database is required for v1 core functionality (FR-1 to FR-4); persistence is scoped only for the v2 history feature (FR-5).

```
┌─────────────┐      HTTPS/REST      ┌──────────────────────┐
│  Frontend    │ ───────────────────▶ │  CGPACalc.API        │
│  (SPA)       │ ◀─────────────────── │  (Controllers)       │
└─────────────┘                       └───────────┬──────────┘
                                                    │
                                       ┌────────────▼──────────┐
                                       │  CGPACalc.Application  │
                                       │  (Use Cases/Handlers)  │
                                       └────────────┬──────────┘
                                                    │
                                       ┌────────────▼──────────┐
                                       │  CGPACalc.Domain       │
                                       │  (Calculation Rules)   │
                                       └────────────────────────┘
                                                    │
                                       ┌────────────▼──────────┐
                                       │  CGPACalc.Infrastructure│
                                       │  (EF Core — v2 only)   │
                                       └────────────────────────┘
```

## 3. Component Design

### 3.1 Domain Layer
- `Grade` (value object): letter/numeric grade + resolved grade point for a given `GradingScale`
- `CreditUnit` (value object): positive numeric wrapper, validates > 0 at construction
- `GradingScale` (value object/enum-backed): defines valid grade→point mapping and valid numeric range (e.g. 5.0 scale: 0.0–5.0)
- `Course` (entity/DTO-adjacent): `Grade + CreditUnit + optional name`
- `CgpaCalculator` (domain service): pure function, `Calculate(IEnumerable<Course>, GradingScale) → CgpaResult`
- `ScaleConverter` (domain service): pure function, `Convert(decimal value, GradingScale source, GradingScale target) → decimal`

Both domain services are pure — no I/O, no side effects — satisfying NFR-6 (90%+ unit test coverage is realistic because there's nothing to mock).

### 3.2 Application Layer
- `CalculateCgpaCommand` / `CalculateCgpaHandler` → validates input (FluentValidation), delegates to `CgpaCalculator`, maps to `CgpaResultDto`
- `ConvertScaleCommand` / `ConvertScaleHandler` → validates range (FR-3.3), delegates to `ScaleConverter`
- Validators:
  - `CourseValidator`: credit unit > 0, grade valid for declared scale (FR-1.2, FR-1.3)
  - `ConvertScaleValidator`: input value within source scale's valid range (FR-3.3)

### 3.3 API Layer
- `CalculationController` → `POST /api/calculate/cgpa`
- `ConversionController` → `POST /api/convert/scale`
- `ExceptionHandlingMiddleware` → catches `ValidationException` → 400 with field errors; catches unhandled → 500 with generic ProblemDetails (NFR-5)
- No auth middleware required for v1 endpoints (NFR-2); reserved for v2 `HistoryController`

### 3.4 Infrastructure Layer (v2 scope only)
- `AppDbContext` with `Student`, `Semester`, `Course`, `GradeRecord` tables
- `IGradeHistoryRepository` implementation for save/retrieve
- Not built in v1 — interface defined now so Application layer doesn't need rework later

## 4. Data Model

**v1**: no persisted data. All request/response objects are transient DTOs.

**v2 (reserved)**:

```
Student            Semester            Course              GradeRecord
──────────         ──────────          ──────────          ──────────
Id (PK)            Id (PK)             Id (PK)              Id (PK)
Name               StudentId (FK)      SemesterId (FK)      CourseId (FK)
Email                                  Name                 Grade
                                       CreditUnit           GradePoint
```

## 5. Core Algorithm

**CGPA calculation** (FR-2.1, FR-2.2):

```
totalWeightedPoints = Σ (course.GradePoint × course.CreditUnit)
totalCreditUnits    = Σ (course.CreditUnit)
CGPA                = totalWeightedPoints / totalCreditUnits   (rounded to 2 dp, FR-2.4)
```

Cumulative CGPA across semesters applies the same formula over the full combined course set, not an average of per-semester GPAs (avoids weighting distortion when semesters have different total credit loads).

**Scale conversion** (FR-3.2):

```
convertedValue = (inputValue / sourceScale.Max) × targetScale.Max
```

Standard proportional formula, applied per the scale pair's documented convention (some institutions use non-linear letter-grade remapping instead of pure proportional scaling — this needs to be confirmed per the PRD's open question on which formulas to support, and encoded as a strategy per scale pair rather than hardcoded proportional math everywhere).

## 6. API Development & Architecture

### 6.1 API Project Structure

```
CGPACalc.API/
├── Controllers/
│   ├── CalculationController.cs
│   ├── ConversionController.cs
│   └── HistoryController.cs          (v2, stubbed/disabled in v1)
├── Contracts/
│   ├── Requests/
│   │   ├── CalculateCgpaRequest.cs
│   │   └── ConvertScaleRequest.cs
│   └── Responses/
│       ├── CgpaResultResponse.cs
│       └── ScaleConversionResponse.cs
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   └── RequestLoggingMiddleware.cs
├── Filters/
│   └── ValidationFilter.cs           (short-circuits to 400 before hitting handler)
├── Extensions/
│   ├── ServiceCollectionExtensions.cs (DI registration per layer)
│   └── ApplicationBuilderExtensions.cs (middleware pipeline wiring)
├── Mapping/
│   └── ApiMappingProfile.cs          (AutoMapper or manual: Request→Command, Result→Response)
├── Program.cs
└── appsettings.json / appsettings.Development.json
```

Controllers stay thin: bind request → map to Application command/query → dispatch → map result to response DTO. No business logic lives in this layer — that's the whole point of keeping Domain pure and Application orchestrating.

### 6.2 Request Pipeline (per call)

```
HTTP Request
   │
   ▼
[Routing] → matches controller/action by convention (api/[controller])
   │
   ▼
[Model Binding] → JSON → Request DTO (CalculateCgpaRequest)
   │
   ▼
[ValidationFilter] → runs FluentValidation validator for the Request DTO
   │                   → fails → short-circuit 400 with ProblemDetails, controller never invoked
   ▼
[Controller Action] → maps Request DTO → Application Command
   │
   ▼
[Application Handler] → re-validates domain invariants, calls Domain service
   │
   ▼
[Domain Service] → pure calculation, returns Result or throws domain exception
   │
   ▼
[Controller] → maps Result → Response DTO → 200 OK
   │
   ▼
[ExceptionHandlingMiddleware] → wraps the whole pipeline; catches anything unhandled → ProblemDetails
```

Validation happens twice by design: the `ValidationFilter` rejects malformed/obviously-invalid input fast (cheap, no domain objects constructed), and the Application/Domain layer enforces invariants that require business context (e.g. "is this grade valid for this specific scale") — satisfies NFR-3 without making the filter layer aware of business rules.

### 6.3 Controllers (concrete shape)

```csharp
[ApiController]
[Route("api/calculate")]
public class CalculationController : ControllerBase
{
    private readonly ISender _sender; // MediatR ISender, or direct handler DI

    [HttpPost("cgpa")]
    [ProducesResponseType(typeof(CgpaResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CgpaResultResponse>> CalculateCgpa(
        [FromBody] CalculateCgpaRequest request, CancellationToken ct)
    {
        var command = request.ToCommand();
        var result = await _sender.Send(command, ct);
        return Ok(result.ToResponse());
    }
}
```

Design choice: use MediatR (or an equivalent lightweight command dispatcher) so controllers depend only on `ISender`, not on every individual handler — keeps the API layer decoupled from Application internals and makes it trivial to add cross-cutting pipeline behaviors (validation, logging, timing) without touching controllers.

### 6.4 Routing & Versioning

- Convention-based routing: `api/[controller]/[action]` kept explicit per endpoint rather than fully attribute-free, so contract changes are visible in the controller itself
- API versioning via URL segment reserved for when v2 ships breaking changes: `api/v1/calculate/cgpa` — start the route prefix with `v1` from day one even though there's only one version, so v2 doesn't require a breaking route change later
- `HistoryController` routes reserved under `api/v1/history` but returns `501 Not Implemented` or is excluded from routing entirely until v2 (avoid shipping dead/misleading endpoints)

### 6.5 Middleware Pipeline (Program.cs order)

```
1. ExceptionHandlingMiddleware   (outermost — catches everything below)
2. HTTPS redirection / HSTS      (NFR-4)
3. CORS                          (allow the frontend's origin(s) only, not *)
4. RequestLoggingMiddleware      (Serilog structured request logging)
5. Routing
6. Rate limiting                 (basic fixed-window limiter on calc endpoints — public, unauthenticated, so cheap to abuse)
7. Authorization                 (no-op in v1; wired for v2 without route changes)
8. Endpoint execution
```

### 6.6 Dependency Injection Registration

- `Program.cs` calls `builder.Services.AddApplication()`, `.AddInfrastructure(config)`, `.AddApiServices()` — each an extension method defined in its own layer's assembly, keeping `Program.cs` a thin composition root
- Domain services (`CgpaCalculator`, `ScaleConverter`) registered as scoped/singleton (they're stateless, so singleton is safe and slightly cheaper)
- FluentValidation validators auto-registered via assembly scanning (`AddValidatorsFromAssemblyContaining<T>()`)
- `IGradeHistoryRepository` registered only when v2 persistence is enabled — kept behind a feature flag/config check so v1 deployments don't need a DB connection string at all

### 6.7 API Contracts

```
POST /api/v1/calculate/cgpa
Request:
{
  "scale": "5.0",
  "courses": [
    { "name": "MTH101", "grade": "A", "creditUnit": 3 }
  ]
}
Response 200:
{
  "cgpa": 4.33,
  "breakdown": [
    { "name": "MTH101", "grade": "A", "gradePoint": 5.0, "creditUnit": 3, "contribution": 15.0 }
  ]
}
Response 400: { "errors": { "courses[0].creditUnit": ["must be greater than 0"] } }

POST /api/v1/convert/scale
Request: { "value": 4.2, "sourceScale": "5.0", "targetScale": "4.0" }
Response 200: { "convertedValue": 3.36, "sourceScale": "5.0", "targetScale": "4.0" }
Response 400: { "errors": { "value": ["out of range for source scale 5.0"] } }
```

### 6.8 API Documentation & Discoverability

- Swagger/OpenAPI (Swashbuckle) enabled in all environments below production, disabled or auth-gated in production
- `[ProducesResponseType]` attributes on every action so the generated spec accurately reflects 200/400/500 shapes
- XML doc comments on request/response DTOs surfaced in Swagger UI for frontend consumers

### 6.9 Cross-Cutting API Concerns

| Concern | Approach |
|---|---|
| CORS | Explicit allow-list of frontend origin(s); no wildcard, even though endpoints are unauthenticated |
| Rate limiting | Fixed-window limiter (e.g. 60 req/min/IP) on calculate/convert endpoints — public and stateless means they're a cheap target for abuse otherwise |
| Request size limits | Cap course list length server-side (e.g. max 60 entries) even though the UI won't normally exceed it — prevents oversized payload abuse |
| Idempotency | Both endpoints are naturally idempotent (pure computation, no side effects) — no idempotency key needed for v1 |
| API contracts vs Domain | Request/Response DTOs never leak Domain types directly — mapping layer keeps API contract stable even if Domain internals change |

## 7. Error Handling Strategy

- All validation failures return `400` with field-level error details (satisfies FR-1.2, FR-1.3, FR-3.3)
- Domain-level invariant violations (e.g. constructing a `CreditUnit` with a negative value) throw domain exceptions, caught by middleware and translated to `400`
- Unexpected exceptions return `500` with a generic message; full details logged server-side only (NFR-5)

## 8. Non-Functional Requirement Mapping

| NFR | Design Decision |
|---|---|
| NFR-1 (< 500ms response) | Pure in-memory computation, no DB round-trip in v1 — response time is dominated by network, not compute |
| NFR-2 (stateless) | No session/cookie dependency for calculate/convert endpoints |
| NFR-3 (server-side validation) | FluentValidation runs regardless of client-side checks |
| NFR-4 (HTTPS only) | Enforced at hosting/reverse-proxy level (Kestrel + HSTS or behind Nginx/Cloudflare) |
| NFR-6 (90% test coverage on calc logic) | Domain services are pure functions — straightforward to hit high coverage with xUnit + FluentAssertions, including edge cases (zero credits, boundary grades, out-of-range conversion) |
| NFR-7 (responsive frontend) | Mobile-first CSS, tested at common breakpoints |

## 9. Testing Strategy

- **Domain unit tests**: exhaustive cases for `CgpaCalculator` (single course, multiple, zero credit unit edge case, rounding boundary) and `ScaleConverter` (each supported scale pair, in-range and out-of-range values)
- **Application tests**: handler-level tests verifying validation errors surface correctly
- **API integration tests**: end-to-end request/response shape validation for both endpoints, including 400 paths
- Coverage target: 90%+ on Domain and Application layers per NFR-6

## 10. Open Technical Decisions (carried from PRD Section 10)

- Confirm which scale pairs use proportional conversion vs. fixed lookup tables before implementing `ScaleConverter` — this changes whether it's a formula or a data-driven strategy pattern
- Decide if v1 ships with a hardcoded set of supported scales (enum-backed) or a lightweight configurable list — enum-backed is simpler and matches PRD's "start with common scales" scope
- Defer all v2 persistence/auth work until history feature is prioritized
                                                  here it is