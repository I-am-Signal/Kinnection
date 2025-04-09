using System.Security.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Kinnection;

static class TreeAPIs
{
    public static void APIs(WebApplication app)
    {
        app.MapPost("/trees", (HttpContext httpContext, PostTreesRequest request) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();
                var (_, UserID) = Authenticator.Authenticate(
                    Context: Context, httpContext: httpContext);

                var EncryptionKeys = KeyMaster.GetKeys();

                // Validate required fields
                if (string.IsNullOrEmpty(request.Name))
                    throw new ArgumentException("Name must not be null!");

                // Create the new tree
                var NewTree = new Tree
                {
                    User = Context.Users.First(u => u.ID == UserID),
                    Created = DateTime.UtcNow,
                    Name = request.Name,
                    SelfID = null
                };

                Context.Trees.Add(NewTree);
                Context.SaveChanges();

                // Compile response
                return Results.Created($"{NewTree.ID}", new GetTreesResponse
                {
                    ID = NewTree.ID,
                    Name = NewTree.Name,
                    Member_Self_ID = NewTree.SelfID
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
            catch (KeyNotFoundException k)
            {
                Console.WriteLine(k);
                return Results.Problem(
                    detail: k.Message,
                    statusCode: 409);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Results.Problem(statusCode: 500);
            }
        })
        .WithName("PostTree")
        .WithOpenApi();


        app.MapPut("/trees/{id}", (int id, HttpContext httpContext, PutTreesRequest request) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();

                // Authenticate
                Authenticator.Authenticate(Context, httpContext: httpContext);

                // Modify and save user
                var Existing = Context.Trees
                    .First(t => t.ID == id);

                // Validate required fields
                if (string.IsNullOrEmpty(request.Name))
                    throw new ArgumentException("Name must not be null!");

                Existing.Name = request.Name;
                Existing.SelfID = request.Member_Self_ID;

                Context.SaveChanges();

                // Compile response
                return Results.Ok(new GetTreesResponse
                {
                    ID = Existing.ID,
                    Name = Existing.Name,
                    Member_Self_ID = Existing.SelfID
                });
            }
            catch (ArgumentException a)
            {
                Console.WriteLine(a);
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 400
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
            catch (Exception e) when (e is ArgumentNullException || e is InvalidOperationException)
            {
                Console.WriteLine(e);
                return Results.Problem(
                    detail: e.Message,
                    statusCode: 404
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Results.Problem(statusCode: 500);
            }
        })
        .WithName("PutTree")
        .WithOpenApi();

        app.MapGet("/trees/{id}", (int id, HttpContext httpContext) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();

                // Authenticate
                Authenticator.Authenticate(Context, httpContext: httpContext);

                // Compile response
                var Members = Context.Members
                    .Include(member => member.Tree)
                    .Where(member => member.Tree.ID == id)
                    .Select(member => new GetMembersResponse
                    {
                        ID = member.ID,
                        Fname = member.Fname,
                        Mnames = member.Mnames,
                        Lname = member.Lname,
                        Sex = member.Sex,
                        DOB = member.DOB,
                        DOD = member.DOD,
                        Spouses = Context.Spouses
                            .Select(spouse => new GetSpousesResponse
                            {
                                ID = spouse.ID,
                                Husband_ID = spouse.Husband.ID,
                                Wife_ID = spouse.Wife.ID,
                                Started = spouse.Started,
                                Ended = spouse.Ended
                            })
                            .Where(spouse => spouse.Husband_ID == member.ID || spouse.Wife_ID == member.ID)
                            .ToList(),
                        Children = Context.ParentalRelationships
                            .Select(pcr => new GetChildrenResponse
                            {
                                ID = pcr.ID,
                                Parent_ID = pcr.Parent.ID,
                                Child_ID = pcr.Child.ID,
                                Adopted = pcr.Adopted
                            })
                            .Where(pcr => pcr.Parent_ID == member.ID || pcr.Child_ID == member.ID)
                            .ToList()
                    })
                    .ToList();

                return Results.Ok(
                    Context.Trees
                    .Select(tree => new GetIndividualTreesResponse
                    {
                        ID = tree.ID,
                        Name = tree.Name,
                        Member_Self_ID = tree.SelfID,
                        Members = Members
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
        .WithName("GetTree")
        .WithOpenApi();

        app.MapGet("/trees/", (HttpContext httpContext) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();

                // Authenticate
                var (_, UserID) = Authenticator.Authenticate(Context, httpContext: httpContext);

                // Compile response
                return Results.Ok(
                    Context.Trees.Include(t => t.User)
                    .Where(t => t.User.ID == UserID)
                    .Select(tree => new GetTreesResponse
                    {
                        ID = tree.ID,
                        Name = tree.Name,
                        Member_Self_ID = tree.SelfID
                    })
                    .ToList()
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
        .WithName("GetTrees")
        .WithOpenApi();

        app.MapDelete("/trees/{id}", (int id, HttpContext httpContext) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();

                // Authenticate
                Authenticator.Authenticate(Context, httpContext: httpContext);

                // Find the user to delete
                var TreeToDelete = Context.Trees
                    .FirstOrDefault(t => t.ID == id) ??
                        throw new InvalidOperationException($"Tree with ID {id} not found.");

                Context.Trees.Remove(TreeToDelete);
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
        .WithName("DeleteTree")
        .WithOpenApi();
    }
}