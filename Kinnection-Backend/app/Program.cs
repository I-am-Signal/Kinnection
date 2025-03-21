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
      using (var scope = app.Services.CreateScope())
      {
        var services = scope.ServiceProvider;

        var MigrationContext = services.GetRequiredService<KinnectionContext>();

        if (MigrationContext.Database.GetPendingMigrations().Any())
        {
          MigrationContext.Database.Migrate();
        }
      }
    }

    // Ensure encryption keys exist
    Encryption? EncryptionKeys;
    try
    {
      EncryptionKeys = Context.EncryptionKeys
        .OrderByDescending(e => e.Created)
        .FirstOrDefault();
    } finally { }
    
    if (null == EncryptionKeys)
    {
      var Keys = KeyMaster.GenerateKeys();
      EncryptionKeys = new Encryption
      {
        Created = DateTime.UtcNow,
        Public = Keys["public"],
        Private = Keys["private"]
      };

      Context.Add(EncryptionKeys);
      Context.SaveChanges();
      Console.WriteLine("New encryption keys have been created.");
    }

    KeyMaster.SetKeys(EncryptionKeys.Public, EncryptionKeys.Private);

    // Start APIs
    UserAPIs.APIs(app);
    app.Run();
  }
}