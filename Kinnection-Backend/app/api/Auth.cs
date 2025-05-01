using System.Security.Authentication;

namespace Kinnection;
static class AuthAPIs
{
    public static void APIs(WebApplication app)
    {
        app.MapPost("/auth/login", (HttpContext httpContext, LoginRequest Request) =>
        {
            try
            {
                var EncryptionKeys = KeyMaster.GetKeys();
                using var Context = DatabaseManager.GetActiveContext();

                // Ensure user exists
                var ExistingUser = Context.Users
                    .FirstOrDefault(b => b.Email == Request.Email) ??
                        throw new KeyNotFoundException(
                            $"A user with email {Request.Email} does not exist.");

                // Ensure user's password exists
                var ExistingPass = Context.Passwords
                    .OrderByDescending(p => p.Created)
                    .FirstOrDefault(p => p.User.ID == ExistingUser!.ID) ??
                        throw new ApplicationException(
                            $"User {ExistingUser.ID} exists, but no associated password object also exists. Please contact support.");

                // Check password is correct
                bool ValidPass = PassForge.IsPassCorrect(
                    KeyMaster.Decrypt(Request.Password, EncryptionKeys.Private), ExistingUser.ID);

                if (!ValidPass)
                {
                    throw new InvalidCredentialException(
                        "The email/password combination used is invalid.");
                }

                // Compile Response
                Authenticator.Provision(ExistingUser.ID, httpContext);
                return Results.NoContent();
            }
            catch (InvalidCredentialException c)
            {
                Console.WriteLine($"Issue with POST /auth/login/: {c}");
                return Results.Problem(
                    detail: c.Message,
                    statusCode: 401);
            }
            catch (KeyNotFoundException k)
            {
                Console.WriteLine($"Issue with POST /auth/login/: {k}");
                return Results.Problem(
                    detail: k.Message,
                    statusCode: 404);
            }
            catch (ApplicationException a)
            {
                Console.WriteLine($"Issue with POST /auth/login/: {a}");
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 500);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Issue with POST /auth/login/: {e}");
                return Results.Problem(
                    statusCode: 500);
            }
        })
        .WithName("PostLogin")
        .WithOpenApi();


        app.MapPost("/auth/logout/", (HttpContext httpContext) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();

                // Authenticate User
                var Tokens = new Dictionary<string, string>()
                {
                    ["access"] = httpContext.Request.Headers.Authorization!,
                    ["refresh"] = httpContext.Request.Headers["X-Refresh-Token"]!
                };
                Authenticator.Authenticate(Context, Tokens: Tokens);

                return Results.NoContent();
            }
            catch (AuthenticationException a)
            {
                Console.WriteLine($"Issue with POST /auth/logout/: {a}");
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 401
                );
            }
            catch (KeyNotFoundException k)
            {
                Console.WriteLine($"Issue with POST /auth/logout/: {k}");
                return Results.Problem(
                    detail: k.Message,
                    statusCode: 404
                );
            }
            catch (Exception e)
            {
                Console.WriteLine($"Issue with POST /auth/logout/: {e}");
                return Results.Problem(statusCode: 500);
            }
        })
        .WithName("PostLogout")
        .WithOpenApi();


        app.MapGet("/auth/public/", (HttpContext httpContext) =>
        {
            try
            {
                var Keys = KeyMaster.GetKeys();

                httpContext.Response.Headers["X-Public"] = Keys.Public;
                return Results.NoContent();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Issue with GET /auth/public/: {e}");
                return Results.Problem(statusCode: 500);
            }
        })
        .WithName("GetPublic")
        .WithOpenApi();


        app.MapPost("/auth/verify/", (HttpContext httpContext) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();

                // Authenticate User
                Authenticator.Authenticate(Context, httpContext: httpContext);

                return Results.NoContent();
            }
            catch (AuthenticationException a)
            {
                Console.WriteLine($"Issue with POST /auth/verify/: {a}");
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 401
                );
            }
            catch (KeyNotFoundException k)
            {
                Console.WriteLine($"Issue with POST /auth/verify/: {k}");
                return Results.Problem(
                    detail: k.Message,
                    statusCode: 404
                );
            }
            catch (Exception e)
            {
                Console.WriteLine($"Issue with POST /auth/verify/: {e}");
                return Results.Problem(statusCode: 500);
            }
        })
        .WithName("PostVerify")
        .WithOpenApi();
    }
}
