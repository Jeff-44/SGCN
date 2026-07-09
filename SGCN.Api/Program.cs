using SGCN.Api.Extensions;
using SGCN.Api.Middleware;
using SGCN.Application;
using SGCN.Infrastructure;
using SGCN.Infrastructure.Data;
using SGCN.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using SGCN.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiDataProtection(builder.Environment);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddStandardApiResponses();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddHealthChecks();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://localhost:5174", "sgcn.up.railway.app")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
var app = builder.Build();

await IdentitySeeder.SeedRolesAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    await DevelopmentAdminSeeder.SeedAsync(app.Services, app.Configuration);
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}
await ReferentialSeeder.SeedAsync(app.Services);

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
