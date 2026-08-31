using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> b)
    {
        b.HasKey(s => s.Id);

        b.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        b.HasMany(s => s.Enrollments)
            .WithOne(e => e.Student)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
       
        b.HasQueryFilter(s => !s.IsDeleted);

        b.Property<DateTime>("LastUpdated");

    }
}