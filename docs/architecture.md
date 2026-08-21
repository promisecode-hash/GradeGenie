# GradeGenie backend architecture

## Dependency rule

`Api -> Application -> Domain` and `Infrastructure -> Domain`. All production projects live at the repository root. The Domain project has no package or project references. The API is the composition root: it selects SQLite/EF Core and the HTTP-based AI provider.

## Layer responsibilities

| Layer | Owns | Must not know about |
| --- | --- | --- |
| Domain | Student, Semester, Course, Grade; five-point grade mapping; GPA and CGPA calculation; repository/provider contracts | EF Core, HTTP, ASP.NET Core |
| Application | Use-case orchestration and response DTOs | Controllers, database implementation, AI vendor details |
| Infrastructure | EF Core `DbContext`, entity mappings, repository implementation, HTTP AI adapter | API routing and authorization |
| API | JWT middleware, controller endpoints, dependency registration, HTTP status codes | GPA mathematics and persistence queries |

## First vertical slice

`GET /api/students/{studentId}/cgpa` delegates to `IStudentAcademicService`, which loads the student through the Domain repository port. The Domain entity computes CGPA from all course quality points and credit units. The controller only translates the result to HTTP.

`POST /api/students/{studentId}/semesters/{semesterId}/insight` uses the same service and calls the Domain AI-provider port. Replacing the provider only means replacing its Infrastructure adapter and configuration.

## Next implementation slices

1. Add authenticated student ownership checks using the JWT subject (`UserId`).
2. Add commands/endpoints to create students, semesters, and courses.
3. Create the first EF Core migration from the API startup project.
4. Add domain unit tests for grade mapping, zero-credit cases, GPA, and cumulative weighted CGPA.
