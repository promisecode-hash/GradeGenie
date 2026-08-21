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
        if (string.IsNullOrWhiteSpace(settings.Endpoint) || string.IsNullOrWhiteSpace(settings.ApiKey)) throw new InvalidOperationException("The AI provider is not configured.");
        using var request = new HttpRequestMessage(HttpMethod.Post, settings.Endpoint)
        {
            Content = JsonContent.Create(new { model = settings.Model, student = student.FullName, semester = semester.Name, gpa = semester.Gpa,
                courses = semester.Courses.Select(course => new { course.Code, course.CreditUnits, grade = course.Grade.ToString() }) })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<InsightResponse>(cancellationToken: cancellationToken);
        return result?.Insight ?? throw new InvalidOperationException("The AI provider returned no insight.");
    }
    private sealed record InsightResponse(string? Insight);
}
