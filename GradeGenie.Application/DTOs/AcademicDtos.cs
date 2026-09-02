using GradeGenie.Domain.Entities;

namespace GradeGenie.Application.DTOs;

public sealed record CourseDto(Guid Id, string Code, string Title, decimal CreditUnits, Grade Grade, decimal GradePoint);
public sealed record SemesterDto(Guid Id, string Name, int AcademicYear, decimal Gpa, decimal TotalCreditUnits, IReadOnlyCollection<CourseDto> Courses);
public sealed record StudentCgpaDto(Guid StudentId, string FullName, decimal Cgpa, EducationInstitutionType InstitutionType, IReadOnlyCollection<SemesterDto> Semesters);
public sealed record SemesterInsightDto(Guid SemesterId, decimal Gpa, string Insight);
public sealed record StudentDto(Guid Id, string UserId, string FullName, EducationInstitutionType InstitutionType);
public sealed record CreateStudentRequest(string FullName, EducationInstitutionType InstitutionType = EducationInstitutionType.University);
public sealed record CreateSemesterRequest(string Name, int AcademicYear);
public sealed record CreateCourseRequest(string Code, string Title, decimal CreditUnits, Grade Grade);
public sealed record ConvertScaleRequest(decimal Value, GradeGenie.Domain.Services.GradingScale SourceScale, GradeGenie.Domain.Services.GradingScale TargetScale);
public sealed record ScaleConversionRequest(decimal Value, GradeGenie.Domain.Services.GradingScale SourceScale, GradeGenie.Domain.Services.GradingScale TargetScale);
public sealed record ScaleConversionResponse(decimal ConvertedValue, GradeGenie.Domain.Services.GradingScale SourceScale, GradeGenie.Domain.Services.GradingScale TargetScale);
public sealed record TargetGradeRequest(decimal CurrentCgpa, decimal TargetCgpa, decimal CompletedCreditUnits, decimal RemainingCreditUnits);
public sealed record TargetGradeResponse(decimal RequiredGradePoint, string RequiredLetterGrade);
public sealed record AcademicPlanRequest(decimal CurrentCgpa, decimal TargetCgpa, decimal CompletedCreditUnits, decimal RemainingCreditUnits);
public sealed record AcademicPlanResponse(decimal RequiredGradePoint, string RecommendedPriority, string Summary);
