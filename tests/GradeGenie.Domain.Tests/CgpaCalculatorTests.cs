using GradeGenie.Domain.Entities;
using GradeGenie.Domain.Services;
using Xunit;

namespace GradeGenie.Domain.Tests;

public sealed class CgpaCalculatorTests
{
    [Fact]
    public void Single_course_returns_its_grade_point_as_cgpa()
    {
        var course = new Course("CSC101", "Computing", 3, Grade.A);
        var result = CgpaCalculator.Calculate(new[] { course });
        Assert.Equal(5.00m, result.Cgpa);
        Assert.Single(result.Breakdown);
    }

    [Fact]
    public void Multiple_courses_calculate_weighted_cgpa()
    {
        var courses = new[]
        {
            new Course("CSC101", "Computing", 3, Grade.A),
            new Course("MTH101", "Calculus", 2, Grade.C)
        };
        var result = CgpaCalculator.Calculate(courses);
        Assert.Equal(4.20m, result.Cgpa);
        Assert.Equal(2, result.Breakdown.Count);
    }

    [Fact]
    public void Empty_course_list_returns_zero()
    {
        var result = CgpaCalculator.Calculate(Array.Empty<Course>());
        Assert.Equal(0m, result.Cgpa);
        Assert.Empty(result.Breakdown);
    }
}
