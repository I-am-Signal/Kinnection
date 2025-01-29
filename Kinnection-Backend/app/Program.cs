using Kinnection;
using Microsoft.EntityFrameworkCore;
class Program
{
  static void Main(string[] args)
  {
    // Connect to the DB
    DatabaseManager.GetActiveContext();

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
    using (var scope = app.Services.CreateScope()){
      var services = scope.ServiceProvider;

      var context = services.GetRequiredService<KinnectionContext>();

      if (context.Database.GetPendingMigrations().Any())
      {
        context.Database.Migrate();
      }
    }

    // Start API    
    API.APIs(app);
  }
}