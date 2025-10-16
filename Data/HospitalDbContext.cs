using Microsoft.EntityFrameworkCore;
using PruebaCSharp.Models;

namespace PruebaCSharp.Data
{
    public class HospitalDbContext : DbContext
    {
        public HospitalDbContext(DbContextOptions<HospitalDbContext> options) : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Patient entity
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Document).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                
                // Unique constraint on document
                entity.HasIndex(e => e.Document).IsUnique();
            });

            // Configure Doctor entity
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Document).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Specialty).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                
                // Unique constraint on document
                entity.HasIndex(e => e.Document).IsUnique();
            });

            // Configure Appointment entity
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AppointmentDate).IsRequired();
                entity.Property(e => e.AppointmentTime).IsRequired();
                entity.Property(e => e.Status).HasDefaultValue(AppointmentStatus.Scheduled);
                entity.Property(e => e.Notes).HasMaxLength(500).IsRequired(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                // Foreign key relationships
                entity.HasOne(e => e.Patient)
                      .WithMany(p => p.Appointments)
                      .HasForeignKey(e => e.PatientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Doctor)
                      .WithMany(d => d.Appointments)
                      .HasForeignKey(e => e.DoctorId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Unique constraint to prevent double booking
                entity.HasIndex(e => new { e.DoctorId, e.AppointmentDate, e.AppointmentTime })
                      .IsUnique();
                
                entity.HasIndex(e => new { e.PatientId, e.AppointmentDate, e.AppointmentTime })
                      .IsUnique();
            });

            // Configure EmailLog entity
            modelBuilder.Entity<EmailLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RecipientEmail).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Body).IsRequired();
                entity.Property(e => e.Status).HasDefaultValue(EmailStatus.Failed).HasSentinel(EmailStatus.Sent);
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
                entity.Property(e => e.SentAt).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                // Foreign key relationship
                entity.HasOne(e => e.Appointment)
                      .WithMany(a => a.EmailLogs)
                      .HasForeignKey(e => e.AppointmentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data - DISABLED to prevent conflicts with manual data insertion
            // SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var now = DateTime.UtcNow;
            
            // Seed Doctors
            modelBuilder.Entity<Doctor>().HasData(
                new Doctor
                {
                    Id = 1,
                    Name = "Dr. Juan Carlos Pérez",
                    Document = "12345678",
                    Specialty = "Cardiology",
                    Phone = "3001234567",
                    Email = "juan.perez@hospital.com",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Doctor
                {
                    Id = 2,
                    Name = "Dra. María González",
                    Document = "87654321",
                    Specialty = "Pediatrics",
                    Phone = "3007654321",
                    Email = "maria.gonzalez@hospital.com",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Doctor
                {
                    Id = 3,
                    Name = "Dr. Carlos Rodríguez",
                    Document = "11223344",
                    Specialty = "Orthopedics",
                    Phone = "3009876543",
                    Email = "carlos.rodriguez@hospital.com",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            );

            // Seed Patients
            modelBuilder.Entity<Patient>().HasData(
                new Patient
                {
                    Id = 1,
                    Name = "Ana María López",
                    Document = "11111111",
                    Age = 35,
                    Phone = "3001111111",
                    Email = "ana.lopez@email.com",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Patient
                {
                    Id = 2,
                    Name = "Pedro Martínez",
                    Document = "22222222",
                    Age = 42,
                    Phone = "3002222222",
                    Email = "pedro.martinez@email.com",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Patient
                {
                    Id = 3,
                    Name = "Laura Sánchez",
                    Document = "33333333",
                    Age = 28,
                    Phone = "3003333333",
                    Email = "laura.sanchez@email.com",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            );
        }
    }
}
