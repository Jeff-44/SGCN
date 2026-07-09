using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SGCN.Domain.Entities;
using SGCN.Infrastructure.Persistence;

namespace SGCN.Infrastructure.Data;

public static class ReferentialSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (await context.Departments.AnyAsync())
        {
            return;
        }

        var departments = new[]
        {
            new Department { Name = "Ouest", Code = "OU", IsActive = true },
            new Department { Name = "Artibonite", Code = "AR", IsActive = true },
            new Department { Name = "Nord", Code = "NO", IsActive = true }
        };

        context.Departments.AddRange(departments);
        await context.SaveChangesAsync();

        var ouest = departments[0];
        var artibonite = departments[1];
        var nord = departments[2];

        var communes = new[]
        {
            new Commune { Name = "Port-au-Prince", Code = "PAP", DepartmentId = ouest.Id, IsActive = true },
            new Commune { Name = "Carrefour", Code = "CAR", DepartmentId = ouest.Id, IsActive = true },
            new Commune { Name = "Gonaïves", Code = "GON", DepartmentId = artibonite.Id, IsActive = true },
            new Commune { Name = "Cap-Haïtien", Code = "CAP", DepartmentId = nord.Id, IsActive = true }
        };

        context.Communes.AddRange(communes);
        await context.SaveChangesAsync();

        var portAuPrince = communes[0];
        var carrefour = communes[1];
        var gonaives = communes[2];
        var capHaïtien = communes[3];

        var hospitals = new[]
        {
            new Hospital { Name = "Hôpital de la Paix", Code = "HPX", CommuneId = portAuPrince.Id, Address = "Port-au-Prince", IsActive = true },
            new Hospital { Name = "Hôpital Saint-Damien", Code = "HSD", CommuneId = portAuPrince.Id, Address = "Tabarre", IsActive = true },
            new Hospital { Name = "Hôpital de Carrefour", Code = "HCA", CommuneId = carrefour.Id, Address = "Carrefour", IsActive = true },
            new Hospital { Name = "Hôpital Justinien", Code = "HJU", CommuneId = gonaives.Id, Address = "Gonaïves", IsActive = true },
            new Hospital { Name = "Hôpital Général de Cap-Haïtien", Code = "HGH", CommuneId = capHaïtien.Id, Address = "Cap-Haïtien", IsActive = true }
        };

        context.Hospitals.AddRange(hospitals);
        await context.SaveChangesAsync();
    }
}
