using System.Security.Authentication;

namespace Kinnection;

static class UserAPIs
{
    public static void APIs(WebApplication app)
    {
        app.MapPost("/users", (HttpContext httpContext, PostUsersRequest request) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();
                var EncryptionKeys = KeyMaster.GetKeys();

                // Check user with that email does not exist
                var Existing = Context.Users
                    .FirstOrDefault(e => e.Email == request.Email);

                if (Existing != null)
                {
                    throw new InvalidOperationException(
                        $"A User with email {request.Email} already exists.");
                }

                // Validate required fields
                if (string.IsNullOrEmpty(request.Password))
                    throw new ArgumentException("Password must not be null!");
                if (string.IsNullOrEmpty(request.Email))
                    throw new ArgumentException("Email must not be null!");

                // Hash password
                string PassHash = PassForge.HashPass(
                    KeyMaster.Decrypt(
                        request.Password,
                        EncryptionKeys.Private));

                // Create the new user
                var NewUser = new User
                {
                    Created = DateTime.UtcNow,
                    Fname = request.Fname,
                    Lname = request.Lname,
                    Email = request.Email,
                    GoogleSO = false
                };

                Context.Users.Add(NewUser);
                Context.SaveChanges();

                // Create new password
                Password NewPass = new Password
                {
                    Created = DateTime.UtcNow,
                    UserID = NewUser.ID,
                    PassString = PassHash
                };

                Context.Passwords.Add(NewPass);
                Context.SaveChanges();

                // Compile response
                Authenticator.Provision(NewUser.ID, httpContext);
                return Results.Created($"{NewUser.ID}", new GetUsersResponse
                {
                    ID = NewUser.ID,
                    Fname = NewUser.Fname,
                    Lname = NewUser.Lname,
                    Email = NewUser.Email
                });
            }
            catch (ArgumentException a)
            {
                Console.WriteLine(a);
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 400);
            }
            catch (AuthenticationException a)
            {
                Console.WriteLine(a);
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 401
                );
            }
            catch (InvalidOperationException i)
            {
                Console.WriteLine(i);
                return Results.Problem(
                    detail: i.Message,
                    statusCode: 409);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Results.Problem(statusCode: 500);
            }
        })
        .WithName("PostUser")
        .WithOpenApi();


        app.MapPut("/users/{id}", (int id, HttpContext httpContext, PutUsersRequest request) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();

                // Authenticate
                Authenticator.Authenticate(Context, httpContext: httpContext);

                // Modify and save user
                var existing = Context.Users
                    .First(b => b.ID == id);

                existing.Fname = request.Fname;
                existing.Lname = request.Lname;
                existing.Email = request.Email;

                Context.SaveChanges();

                // Compile response
                return Results.Ok(new GetUsersResponse
                {
                    ID = existing.ID,
                    Fname = existing.Fname,
                    Lname = existing.Lname,
                    Email = existing.Email
                });
            }
            catch (AuthenticationException a)
            {
                Console.WriteLine(a);
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 401
                );
            }
            catch (InvalidOperationException i)
            {
                Console.WriteLine(i);
                return Results.Problem(
                    detail: i.Message,
                    statusCode: 404
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Results.Problem(statusCode: 500);
            }
        })
        .WithName("PutUser")
        .WithOpenApi();

        app.MapGet("/users/{id}", (int id, HttpContext httpContext) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();

                // Authenticate
                Authenticator.Authenticate(Context, httpContext: httpContext);

                // Compile response
                return Results.Ok(
                    Context.Users.Select(user => new GetUsersResponse
                    {
                        ID = user.ID,
                        Fname = user.Fname,
                        Lname = user.Lname,
                        Email = user.Email
                    })
                    .Single(u => u.ID == id)
                );
            }
            catch (AuthenticationException a)
            {
                Console.WriteLine(a);
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 401
                );
            }
            catch (InvalidOperationException i)
            {
                Console.WriteLine(i);
                return Results.Problem(
                    detail: i.Message,
                    statusCode: 404
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Results.Problem(statusCode: 500);
            }
        })
        .WithName("GetUser")
        .WithOpenApi();

        app.MapDelete("/users/{id}", (int id, HttpContext httpContext) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();

                // Authenticate
                Authenticator.Authenticate(Context, httpContext: httpContext);

                // Find the user to delete
                var UserToDelete = Context.Users
                    .FirstOrDefault(u => u.ID == id) ??
                        throw new InvalidOperationException($"User with ID {id} not found.");

                // Remove the user and their associated records
                List<object> UserRecords = [];
                UserRecords.AddRange(Context.Passwords
                    .Where(p => p.UserID == UserToDelete.ID)
                    .ToList());
                UserRecords.AddRange(Context.Authentications
                    .Where(a => a.UserID == UserToDelete.ID)
                    .ToList());
                UserRecords.Add(UserToDelete);
                foreach (var Record in UserRecords)
                    Context.Remove(Record);

                Context.SaveChanges();

                // Return a 204 No Content response
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
            catch (InvalidOperationException i)
            {
                Console.WriteLine(i);
                return Results.Problem(
                    detail: i.Message,
                    statusCode: 404
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Results.Problem(statusCode: 500);
            }
        })
        .WithName("DeleteUser")
        .WithOpenApi();
    }
}