using GradeGenie.Domain.Entities;
using Xunit;

namespace GradeGenie.Domain.Tests;

public sealed class SemesterInstitutionAwareTests
{
    [Fact]
    public void GpaFor_returns_five_point_for_university()
    {
        var semester = new Semester("First Semester", 2026);
        semester.AddCourse(new Course("CSC101", "Computing", 3, Grade.A));

        var gpa = semester.GpaFor(EducationInstitutionType.University);

        Assert.Equal(5.00m, gpa);
        Assert.Equal(5.00m, semester.Gpa); // baseline unchanged
    }

    [Fact]
    public void GpaFor_returns_four_point_for_polytechnic()
    {
        var semester = new Semester("First Semester", 2026);
        semester.AddCourse(new Course("CSC101", "Computing", 3, Grade.A));

        var gpa = semester.GpaFor(EducationInstitutionType.Polytechnic);

        Assert.Equal(4.00m, gpa);
        Assert.Equal(5.00m, semester.Gpa); // baseline unchanged
    }
}
