using GradeGenie.Domain.Services;

namespace GradeGenie.Domain.Entities;

public enum EducationInstitutionType
{
    University = 0,
    Polytechnic = 1
}

public sealed class Student
{
    private readonly List<Semester> _semesters = new();
    private Student() { }

    public Student(string userId, string fullName, EducationInstitutionType institutionType = EducationInstitutionType.University)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User id is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Student name is required.", nameof(fullName));
        UserId = userId;
        FullName = fullName.Trim();
        InstitutionType = institutionType;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserId { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public EducationInstitutionType InstitutionType { get; private set; }
    public IReadOnlyCollection<Semester> Semesters => _semesters.AsReadOnly();
    public decimal Cgpa
    {
        get
        {
            var courses = _semesters.SelectMany(s => s.Courses);
            var result = CgpaCalculator.Calculate(courses, InstitutionType);
            return result.Cgpa;
        }
    }

    public void AddSemester(Semester semester)
    {
        ArgumentNullException.ThrowIfNull(semester);
        _semesters.Add(semester);
    }
}
    