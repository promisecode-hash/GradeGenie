using GradeGenie.Domain.Entities;
using Xunit;

namespace GradeGenie.Domain.Tests;

public sealed class StudentAcademicCalculationTests
{
    [Fact]
    public void Semester_calculates_weighted_gpa_from_credit_units_and_grades()
    {
        var semester = new Semester("First Semester", 2025);
        semester.AddCourse(new Course("CSC101", "Introduction to Computing", 3, Grade.A));
        semester.AddCourse(new Course("MTH101", "Calculus", 2, Grade.C));
        Assert.Equal(4.20m, semester.Gpa);
        Assert.Equal(5m, semester.TotalCreditUnits);
    }

    [Fact]
    public void Student_calculates_cgpa_as_a_weighted_average_across_semesters()
    {
        var first = new Semester("First Semester", 2025);
        first.AddCourse(new Course("CSC101", "Introduction to Computing", 3, Grade.A));
        var second = new Semester("Second Semester", 2025);
        second.AddCourse(new Course("MTH102", "Linear Algebra", 6, Grade.C));
        var student = new Student("auth-user-1", "Ada Lovelace");
        student.AddSemester(first);
        student.AddSemester(second);
        Assert.Equal(3.67m, student.Cgpa);
    }

    [Theory]
    [InlineData(Grade.A, 5)]
    [InlineData(Grade.F, 0)]
    public void Grade_scale_maps_letter_grade_to_five_point_value(Grade grade, decimal expected) => Assert.Equal(expected, GradePointScale.GetPoint(grade));
}
