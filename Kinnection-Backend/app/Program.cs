using Kinnection;
using Microsoft.EntityFrameworkCore;
class Program
{
  static void Main(string[] args)
  {
    // Connect to the DB
    using var Context = DatabaseManager.GetActiveContext();

    // Build API
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddDbContext<KinnectionContext>(options =>
      options.UseMySQL(DatabaseManager.DBURL));

    builder.Services.AddSwaggerGen();

    var app = builder.Build();

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
    }

    // Ensure encryption keys exist
    KeyMaster.SearchKeys();

    // Start APIs
    UserAPIs.APIs(app);
    app.Run();
  }
}