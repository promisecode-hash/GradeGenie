namespace GradeGenie.Domain.Entities;

public sealed class Semester
{
    private readonly List<Course> _courses = [];
    private Semester() { }

    public Semester(string name, int academicYear)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Semester name is required.", nameof(name));
        if (academicYear < 1900) throw new ArgumentOutOfRangeException(nameof(academicYear));
        Name = name.Trim();
        AcademicYear = academicYear;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid StudentId { get; private set; }
    public string Name { get; private set; } = null!;
    public int AcademicYear { get; private set; }
    public IReadOnlyCollection<Course> Courses => _courses.AsReadOnly();
    public decimal TotalCreditUnits => _courses.Sum(course => course.CreditUnits);
    public decimal Gpa => TotalCreditUnits == 0 ? 0m : decimal.Round(_courses.Sum(course => course.QualityPoints) / TotalCreditUnits, 2);

    public void AddCourse(Course course)
    {
        ArgumentNullException.ThrowIfNull(course);
        if (_courses.Any(existing => existing.Code == course.Code)) throw new InvalidOperationException("A course can only appear once in a semester.");
        _courses.Add(course);
    }
}
