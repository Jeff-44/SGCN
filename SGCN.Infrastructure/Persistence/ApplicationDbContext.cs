using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SGCN.Domain.Common;
using SGCN.Domain.Entities;
using SGCN.Domain.Identity;

namespace SGCN.Infrastructure.Persistence;

public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Commune> Communes => Set<Commune>();
    public DbSet<Hospital> Hospitals => Set<Hospital>();
    public DbSet<BirthRecord> BirthRecords => Set<BirthRecord>();
    public DbSet<CertificateRequest> CertificateRequests => Set<CertificateRequest>();
    public DbSet<Certificate> Certificates => Set<Certificate>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.FullName).HasMaxLength(150);
            entity.Property(user => user.NifOrCin).HasMaxLength(50);
        });

        builder.Entity<Department>(entity =>
        {
            entity.Property(department => department.Name).HasMaxLength(150).IsRequired();
            entity.Property(department => department.Code).HasMaxLength(50);
            entity.HasIndex(department => department.Code).IsUnique();
            entity.HasMany(department => department.Communes)
                .WithOne(commune => commune.Department)
                .HasForeignKey(commune => commune.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Commune>(entity =>
        {
            entity.Property(commune => commune.Name).HasMaxLength(150).IsRequired();
            entity.Property(commune => commune.Code).HasMaxLength(50);
            entity.HasIndex(commune => commune.Code).IsUnique();
            entity.HasOne(commune => commune.Department)
                .WithMany(department => department.Communes)
                .HasForeignKey(commune => commune.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(commune => commune.Hospitals)
                .WithOne(hospital => hospital.Commune)
                .HasForeignKey(hospital => hospital.CommuneId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Hospital>(entity =>
        {
            entity.Property(hospital => hospital.Name).HasMaxLength(150).IsRequired();
            entity.Property(hospital => hospital.Code).HasMaxLength(50);
            entity.Property(hospital => hospital.Address).HasMaxLength(250);
            entity.HasIndex(hospital => hospital.Code).IsUnique();
            entity.HasOne(hospital => hospital.Commune)
                .WithMany(commune => commune.Hospitals)
                .HasForeignKey(hospital => hospital.CommuneId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<BirthRecord>(entity =>
        {
            entity.Property(record => record.SgcnId).HasMaxLength(30).IsRequired();
            entity.Property(record => record.ChildFirstName).HasMaxLength(100).IsRequired();
            entity.Property(record => record.ChildLastName).HasMaxLength(100).IsRequired();
            entity.Property(record => record.BirthPlace).HasMaxLength(200).IsRequired();
            entity.Property(record => record.MotherFullName).HasMaxLength(150).IsRequired();
            entity.Property(record => record.FatherFullName).HasMaxLength(150);
            entity.Property(record => record.HospitalFileNumber).HasMaxLength(100);
            entity.Property(record => record.CreatedByUserId).IsRequired();
            entity.HasIndex(record => record.SgcnId).IsUnique();
            entity.HasOne(record => record.Hospital)
                .WithMany()
                .HasForeignKey(record => record.HospitalId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(record => record.CreatedByUser)
                .WithMany()
                .HasForeignKey(record => record.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CertificateRequest>(entity =>
        {
            entity.Property(request => request.RequestNumber).HasMaxLength(30).IsRequired();
            entity.Property(request => request.RequestedByUserId).IsRequired();
            entity.Property(request => request.TargetFirstName).HasMaxLength(100).IsRequired();
            entity.Property(request => request.TargetLastName).HasMaxLength(100).IsRequired();
            entity.Property(request => request.MotherFullName).HasMaxLength(150).IsRequired();
            entity.Property(request => request.FatherFullName).HasMaxLength(150);
            entity.Property(request => request.BirthPlace).HasMaxLength(200).IsRequired();
            entity.Property(request => request.HospitalFileNumber).HasMaxLength(100);
            entity.Property(request => request.RelationshipToTarget).HasMaxLength(100).IsRequired();
            entity.Property(request => request.RejectionReason).HasMaxLength(500);
            entity.HasIndex(request => request.RequestNumber).IsUnique();
            entity.HasOne(request => request.RequestedByUser)
                .WithMany()
                .HasForeignKey(request => request.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(request => request.LinkedBirthRecord)
                .WithMany()
                .HasForeignKey(request => request.LinkedBirthRecordId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Certificate>(entity =>
        {
            entity.Property(certificate => certificate.CertificateNumber).HasMaxLength(30).IsRequired();
            entity.Property(certificate => certificate.ChildFirstName).HasMaxLength(100).IsRequired();
            entity.Property(certificate => certificate.ChildLastName).HasMaxLength(100).IsRequired();
            entity.Property(certificate => certificate.BirthPlace).HasMaxLength(200).IsRequired();
            entity.Property(certificate => certificate.HospitalName).HasMaxLength(150).IsRequired();
            entity.Property(certificate => certificate.CommuneName).HasMaxLength(150).IsRequired();
            entity.Property(certificate => certificate.DepartmentName).HasMaxLength(150).IsRequired();
            entity.Property(certificate => certificate.MotherFullName).HasMaxLength(150).IsRequired();
            entity.Property(certificate => certificate.FatherFullName).HasMaxLength(150);
            entity.Property(certificate => certificate.CreatedByUserId).IsRequired();
            entity.Property(certificate => certificate.VerificationCode).HasMaxLength(50).IsRequired();
            entity.Property(certificate => certificate.PdfPath).HasMaxLength(500);
            entity.Property(certificate => certificate.QrCodePath).HasMaxLength(500);
            entity.Property(certificate => certificate.AnnulledReason).HasMaxLength(500);
            entity.HasIndex(certificate => certificate.CertificateNumber).IsUnique();
            entity.HasIndex(certificate => certificate.VerificationCode).IsUnique();
            entity.HasIndex(certificate => certificate.BirthRecordId).IsUnique();
            entity.HasIndex(certificate => certificate.CertificateRequestId).IsUnique();
            entity.HasOne(certificate => certificate.CertificateRequest)
                .WithMany()
                .HasForeignKey(certificate => certificate.CertificateRequestId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(certificate => certificate.BirthRecord)
                .WithMany()
                .HasForeignKey(certificate => certificate.BirthRecordId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(certificate => certificate.CreatedByUser)
                .WithMany()
                .HasForeignKey(certificate => certificate.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ApplyAuditInfo()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ApplicationUser>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
            }
        }
    }
}
