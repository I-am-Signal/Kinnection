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
                var (_, UserID) = Authenticator.Authenticate(Context, httpContext: httpContext);

                // Validate required fields
                if (string.IsNullOrEmpty(request.Name))
                    throw new ArgumentException("Name must not be null!");

                // Create the new tree
                var NewTree = new Tree
                {
                    // Check authorization
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
                    Id = NewTree.ID,
                    Name = NewTree.Name,
                    Member_self_id = NewTree.SelfID
                });
            }
            catch (ArgumentException a)
            {
                Console.WriteLine($"Issue with POST /trees/: {a}");
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 400);
            }
            catch (AuthenticationException a)
            {
                Console.WriteLine($"Issue with POST /trees/: {a}");
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 401
                );
            }
            catch (KeyNotFoundException k)
            {
                Console.WriteLine($"Issue with POST /trees/: {k}");
                return Results.Problem(
                    detail: k.Message,
                    statusCode: 409);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Issue with POST /trees/: {e}");
                return Results.Problem(statusCode: 500);
            }
        })
        .WithName("PostTree")
        .WithOpenApi();


        app.MapPut("/trees/{tree_id}", (int tree_id, HttpContext httpContext, PutTreesRequest request) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();

                // Authenticate
                var (_, UserID) = Authenticator.Authenticate(Context, httpContext: httpContext);

                // Modify and save user
                var Existing = Context.Trees
                    // Check authorization
                    .First(t => t.ID == tree_id && t.User.ID == UserID);

                // Validate required fields
                if (string.IsNullOrEmpty(request.Name))
                    throw new ArgumentException("Name must not be null!");

                Existing.Name = request.Name;
                Existing.SelfID = request.Member_self_id;

                Context.SaveChanges();

                // Compile response
                return Results.Ok(
                    new GetTreesResponse
                    {
                        Id = Existing.ID,
                        Name = Existing.Name,
                        Member_self_id = Existing.SelfID
                    });
            }
            catch (ArgumentException a)
            {
                Console.WriteLine($"Issue with PUT /trees/{{tree_id}}: {a}");
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 400
                );
            }
            catch (AuthenticationException a)
            {
                Console.WriteLine($"Issue with PUT /trees/{{tree_id}}: {a}");
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 401
                );
            }
            catch (Exception e) when (e is ArgumentNullException || e is InvalidOperationException)
            {
                Console.WriteLine($"Issue with PUT /trees/{{tree_id}}: {e}");
                return Results.Problem(
                    detail: e.Message,
                    statusCode: 404
                );
            }
            catch (Exception e)
            {
                Console.WriteLine($"Issue with PUT /trees/{{tree_id}}: {e}");
                return Results.Problem(statusCode: 500);
            }
        })
        .WithName("PutTree")
        .WithOpenApi();

        app.MapGet("/trees/{tree_id}", (int tree_id, HttpContext httpContext) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();

                // Authenticate
                var (_, UserID) = Authenticator.Authenticate(Context, httpContext: httpContext);

                // Compile response
                var Tree = Context.Trees
                    // Check authorization
                    .First(t => t.ID == tree_id && t.User.ID == UserID);

                var Members = Context.Members
                    .Include(member => member.Tree)
                    .Where(member => member.Tree.ID == tree_id && member.Tree.User.ID == UserID)
                    .Select(member => new GetTreesMembersResponse
                    {
                        Id = member.ID,
                        Fname = member.Fname,
                        Mnames = member.Mnames,
                        Lname = member.Lname,
                        Sex = member.Sex,
                        Dob = member.DOB,
                        Dod = member.DOD,
                        Spouses = Context.Spouses
                            .Select(spouse => new GetSpousesResponse
                            {
                                Id = spouse.ID,
                                Husband_id = spouse.Husband.ID,
                                Wife_id = spouse.Wife.ID,
                                Started = spouse.Started,
                                Ended = spouse.Ended
                            })
                            .Where(spouse => spouse.Husband_id == member.ID || spouse.Wife_id == member.ID)
                            .ToList(),
                        Children = Context.ParentalRelationships
                            .Select(pcr => new GetChildrenResponse
                            {
                                Id = pcr.ID,
                                Parent_id = pcr.Parent.ID,
                                Child_id = pcr.Child.ID,
                                Adopted = pcr.Adopted
                            })
                            .Where(pcr => pcr.Parent_id == member.ID || pcr.Child_id == member.ID)
                            .ToList()
                    })
                    .ToList();

                return Results.Ok(
                    new GetIndividualTreesResponse
                    {
                        Id = Tree.ID,
                        Name = Tree.Name,
                        Member_self_id = Tree.SelfID,
                        Members = Members
                    });
            }
            catch (AuthenticationException a)
            {
                Console.WriteLine($"Issue with GET /trees/{{tree_id}}: {a}");
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 401
                );
            }
            catch (InvalidOperationException i)
            {
                Console.WriteLine($"Issue with GET /trees/{{tree_id}}: {i}");
                return Results.Problem(
                    detail: i.Message,
                    statusCode: 404
                );
            }
            catch (Exception e)
            {
                Console.WriteLine($"Issue with GET /trees/{{tree_id}}: {e}");
                return Results.Problem(statusCode: 500);
            }
        })
        .WithName("GetTree")
        .WithOpenApi();

        app.MapGet("/{user_id}/trees/", (int user_id, HttpContext httpContext) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();

                // Authenticate
                var (_, UserID) = Authenticator.Authenticate(Context, httpContext: httpContext);

                if (UserID != user_id)
                    throw new AuthenticationException("Cannot view family trees other than your own.");

                // Compile response
                return Results.Ok(
                    new GetAllTreesResponse
                    {
                        Trees = Context.Trees
                            .Where(t => t.User.ID == UserID)
                            .Select(tree => new GetTreesResponse
                            {
                                Id = tree.ID,
                                Name = tree.Name,
                                Member_self_id = tree.SelfID
                            })
                            .OrderBy(t => t.Id)
                            .ToList()
                    }
                );
            }
            catch (AuthenticationException a)
            {
                Console.WriteLine($"Issue with GET /trees/: {a}");
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 401
                );
            }
            catch (InvalidOperationException i)
            {
                Console.WriteLine($"Issue with GET /trees/: {i}");
                return Results.Problem(
                    detail: i.Message,
                    statusCode: 404
                );
            }
            catch (Exception e)
            {
                Console.WriteLine($"Issue with GET /trees/: {e}");
                return Results.Problem(statusCode: 500);
            }
        })
        .WithName("GetTrees")
        .WithOpenApi();

        app.MapDelete("/trees/{tree_id}", (int tree_id, HttpContext httpContext) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();

                // Authenticate
                var (_, UserID) = Authenticator.Authenticate(Context, httpContext: httpContext);

                // Find the user to delete
                var TreeToDelete = Context.Trees
                    .FirstOrDefault(t => t.ID == tree_id && t.User.ID == UserID) ??
                        throw new InvalidOperationException($"Tree with ID {tree_id} not found.");

                Context.Trees.Remove(TreeToDelete);
                Context.SaveChanges();

                // Return a 204 No Content response
                return Results.NoContent();
            }
            catch (AuthenticationException a)
            {
                Console.WriteLine($"Issue with DELETE /trees/{{tree_id}}: {a}");
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 401
                );
            }
            catch (InvalidOperationException i)
            {
                Console.WriteLine($"Issue with DELETE /trees/{{tree_id}}: {i}");
                return Results.Problem(
                    detail: i.Message,
                    statusCode: 404
                );
            }
            catch (Exception e)
            {
                Console.WriteLine($"Issue with DELETE /trees/{{tree_id}}: {e}");
                return Results.Problem(statusCode: 500);
            }
        })
        .WithName("DeleteTree")
        .WithOpenApi();
    }
}