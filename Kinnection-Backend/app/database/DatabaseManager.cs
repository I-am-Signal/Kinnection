using Microsoft.EntityFrameworkCore;

namespace Kinnection
{
    public static class DatabaseManager
    {
        private static readonly byte RETRY_ATTEMPTS = Convert.ToByte(Environment.GetEnvironmentVariable("RETRY_ATTEMPTS") ?? "5");
        private static readonly byte RETRY_IN = Convert.ToByte(Environment.GetEnvironmentVariable("RETRY_IN") ?? "5");

        public static readonly string DBURL =
            $"Server={Environment.GetEnvironmentVariable("MYSQL_HOST") ?? Environment.GetEnvironmentVariable("MYSQL_EXTERNAL_HOST")};" +
            $"Port={Environment.GetEnvironmentVariable("MYSQL_PORT")};" +
            $"Database={Environment.GetEnvironmentVariable("MYSQL_DATABASE")};" +
            $"User={Environment.GetEnvironmentVariable("MYSQL_USER")};" +
            $"Password={Environment.GetEnvironmentVariable("MYSQL_PASSWORD")};";

        private static KinnectionContext _activeContext = InitializeDatabaseContext();

        public static KinnectionContext GetActiveContext()
        {
            if (_activeContext == null || IsContextDisposed(_activeContext))
                _activeContext = InitializeDatabaseContext();

            return _activeContext!;
        }

        private static KinnectionContext InitializeDatabaseContext()
        {
            for (int i = 0; i < RETRY_ATTEMPTS; i++)
            {
                try
                {
                    // Build the options and context
                    var optionsBuilder = new DbContextOptionsBuilder<KinnectionContext>().UseMySQL();
                    optionsBuilder.UseMySQL(DBURL);

                    var context = new KinnectionContext(optionsBuilder.Options);
                    if (!context.Database.CanConnect())
                        throw new Exception("Database is not ready for connection.");

                    if (i > 0) // Prevents constant spam of DB connection success to logs
                        Console.WriteLine("Connection to database was successful.");
                    return context;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Try {i + 1} failed to connect to DB: {e.Message}");
                    if (i < RETRY_ATTEMPTS - 1)
                    {
                        Console.WriteLine($"Retrying in {RETRY_IN} seconds...");
                        Thread.Sleep(RETRY_IN * 1000);
                    }
                    else
                    {
                        throw new Exception("Unable to connect to the database after multiple attempts.", e);
                    }
                }
            }

            // Under normal conditions, this will never be triggered
            throw new InvalidOperationException("Unexpected error in database connection logic.");
        }

        private static bool IsContextDisposed(KinnectionContext Context)
        {
            try { return !Context.Database.CanConnect(); }
            catch (ObjectDisposedException) { return true; }
        }
    }
}