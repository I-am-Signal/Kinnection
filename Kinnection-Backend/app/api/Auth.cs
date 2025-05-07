using System.Security.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Kinnection;
static class AuthAPIs
{
    public static void APIs(WebApplication app)
    {
        app.MapPost("/auth/login", (HttpContext httpContext, LoginRequest Request) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();

                // Ensure user exists
                var ExistingUser = Context.Users
                    .FirstOrDefault(b => b.Email == Request.Email);
                if (ExistingUser == null)
                    return Results.Problem(
                        statusCode: 404,
                        detail: $"A user with email {Request.Email} does not exist."
                    );

                // Ensure user's password exists
                var ExistingPass = Context.Passwords
                    .OrderByDescending(p => p.Created)
                    .FirstOrDefault(p => p.User.ID == ExistingUser!.ID);
                if (ExistingPass == null)
                    return Results.Problem(
                        statusCode: 500,
                        detail: $"User {ExistingUser.ID} exists, but no associated password object also exists. Please contact support."
                    );

                // Check password is correct
                bool PassIsValid = PassForge.IsPassCorrect(
                    KeyMaster.Decrypt(Request.Password), ExistingUser.ID);

                if (!PassIsValid)
                    return Results.Problem(
                        statusCode: 401,
                        detail: "The email/password combination used is invalid."
                    );                

                // Compile Response
                Authenticator.Provision(ExistingUser.ID, httpContext);
                return Results.NoContent();
            }
            catch (KeyNotFoundException k)
            {
                Console.WriteLine($"Issue with POST /auth/login/: {k}");
                return Results.Problem(
                    detail: k.Message,
                    statusCode: 404
                );
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
                Authenticator.Authenticate(Context, httpContext: httpContext);

                // No tokens needed on successful logout
                httpContext.Response.Headers.Authorization = "";
                httpContext.Response.Headers["X-Refresh-Token"] = "";

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

        app.MapPost("/auth/pass/forgot", async (HttpContext httpContext, ForgotPassRequest Request) =>
        {
            try
            {
                // URL needs to be frontend url
                // This would allow for the password to be collected and then sent to the backend
                // via /auth/pass/reset with Password in the body
                string ResetURL = Authenticator.GeneratePassResetURL(Request.Email);

                var Response = await JustGonnaSendIt.SendEmail(
                    Address: new SendGrid.Helpers.Mail.EmailAddress(Request.Email),
                    Subject: "Password Reset Request",
                    PlainTextContent: @$"Password Reset Request
Link to reset your password.
If the above link did not work, please copy and paste the following link into your browser:
{ResetURL}",
                    HTMLContent: @$"<h1>Password Reset Request</h1>
<p><a href=""{ResetURL}"">Link to reset your password.</a></p>
<p>If the above link did not work, please copy and paste the following link into your browser:</p>
<p>{ResetURL}</p>"
                );

                if (Response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    Console.WriteLine(Response);
                    throw new Exception("Response from SendGrid status code was not success!");
                }
                return Results.Ok();
            }
            catch (InvalidOperationException i)
            {
                Console.WriteLine($"Issue with POST /auth/pass/forgot: {i}");
                return Results.Problem(
                    statusCode: 404,
                    detail: $"A user with email {Request.Email} was not found.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Issue with POST /auth/pass/forgot: {e}");
                return Results.Problem(statusCode: 500);
            }
        })
        .WithName("PostPassForgot")
        .WithOpenApi();

        app.MapPost("/auth/pass/reset/", (HttpContext httpContext, ResetPassRequest Request) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();

                // Verify and process token into its parts
                var ProcessedToken = Authenticator.ProcessToken(
                    Base64UrlEncoder.Decode(
                        httpContext.Request.Headers["X-Reset-Token"]));

                // Check that the reset token is valid
                var UserAuth = Context.Authentications
                    .Include(a => a.User)
                    .First(a => a.User.ID == ProcessedToken["payload"]["sub"].GetInt32());

                if (!ProcessedToken["payload"].TryGetValue("res", out var Reset))
                    throw new AuthenticationException(
                        "Invalid Reset Token");

                if (!Reset.GetBoolean())
                    throw new AuthenticationException(
                        "Password reset was denied. Please contact support.");

                // Get signature for comparison
                var Token = UserAuth.Refresh.Split('.');
                if (Token.Length != 3)
                    return Results.Problem(
                        statusCode: 401,
                        detail: "The provided reset token is invalid."
                    );
                string SavedSignature = Token[2];

                if (SavedSignature != ProcessedToken["signature"]["signature"].GetString())
                    throw new AuthenticationException("Invalid Reset Token");

                if (Authenticator.IsExpired(
                    DateTimeOffset.FromUnixTimeSeconds(
                        ProcessedToken["payload"]["exp"].GetInt64())))
                    throw new AuthenticationException("Expired Reset Token");

                // Check new password is not the same as previous passwords
                string HashedPass = PassForge.HashPass(
                        KeyMaster.Decrypt(Request.Password));

                var PrevPasswords = Context.Passwords
                    .Where(p => p.User.ID == ProcessedToken["payload"]["sub"].GetInt32())
                    .ToList();

                foreach (var Pass in PrevPasswords)
                {
                    if (Authenticator.IsEqual(HashedPass, Pass.PassString))
                        return Results.Problem(
                            statusCode: 400,
                            title: "Password Reuse Denied",
                            detail: "Password cannot be the same as a previously used password."
                        );
                }

                var NewPass = new Password
                {
                    Created = DateTime.UtcNow,
                    // Decrypt encrypted-in-transit password, then hash and store
                    PassString = PassForge.HashPass(
                        KeyMaster.Decrypt(Request.Password)),
                    User = UserAuth.User
                };
                Context.Passwords.Add(NewPass);

                // Clear token
                UserAuth.Refresh = Authenticator.GenerateRandomString();

                Context.SaveChanges();

                return Results.Ok();
            }
            catch (AuthenticationException a)
            {
                Console.WriteLine($"Issue at POST /auth/pass/reset: {a}");
                return Results.Problem(
                    statusCode: 401,
                    detail: a.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Issue at POST /auth/pass/reset: {e}");
                return Results.Problem(statusCode: 500);
            }
        })
        .WithName("PostPassReset")
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
