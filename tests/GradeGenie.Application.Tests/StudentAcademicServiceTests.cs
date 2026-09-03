using GradeGenie.Application.DTOs;
using GradeGenie.Application.Services;
using GradeGenie.Domain.Entities;
using GradeGenie.Domain.Interfaces;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using System;
using Xunit;

namespace GradeGenie.Application.Tests;

public sealed class StudentAcademicServiceTests
{
    [Fact]
    public async Task CreateStudent_calls_repository_and_returns_dto()
    {
        var repo = new Mock<IStudentRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var insights = new Mock<GradeGenie.Domain.Interfaces.IAcademicInsightsProvider>();
        var service = new StudentAcademicService(repo.Object, insights.Object);

        var dto = await service.CreateStudentAsync("user-1", new CreateStudentRequest("Ada Lovelace"));

        Assert.Equal("Ada Lovelace", dto.FullName);
        repo.Verify(r => r.AddAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddSemester_returns_null_when_student_not_found()
    {
        var repo = new Mock<IStudentRepository>();
        repo.Setup(r => r.GetWithSemestersForUserAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Student?)null);
        var insights = new Mock<GradeGenie.Domain.Interfaces.IAcademicInsightsProvider>();
        var service = new StudentAcademicService(repo.Object, insights.Object);

        var result = await service.AddSemesterAsync("user-1", Guid.NewGuid(), new CreateSemesterRequest("S1", 2026));
        Assert.Null(result);
    }
}
