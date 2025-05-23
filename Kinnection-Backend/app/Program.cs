using Kinnection;
using Microsoft.EntityFrameworkCore;
class Program
{
  static void Main()
  {
    // Get required env vars
    var ANG_PORT = Environment.GetEnvironmentVariable("ANG_PORT");
    var ISSUER = Environment.GetEnvironmentVariable("ISSUER");
    if (string.IsNullOrWhiteSpace(ANG_PORT) || string.IsNullOrWhiteSpace(ISSUER))
      throw new Exception("Missing value for an Angular environment variable!");

    // Connect to the DB
    using var Context = DatabaseManager.GetActiveContext();

    // Build API
    var builder = WebApplication.CreateBuilder();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddDbContext<KinnectionContext>(options =>
      options.UseMySQL(DatabaseManager.DBURL));

    // Compile allowed origins
    IEnumerable<string> Origins = new List<string>
    {
      ISSUER, // Root
      $"{ISSUER}:{ANG_PORT}", // Root with container port
    };

    // Add in dev services
    if (builder.Environment.IsDevelopment())
    {
      var ANG_LOCAL = Environment.GetEnvironmentVariable("ANG_PORT_LOCAL");
      if (string.IsNullOrWhiteSpace(ANG_LOCAL))
        throw new Exception("Missing ANG_PORT_LOCAL environment variable!");
      Origins = Origins.Append($"{ISSUER}:{ANG_LOCAL}"); // Root with local dev port
      builder.Services.AddSwaggerGen();
      builder.Logging.AddConsole();
    }

    // Add CORS preflight checks
    builder.Services.AddCors(options =>
    {
      options.AddPolicy("AllowFrontend", policy =>
      {
        policy.WithOrigins(Origins.ToArray())
          .AllowCredentials()
          .AllowAnyHeader()
          .AllowAnyMethod();
      });
    });

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