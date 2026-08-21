namespace GradeGenie.Domain.Entities;

public sealed class Course
{
    private Course() { }

    public Course(string code, string title, decimal creditUnits, Grade grade)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Course code is required.", nameof(code));
        if (creditUnits <= 0) throw new ArgumentOutOfRangeException(nameof(creditUnits), "Credit units must be positive.");
        Code = code.Trim().ToUpperInvariant();
        Title = title?.Trim() ?? string.Empty;
        CreditUnits = creditUnits;
        Grade = grade;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid SemesterId { get; private set; }
    public string Code { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public decimal CreditUnits { get; private set; }
    public Grade Grade { get; private set; }
    public decimal GradePoint => GradePointScale.GetPoint(Grade);
    public decimal QualityPoints => CreditUnits * GradePoint;
}
