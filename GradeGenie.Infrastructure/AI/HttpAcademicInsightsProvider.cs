using System.Net.Http.Headers;
using System.Net.Http.Json;
using GradeGenie.Domain.Entities;
using GradeGenie.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace GradeGenie.Infrastructure.AI;

public sealed class HttpAcademicInsightsProvider(HttpClient httpClient, IOptions<AiProviderOptions> options) : IAcademicInsightsProvider
{
    public async Task<string> GenerateSemesterInsightAsync(Student student, Semester semester, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.Endpoint) || string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            return GenerateLocalInsight(student, semester);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, settings.Endpoint)
        {
            Content = JsonContent.Create(new
            {
                model = settings.Model,
                student = student.FullName,
                semester = semester.Name,
                gpa = semester.Gpa,
                courses = semester.Courses.Select(course => new { course.Code, course.CreditUnits, grade = course.Grade.ToString() })
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<InsightResponse>(cancellationToken: cancellationToken);
        return result?.Insight ?? throw new InvalidOperationException("The AI provider returned no insight.");
    }

    private static string GenerateLocalInsight(Student student, Semester semester)
    {
        if (!semester.Courses.Any())
        {
            return $"{student.FullName}'s {semester.Name} semester is still open. Add at least one course to create a meaningful academic insight.";
        }

        var bestCourse = semester.Courses
            .OrderByDescending(course => course.GradePoint)
            .First();

        var weakestCourse = semester.Courses
            .OrderBy(course => course.GradePoint)
            .First();

        var gpaText = semester.Gpa >= 3.5m ? "strong" : semester.Gpa >= 2.5m ? "steady" : "needs attention";

        return $"{student.FullName}'s {semester.Name} semester is {gpaText} with a GPA of {semester.Gpa:F2}. The strongest result is {bestCourse.Code} at {bestCourse.Grade}, while {weakestCourse.Code} is the priority area for improvement. Focus on keeping the pace high in your next study cycle.";
    }

    private sealed record InsightResponse(string? Insight);
}
