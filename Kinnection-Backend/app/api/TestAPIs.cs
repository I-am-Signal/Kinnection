using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
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

                    var books = await context.Book
                        .Include(p => p.Publisher)
                        .ToListAsync();
                    
                    var bookResponses = books.Select(book => new BookResponse
                    {
                        ISBN = book.ISBN,
                        Title = book.Title,
                        Publisher = book.Publisher.Name
                    }).ToList();

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

            app.MapGet("/insert", async () =>
            {
                try
                {
                    using var context = DatabaseManager.GetActiveContext();
                    // Adds a publisher
                    var publisher = new Publisher
                    {
                        Name = "Mariner Books"
                    };
                    context.Publisher.Add(publisher);

                    // Adds some books
                    context.Book.Add(new Book
                    {
                        ISBN = "978-0544003415",
                        Title = "The Lord of the Rings",
                        Author = "J.R.R. Tolkien",
                        Language = "English",
                        Pages = 1216,
                        Publisher = publisher
                    });
                    context.Book.Add(new Book
                    {
                        ISBN = "978-0547247762",
                        Title = "The Sealed Letter",
                        Author = "Emma Donoghue",
                        Language = "English",
                        Pages = 416,
                        Publisher = publisher
                    });

                    // Saves changes
                    await context.SaveChangesAsync();
                    return Results.Ok(new { message = "Data was inserted" });
                }
                catch (DbUpdateException d)
                {
                    Console.WriteLine(d);
                    return Results.BadRequest(new { message = "Data was previously inserted" });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return Results.BadRequest(new { message = e.ToString() });
                }
            })
            .WithName("GetInputs")
            .WithOpenApi();

            app.MapDelete("/delete", async () =>
            {
                try
                {
                    using var context = DatabaseManager.GetActiveContext();
                    Publisher mariner = await context.Publisher
                        .SingleAsync(p => p.Name == "Mariner Books");
                    Book lotr = await context.Book
                        .SingleAsync(b => b.Title == "The Lord of the Rings");
                    Book sl = await context.Book
                        .SingleAsync(b => b.Title == "The Sealed Letter");
                    context.Remove(lotr);
                    context.Remove(sl);
                    context.Remove(mariner);
                    await context.SaveChangesAsync();
                    return Results.Ok(new { message = "Data was deleted" });
                }
                catch (DbUpdateException d)
                {
                    Console.WriteLine(d);
                    return Results.BadRequest(new { message = "Data was previously inserted" });
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