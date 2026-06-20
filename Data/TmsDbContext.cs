using Microsoft.EntityFrameworkCore;
namespace TmsApi.Entities;
public class TmsDbContext(DbContextOptions<TmsDbContext> options) : DbContext(options)
{
public DbSet<Student> Students => Set<Student>();
public DbSet<Course> Courses => Set<Course>();
public DbSet<Enrollment> Enrollments => Set<Enrollment>();
public DbSet<Certificate> Certificates => Set<Certificate>();
public DbSet<Assessment> Assessments => Set<Assessment>();
  protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Course>().HasKey(c => c.Id);
        modelBuilder.Entity<Student>().HasKey(s => s.Id);
        modelBuilder.Entity<Enrollment>().HasKey(e => e.Id);

        base.OnModelCreating(modelBuilder);
    }
}

