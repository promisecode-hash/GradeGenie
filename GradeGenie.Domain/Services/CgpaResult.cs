namespace GradeGenie.Domain.Services;

public sealed record CourseContribution(string Code, decimal GradePoint, decimal CreditUnits, decimal Contribution);

public sealed record CgpaResult(decimal Cgpa, IReadOnlyList<CourseContribution> Breakdown);
