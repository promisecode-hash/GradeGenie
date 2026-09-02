using GradeGenie.Domain.Entities;
using GradeGenie.Domain.Services;
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
        var student = new Student("auth-user-1", "Ada Lovelace", EducationInstitutionType.University);
        student.AddSemester(first);
        student.AddSemester(second);
        Assert.Equal(3.67m, student.Cgpa);
        Assert.Equal(EducationInstitutionType.University, student.InstitutionType);
    }

    [Fact]
    public void Polytechnic_students_are_recorded_and_keep_their_selected_institution_type()
    {
        var student = new Student("auth-user-2", "Chinwe Okafor", EducationInstitutionType.Polytechnic);
        Assert.Equal(EducationInstitutionType.Polytechnic, student.InstitutionType);
    }

    [Fact]
    public void Polytechnic_students_calculate_cgpa_on_the_four_point_scale()
    {
        var semester = new Semester("First Semester", 2025);
        semester.AddCourse(new Course("CSC101", "Introduction to Computing", 3, Grade.A));
        var student = new Student("auth-user-3", "Bola Adebayo", EducationInstitutionType.Polytechnic);
        student.AddSemester(semester);
        Assert.Equal(4.00m, student.Cgpa);
        Assert.Equal(4.00m, GradePointScale.GetPoint(Grade.A, EducationInstitutionType.Polytechnic));
    }

    [Theory]
    [InlineData(Grade.A, 5)]
    [InlineData(Grade.F, 0)]
    public void Grade_scale_maps_letter_grade_to_five_point_value(Grade grade, decimal expected) => Assert.Equal(expected, GradePointScale.GetPoint(grade));

    [Fact]
    public void Target_grade_calculator_estimates_required_average_for_remaining_units()
    {
        var requiredPoint = TargetGradeCalculator.CalculateRequiredGradePoint(3.0m, 3.5m, 20m, 20m);
        Assert.Equal(4.00m, requiredPoint);
    }

    [Fact]
    public void Target_grade_calculator_returns_zero_when_no_remaining_units_exist()
    {
        var requiredPoint = TargetGradeCalculator.CalculateRequiredGradePoint(3.8m, 3.8m, 30m, 0m);
        Assert.Equal(0m, requiredPoint);
    }

    [Fact]
    public void Academic_plan_recommends_the_required_focus_for_a_target_cgpa()
    {
        var plan = AcademicPlanner.CreatePlan(2.8m, 3.5m, 40m, 20m);
        Assert.Equal(4.90m, plan.RequiredGradePoint);
        Assert.Contains("A", plan.RecommendedPriority, StringComparison.OrdinalIgnoreCase);
    }
}
