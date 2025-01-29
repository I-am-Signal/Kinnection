using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Kinnection
{
    static class API
    {
        public static void APIs(WebApplication app)
        {
            app.MapGet("/results", async () =>
            {
                try
                {
                    using var context = DatabaseManager.GetActiveContext();
                    StringBuilder output = new StringBuilder();
                    output.Append('[');

                    var bookResponses = await context.Book
                        .Include(p => p.Publisher)
                        .Select(book => new BookResponse
                        {
                            ISBN = book.ISBN,
                            Title = book.Title,
                            Publisher = (book.Publisher != null) ? book.Publisher.Name : null
                        }).ToListAsync();

                    return Results.Ok(bookResponses);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return Results.Problem(statusCode: 500);
                }
            })
            .WithName("GetResults")
            .WithOpenApi();

            app.MapGet("/result/{title}", async (string title) =>
            {
                try
                {
                    using var context = DatabaseManager.GetActiveContext();
                    StringBuilder output = new StringBuilder();
                    output.Append('[');

                    var book = await context.Book
                        .Include(p => p.Publisher)
                        .SingleAsync(b => b.Title == title);

                    var bookResponse = new BookResponse
                    {
                        ISBN = book.ISBN,
                        Title = book.Title,
                        Publisher = book.Publisher?.Name
                    };

                    return Results.Ok(bookResponse);
                }
                catch (InvalidOperationException)
                {
                    return Results.Problem(detail: $"Result not found for title: {title}", statusCode: 404);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return Results.Problem(statusCode: 500);
                }
            })
            .WithName("GetResult")
            .WithOpenApi();

            app.MapPost("/insert", async (HttpContext httpContext, BookRequest request) =>
            {
                try
                {
                    using var context = DatabaseManager.GetActiveContext();

                    Publisher publisher;
                    try
                    {
                        publisher = await context.Publisher
                            .SingleAsync(p => p.Name == request.PublisherName);
                    }
                    catch (InvalidOperationException)
                    {
                        // doesn't work in current db migration, its still set to requiring publisher
                        publisher = null;
                    }

                    context.Book.Add(new Book
                    {
                        ISBN = request.ISBN,
                        Title = request.Title,
                        Author = request.Author,
                        Language = request.Language,
                        Pages = request.Pages,
                        Publisher = publisher
                    });

                    await context.SaveChangesAsync();
                    return Results.Ok(new OkResponse { message = $"{request.Title} was inserted" });
                }
                catch (DbUpdateException d)
                {
                    Console.WriteLine(d);
                    return Results.Problem(
                        detail: $"A book with title {request.Title} was previously inserted",
                        statusCode: 409
                    );
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return Results.Problem(statusCode: 500);
                }
            })
            .WithName("GetInputs")
            .WithOpenApi();

            app.MapDelete("/delete/{title}", async (string title) =>
            {
                try
                {
                    using var context = DatabaseManager.GetActiveContext();
                    context.Remove(await context.Book
                        .SingleAsync(b => b.Title == title));
                    await context.SaveChangesAsync();
                    return Results.Ok(new OkResponse
                    {
                        message = $"{title} was deleted."
                    });
                }
                catch (InvalidOperationException i)
                {
                    Console.WriteLine(i);
                    return Results.Problem(
                        detail: $"{title} could not be deleted. A book with title {title} does not exist.",
                        statusCode: 404
                    );
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return Results.Problem(statusCode: 500); ;
                }
            })
            .WithName("DeleteInputs")
            .WithOpenApi();

            app.Run();
        }
    }
}