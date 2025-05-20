using Kinnection;
using Microsoft.EntityFrameworkCore;
class Program
{
  static void Main()
  {
    // Connect to the DB
    using var Context = DatabaseManager.GetActiveContext();

    // Build API
    var builder = WebApplication.CreateBuilder();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddDbContext<KinnectionContext>(options =>
      options.UseMySQL(DatabaseManager.DBURL));

    builder.Services.AddCors(options =>
    {
      options.AddPolicy("AllowFrontend", policy =>
      {
        var ANG_PORT = Environment.GetEnvironmentVariable("ANG_PORT");
        var ISSUER = Environment.GetEnvironmentVariable("ISSUER");
        if (string.IsNullOrWhiteSpace(ANG_PORT) ||
          string.IsNullOrWhiteSpace(ISSUER))
          throw new Exception("Missing value for an Angular environment variable!");

        if (builder.Environment.IsDevelopment())
        {
          policy.WithOrigins("*");
        }
        else
        {
          policy.WithOrigins($"{ISSUER}:{ANG_PORT}")
            .AllowCredentials();
        }

        policy.AllowAnyHeader()
          .AllowAnyMethod();
      });
    });

    if (builder.Environment.IsDevelopment())
    {
      builder.Services.AddSwaggerGen();
      builder.Logging.AddConsole();
    }

    var app = builder.Build();

    app.UseCors("AllowFrontend");
    if (app.Environment.IsDevelopment())
    {
      app.UseSwagger();
      app.UseSwaggerUI(options =>
      {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
      });
    }

    if (app.Environment.IsProduction())
      app.UseHttpsRedirection();

    // Migrate
    if (Environment.GetEnvironmentVariable("APPLY_MIGRATIONS") == "1")
    {
      using var scope = app.Services.CreateScope();

      var MigrationContext = scope.ServiceProvider.GetRequiredService<KinnectionContext>();

      if (MigrationContext.Database.GetPendingMigrations().Any())
        MigrationContext.Database.Migrate();
      else if (!MigrationContext.Database.GetAppliedMigrations().Any())
        throw new InvalidOperationException(
          "No EF Core Migrations found. Create at least one migration before running the application.");
    }

    // Start APIs
    UserAPIs.APIs(app);
    AuthAPIs.APIs(app);
    TreeAPIs.APIs(app);
    MemberAPIs.APIs(app);
    app.Run();

    // Ensure encryption keys exist
    KeyMaster.GetKeys();
  }
}