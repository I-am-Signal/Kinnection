using Microsoft.EntityFrameworkCore;

// // Apparently Insecure for passwords alone
// byte[] data = System.Text.Encoding.ASCII.GetBytes(inputString);
// data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
// String hash = System.Text.Encoding.ASCII.GetString(data);


// // Better password hashing
// using Microsoft.AspNetCore.Identity;

// var hasher = new PasswordHasher<object>();

// // Hash password
// string HashedPassword = hasher.HashPassword(null, "MySecurePassword");

// // Verify password
// PasswordVerificationResult result = hasher.VerifyHashedPassword(null, hashedPassword, "MySecurePassword");
// bool isMatch = result == PasswordVerificationResult.Success;


namespace Kinnection
{
    static class UserAPIs
    {
        public static WebApplication APIs(WebApplication app)
        {
            app.MapPost("/users", async (HttpContext httpContext, PostUserRequest request) =>
            {
                try
                {
                    using var context = DatabaseManager.GetActiveContext();
                    User? existing = await context.Users
                        .FirstOrDefaultAsync(b => b.Email == request.Email);

                    if (existing != null)
                    {
                        throw new DbUpdateException();
                    }

                    User NewUser = new User
                    {
                        Created = DateTime.UtcNow,
                        Fname = request.Fname,
                        Lname = request.Lname,
                        Email = request.Email,
                        GoogleSO = false
                    };

                    context.Users.Add(NewUser);

                    // var EncryptionKeys = await context.EncryptionKeys
                    //     .OrderByDescending(b => b.Created)
                    //     .FirstOrDefaultAsync()
                    //     ?? throw new KeyNotFoundException();

                    Password NewPass = new Password
                    {
                        Created = DateTime.UtcNow,
                        UserID = NewUser.ID,
                        // PassString = KeyMaster.DecryptWithPrivateKey(request.Password, EncryptionKeys.Private)
                        PassString = request.Password
                    };

                    context.Passwords.Add(NewPass);

                    await context.SaveChangesAsync();
                    return Results.Created($"/users/{NewUser.ID}", new UserResponse
                    {
                        ID = NewUser.ID,
                        Fname = NewUser.Fname,
                        Lname = NewUser.Lname,
                        Email = NewUser.Email
                    });
                }
                catch (DbUpdateException d)
                {
                    Console.WriteLine(d);
                    return Results.Problem(
                        detail: $"A User with email {request.Email} already exists.",
                        statusCode: 409
                    );
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return Results.Problem(statusCode: 500);
                }
            })
            .WithName("PostUser")
            .WithOpenApi();


            app.MapPut("/users/{id}", async (int id, HttpContext httpContext, UserRequest request) =>
            {
                try
                {
                    using var context = DatabaseManager.GetActiveContext();

                    var existing = await context.Users
                        .FirstOrDefaultAsync(b => b.Email == request.Email)
                        ?? throw new KeyNotFoundException();

                    existing.Fname = request.Fname;
                    existing.Lname = request.Lname;
                    existing.Email = request.Email;

                    await context.SaveChangesAsync();

                    return Results.Ok(new UserResponse
                    {
                        ID = existing.ID,
                        Fname = existing.Fname,
                        Lname = existing.Lname,
                        Email = existing.Email
                    });
                }
                catch (KeyNotFoundException k)
                {
                    Console.WriteLine(k);
                    return Results.Problem(
                        detail: $"A User with email {request.Email} does not exist.",
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


            app.MapGet("/users/", async () =>
            {
                try
                {
                    using var Context = DatabaseManager.GetActiveContext();
                    var output = await Context.Users
                        .Select(user => new UserResponse
                        {
                            ID = user.ID,
                            Fname = user.Fname,
                            Lname = user.Lname,
                            Email = user.Email
                        }).ToListAsync();

                    return Results.Ok(output);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return Results.Problem(statusCode: 500);
                }
            })
            .WithName("GetUsers")
            .WithOpenApi();


            app.MapGet("/users/{id}", async (int id) =>
            {
                try
                {
                    using var Context = DatabaseManager.GetActiveContext();
                    var output = await Context.Users
                        .Select(user => new UserResponse
                        {
                            ID = user.ID,
                            Fname = user.Fname,
                            Lname = user.Lname,
                            Email = user.Email
                        })
                        .SingleAsync(u => u.ID == id);

                    return Results.Ok(output);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return Results.Problem(statusCode: 500);
                }
            })
            .WithName("GetUser")
            .WithOpenApi();


            app.MapDelete("/users/{id}", async (int id) =>
            {
                try
                {
                    using var Context = DatabaseManager.GetActiveContext();

                    User DeletedUser = await Context.Users
                        .SingleAsync(u => u.ID == id);
                    Context.Remove(DeletedUser);
                    await Context.SaveChangesAsync();

                    return Results.Ok(new UserResponse
                    {
                        ID = DeletedUser.ID,
                        Fname = DeletedUser.Fname,
                        Lname = DeletedUser.Lname,
                        Email = DeletedUser.Email
                    });
                }
                catch (InvalidOperationException i)
                {
                    Console.WriteLine(i);
                    return Results.Problem(
                        detail: $"A user with ID {id} does not exist.",
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

            return app;
        }
    }
}