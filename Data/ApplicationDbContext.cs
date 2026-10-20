using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace organizador_dietas_santo_tomas.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<DoctorPatient> DoctorPatients => Set<DoctorPatient>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.CreatedByDoctor)
            .WithMany()
            .HasForeignKey(u => u.CreatedByDoctorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<DoctorPatient>().HasKey(link => new { link.DoctorId, link.PatientId });
        builder.Entity<DoctorPatient>()
            .HasOne(link => link.Doctor)
            .WithMany(user => user.DoctorPatients)
            .HasForeignKey(link => link.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<DoctorPatient>()
            .HasOne(link => link.Patient)
            .WithMany(user => user.PatientDoctors)
            .HasForeignKey(link => link.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<DoctorPatient>().HasIndex(link => link.PatientId);
    }
}
