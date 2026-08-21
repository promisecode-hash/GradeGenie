namespace GradeGenie.Domain.Entities;

public sealed class Student
{
    private readonly List<Semester> _semesters = [];
    private Student() { }

    public Student(string userId, string fullName)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User id is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Student name is required.", nameof(fullName));
        UserId = userId;
        FullName = fullName.Trim();
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserId { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public IReadOnlyCollection<Semester> Semesters => _semesters.AsReadOnly();
    public decimal Cgpa
    {
        get
        {
            var units = _semesters.Sum(semester => semester.TotalCreditUnits);
            return units == 0 ? 0m : decimal.Round(_semesters.Sum(semester => semester.Courses.Sum(course => course.QualityPoints)) / units, 2);
        }
    }

    public void AddSemester(Semester semester)
    {
        ArgumentNullException.ThrowIfNull(semester);
        _semesters.Add(semester);
    }
}
