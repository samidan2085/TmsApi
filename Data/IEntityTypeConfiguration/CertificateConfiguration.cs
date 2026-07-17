using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;


namespace TmsApi.Data;
public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{

    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.ToTable("Certificat");
        builder.HasKey(c => c.Id);
         // Properties
        builder.Property(c => c.SerialNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.IssuedAt)
            .IsRequired();
        builder.HasIndex(c => c.SerialNumber)
             .IsUnique();
        // Student 1 -> Many Certificates
        builder.HasOne(c => c.Student)
            .WithMany(s => s.Certificates)
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
         builder.HasOne(c => c.Course)
            .WithMany(c => c.certificates)
            .HasForeignKey(c => c.CourseId)
            .OnDelete(DeleteBehavior.Restrict);    
    }
}
