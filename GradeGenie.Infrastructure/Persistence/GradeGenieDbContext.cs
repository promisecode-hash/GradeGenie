using GradeGenie.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradeGenie.Infrastructure.Persistence;

public sealed class GradeGenieDbContext(DbContextOptions<GradeGenieDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Course> Courses => Set<Course>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(builder =>
        {
            builder.HasKey(student => student.Id);
            builder.Property(student => student.UserId).HasMaxLength(128).IsRequired();
            builder.HasIndex(student => student.UserId).IsUnique();
            builder.Property(student => student.FullName).HasMaxLength(200).IsRequired();
            builder.HasMany(student => student.Semesters).WithOne().HasForeignKey(semester => semester.StudentId).OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(student => student.Semesters).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Semester>(builder =>
        {
            builder.HasKey(semester => semester.Id);
            builder.Property(semester => semester.Name).HasMaxLength(60).IsRequired();
            builder.HasMany(semester => semester.Courses).WithOne().HasForeignKey(course => course.SemesterId).OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(semester => semester.Courses).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Course>(builder =>
        {
            builder.HasKey(course => course.Id);
            builder.Property(course => course.Code).HasMaxLength(30).IsRequired();
            builder.Property(course => course.Title).HasMaxLength(200);
            builder.Property(course => course.CreditUnits).HasPrecision(5, 2);
            builder.Property(course => course.Grade).HasConversion<string>().HasMaxLength(1);
        });
    }
}
