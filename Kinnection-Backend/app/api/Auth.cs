using System.Security.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Kinnection
{
    static class AuthAPIs
    {
        public static void APIs(WebApplication app)
        {
            app.MapPost("/auth/login", async (HttpContext httpContext, LoginRequest Request) =>
            {
                try
                {
                    // Ensure user exists
                    using var context = DatabaseManager.GetActiveContext();
                    var ExistingUser = await context.Users
                        .FirstOrDefaultAsync(b => b.Email == Request.Email) ?? 
                            throw new KeyNotFoundException($"A user with email {Request.Email} does not exist.");

                    // Ensure user's password exists
                    var ExistingPass = await context.Passwords
                        .OrderByDescending(p => p.Created)
                        .FirstOrDefaultAsync(p => p.UserID == ExistingUser!.ID);

                    if (ExistingPass != null)
                    {
                        string Message = $"User {ExistingUser.ID} exists, but no associated password object also exists. Please contact support.";
                        Console.WriteLine(Message);
                        throw new Exception(Message);
                    }

                    // Check password is correct
                    string PrivateKey = (await context.EncryptionKeys
                        .OrderByDescending(b => b.Created)
                        .FirstOrDefaultAsync()
                        ?? throw new Exception()).Private;

                    string Password = KeyMaster.Decrypt(Request.Password, PrivateKey);

                    // TO DO: Build separate password manager module
                    // if (!Authenticator.CheckHashEquivalence(ExistingPass!.PassString, Password))
                    // {
                    //     throw new InvalidCredentialException("The email/password combination used is invalid.");
                    // }

                    // Compile Response
                    var Tokens = await Authenticator.Provision(ExistingUser.ID);
                    httpContext.Response.Headers.Authorization = $"Bearer {Tokens["access"]}";
                    httpContext.Response.Headers["X-Refresh-Token"] = Tokens["refresh"];
                    return Results.NoContent();
                }
                catch (InvalidCredentialException c)
                {
                    Console.WriteLine(c);
                    return Results.Problem(
                        detail: c.Message,
                        statusCode: 401
                    );
                }
                catch (KeyNotFoundException k)
                {
                    Console.WriteLine(k);
                    return Results.Problem(
                        detail: k.Message,
                        statusCode: 404
                    );
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return Results.Problem(statusCode: 500);
                }
            })
            .WithName("PostLogin")
            .WithOpenApi();


            app.MapPost("/auth/logout/", async (HttpContext httpContext) =>
            {
                try
                {
                    using var Context = DatabaseManager.GetActiveContext();

                    // Authenticate User
                    await Authenticator.Authenticate(Context, httpContext, false);

                    return Results.NoContent();
                }
                catch (AuthenticationException a)
                {
                    Console.WriteLine(a);
                    return Results.Problem(
                        detail: a.Message,
                        statusCode: 401
                    );
                }
                catch (KeyNotFoundException k)
                {
                    Console.WriteLine(k);
                    return Results.Problem(
                        detail: k.Message,
                        statusCode: 404
                    );
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return Results.Problem(statusCode: 500);
                }
            })
            .WithName("PostLogout")
            .WithOpenApi();


            app.MapGet("/auth/public/", (HttpContext httpContext) =>
            {
                try
                {
                    var Keys = KeyMaster.SearchKeys();

                    httpContext.Response.Headers["X-Public"] = Keys.Public;
                    return Results.NoContent();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return Results.Problem(statusCode: 500);
                }
            })
            .WithName("GetPublic")
            .WithOpenApi();
            
            
            app.MapPost("/auth/verify/", async (HttpContext httpContext) =>
            {
                try
                {
                    using var Context = DatabaseManager.GetActiveContext();

                    // Authenticate User
                    await Authenticator.Authenticate(Context, httpContext);

                    return Results.NoContent();
                }
                catch (AuthenticationException a)
                {
                    Console.WriteLine(a);
                    return Results.Problem(
                        detail: a.Message,
                        statusCode: 401
                    );
                }
                catch (KeyNotFoundException k)
                {
                    Console.WriteLine(k);
                    return Results.Problem(
                        detail: k.Message,
                        statusCode: 404
                    );
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return Results.Problem(statusCode: 500);
                }
            })
            .WithName("PostLogout")
            .WithOpenApi();
        }
    }
}