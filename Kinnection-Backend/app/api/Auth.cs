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
                        Console.WriteLine("User exists, but no associated password object also exists.");
                        throw new Exception();
                    }

                    // Check password is correct
                    string PrivateKey = (await context.EncryptionKeys
                        .OrderByDescending(b => b.Created)
                        .FirstOrDefaultAsync()
                        ?? throw new Exception()).Private;

                    string Password = KeyMaster.DecryptWithPrivateKey(Request.Password, PrivateKey);

                    if (!Authenticator.CheckHashEquivalence(ExistingPass!.PassString, Password))
                    {
                        throw new InvalidCredentialException("The email and password combination used is invalid.");
                    }

                    // Compile Response
                    var Tokens = await Authenticator.Provision(ExistingUser.ID);
                    httpContext.Response.Headers.Authorization = $"Bearer {Tokens["access"]}";
                    httpContext.Response.Headers["X-Refresh-Token"] = Tokens["refresh"];
                    return Results.Ok(new LoginResponse { ID = ExistingUser.ID });
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


            app.MapPost("/auth/logout/{id}", async (int id, HttpContext httpContext) =>
            {
                try
                {
                    using var Context = DatabaseManager.GetActiveContext();

                    // Authenticate User
                    await Authenticator.Authenticate(Context, httpContext, id, false);

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


            app.MapGet("/auth/public/", async (HttpContext httpContext) =>
            {
                try
                {
                    // Get the public key
                    string Public = Environment.GetEnvironmentVariable("Public")!;
                    if (Public == null)
                    {
                        using var Context = DatabaseManager.GetActiveContext();
                        Public = (await Context.EncryptionKeys.FirstOrDefaultAsync())!.Public;
                        Environment.SetEnvironmentVariable("Public", Public);
                    }

                    httpContext.Response.Headers["X-Public"] = Public;
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
            
            
            app.MapPost("/auth/verify/{id}", async (int id, HttpContext httpContext) =>
            {
                try
                {
                    using var Context = DatabaseManager.GetActiveContext();

                    // Authenticate User
                    await Authenticator.Authenticate(Context, httpContext, id);

                    return Results.Ok(new VerifyResponse { ID = id });
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