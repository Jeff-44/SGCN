using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGCN.Application.Interfaces;
using SGCN.Domain.Identity;
using SGCN.Infrastructure.Persistence;
using SGCN.Infrastructure.Persistence.Repositories;
using SGCN.Infrastructure.Services;

namespace SGCN.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<ICommuneService, CommuneService>();
        services.AddScoped<IHospitalService, HospitalService>();
        services.AddScoped<ISgcnIdGenerator, SgcnIdGenerator>();
        services.AddScoped<IBirthRecordService, BirthRecordService>();
        services.AddScoped<ICertificateNumberGenerator, CertificateNumberGenerator>();
        services.AddScoped<IVerificationCodeGenerator, VerificationCodeGenerator>();
        services.AddScoped<ICertificateRequestService, CertificateRequestService>();
        services.AddScoped<ICertificateService, CertificateService>();

        return services;
    }
}
