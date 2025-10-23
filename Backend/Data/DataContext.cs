using ClinicExam.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicExam.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options) { }

        public DbSet<Clinic> Clinics { get; set; } = null!;
        public DbSet<Speciality> Specialities { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Doctor> Doctors { get; set; } = null!;
        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Clinic>().Property(x => x.Name).IsRequired().HasMaxLength(120);
            modelBuilder.Entity<Speciality>().Property(x => x.Name).IsRequired().HasMaxLength(120);
            modelBuilder.Entity<Category>().Property(x => x.Name).IsRequired().HasMaxLength(120);
            modelBuilder.Entity<Doctor>().Property(x => x.FirstName).IsRequired().HasMaxLength(80);
            modelBuilder.Entity<Doctor>().Property(x => x.LastName).IsRequired().HasMaxLength(80);
            modelBuilder.Entity<Patient>().Property(x => x.Email).IsRequired().HasMaxLength(200);

            modelBuilder.Entity<Clinic>().HasIndex(x => x.Name).IsUnique();
            modelBuilder.Entity<Speciality>().HasIndex(x => x.Name).IsUnique();
            modelBuilder.Entity<Category>().HasIndex(x => x.Name).IsUnique();
            modelBuilder.Entity<Patient>().HasIndex(x => x.Email).IsUnique();

            modelBuilder.Entity<Doctor>()
                .HasIndex(d => new { d.FirstName, d.LastName, d.ClinicId, d.SpecialityId })
                .IsUnique();

            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.Clinic)
                .WithMany(c => c.Doctors)
                .HasForeignKey(d => d.ClinicId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.Speciality)
                .WithMany(s => s.Doctors)
                .HasForeignKey(d => d.SpecialityId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Clinic)
                .WithMany(c => c.Appointments)
                .HasForeignKey(a => a.ClinicId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Category)
                .WithMany(cat => cat.Appointments)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}