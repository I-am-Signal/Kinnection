using System.Security.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Kinnection;

static class MemberAPIs
{
    public static void APIs(WebApplication app)
    {
        app.MapPost("/trees/{tree_id}/members", (int tree_id, HttpContext httpContext, PostMembersRequest Request) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();
                var (_, UserID) = Authenticator.Authenticate(
                    Context: Context, httpContext: httpContext);

                var EncryptionKeys = KeyMaster.GetKeys();

                // Create the new member
                var NewMember = new Member
                {
                    Tree = Context.Trees.First(t => t.ID == tree_id),
                    Created = DateTime.UtcNow,
                    Fname = Request.Fname,
                    Mnames = Request.Mnames,
                    Lname = Request.Lname,
                    Sex = Request.Sex,
                    DOB = Request.DOB,
                    Birthplace = Request.Birthplace,
                    DOD = Request.DOD,
                    Deathplace = Request.Deathplace,
                    CauseOfDeath = Request.Death_Cause,
                    Ethnicity = Request.Ethnicity,
                    Biography = Request.Biography
                };

                Context.Members.Add(NewMember);
                Context.SaveChanges();

                // Compile response
                return Results.Created($"{NewMember.ID}", new GetMembersResponse
                {
                    ID = NewMember.ID,
                    Fname = NewMember.Fname,
                    Mnames = NewMember.Mnames,
                    Lname = NewMember.Lname,
                    Sex = NewMember.Sex,
                    DOB = NewMember.DOB,
                    DOD = NewMember.DOD,
                    Spouses = Context.Spouses
                        .Select(spouse => new GetSpousesResponse
                        {
                            ID = spouse.ID,
                            Husband_ID = spouse.Husband.ID,
                            Wife_ID = spouse.Wife.ID,
                            Started = spouse.Started,
                            Ended = spouse.Ended
                        })
                        .Where(spouse => spouse.Husband_ID == NewMember.ID || spouse.Wife_ID == NewMember.ID)
                        .ToList(),
                    Children = Context.ParentalRelationships
                        .Select(pcr => new GetChildrenResponse
                        {
                            ID = pcr.ID,
                            Parent_ID = pcr.Parent.ID,
                            Child_ID = pcr.Child.ID,
                            Adopted = pcr.Adopted
                        })
                        .Where(pcr => pcr.Parent_ID == NewMember.ID || pcr.Child_ID == NewMember.ID)
                        .ToList()
                });
            }
            catch (ArgumentException a)
            {
                Console.WriteLine($"Issue with POST /trees/{{tree_id}}/members/: {a}");
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 400);
            }
            catch (AuthenticationException a)
            {
                Console.WriteLine($"Issue with POST /trees/{{tree_id}}/members/: {a}");
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 401
                );
            }
            catch (KeyNotFoundException k)
            {
                Console.WriteLine($"Issue with POST /trees/{{tree_id}}/members/: {k}");
                return Results.Problem(
                    detail: k.Message,
                    statusCode: 409);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Issue with POST /trees/{{tree_id}}/members/: {e}");
                return Results.Problem(statusCode: 500);
            }
        })
        .WithName("PostMember")
        .WithOpenApi();


        app.MapPut("/trees/members/{member_id}", (int member_id, HttpContext httpContext, PutMembersRequest Request) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();

                // Authenticate
                Authenticator.Authenticate(Context, httpContext: httpContext);

                // Modify and save user
                var Existing = Context.Members
                    .First(m => m.ID == member_id);

                Existing.Fname = Request.Fname;
                Existing.Mnames = Request.Mnames;
                Existing.Lname = Request.Lname;
                Existing.Sex = Request.Sex;
                Existing.DOB = Request.DOB;
                Existing.Birthplace = Request.Birthplace;
                Existing.DOD = Request.DOD;
                Existing.Deathplace = Request.Deathplace;
                Existing.CauseOfDeath = Request.Death_Cause;
                Existing.Ethnicity = Request.Ethnicity;
                Existing.Biography = Request.Biography;

                Context.SaveChanges();

                // Compile response
                return Results.Ok(new GetMembersResponse
                {
                    ID = Existing.ID,
                    Fname = Existing.Fname,
                    Mnames = Existing.Mnames,
                    Lname = Existing.Lname,
                    Sex = Existing.Sex,
                    DOB = Existing.DOB,
                    DOD = Existing.DOD,
                    Spouses = Context.Spouses
                        .Select(spouse => new GetSpousesResponse
                        {
                            ID = spouse.ID,
                            Husband_ID = spouse.Husband.ID,
                            Wife_ID = spouse.Wife.ID,
                            Started = spouse.Started,
                            Ended = spouse.Ended
                        })
                        .Where(spouse => spouse.Husband_ID == Existing.ID || spouse.Wife_ID == Existing.ID)
                        .ToList(),
                    Children = Context.ParentalRelationships
                        .Select(pcr => new GetChildrenResponse
                        {
                            ID = pcr.ID,
                            Parent_ID = pcr.Parent.ID,
                            Child_ID = pcr.Child.ID,
                            Adopted = pcr.Adopted
                        })
                        .Where(pcr => pcr.Parent_ID == Existing.ID || pcr.Child_ID == Existing.ID)
                        .ToList()
                });
            }
            catch (ArgumentException a)
            {
                Console.WriteLine($"Issue with PUT /trees/members/{{member_id}}: {a}");
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 400
                );
            }
            catch (AuthenticationException a)
            {
                Console.WriteLine($"Issue with PUT /trees/members/{{member_id}}: {a}");
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 401
                );
            }
            catch (Exception e) when (e is ArgumentNullException || e is InvalidOperationException)
            {
                Console.WriteLine($"Issue with PUT /trees/members/{{member_id}}: {e}");
                return Results.Problem(
                    detail: e.Message,
                    statusCode: 404
                );
            }
            catch (Exception e)
            {
                Console.WriteLine($"Issue with PUT /trees/members/{{member_id}}: {e}");
                return Results.Problem(statusCode: 500);
            }
        })
        .WithName("PutMember")
        .WithOpenApi();

        app.MapGet("/trees/members/{member_id}", (int member_id, HttpContext httpContext) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();

                // Authenticate
                Authenticator.Authenticate(Context, httpContext: httpContext);

                // Compile response
                var Member = Context.Members
                    .Where(member => member.ID == member_id)
                    .Select(member => new GetIndividualMembersResponse
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
                            .ToList(),
                        Birthplace = member.Birthplace,
                        Deathplace = member.Deathplace,
                        Death_Cause = member.CauseOfDeath,
                        Ethnicity = member.Ethnicity,
                        Biography = member.Biography,
                        Residences = Context.Residences
                            .Include(residence => residence.Member)
                            .Where(residence => residence.Member.ID == member.ID)
                            .Select(residence => new GetResidencesResponse
                            {
                                ID = residence.ID,
                                Addr_Line_1 = residence.Address_Line_1,
                                Addr_Line_2 = residence.Address_Line_2,
                                City = residence.City,
                                State = residence.State,
                                Country = residence.Country
                            })
                            .ToList(),
                        Emails = Context.Emails
                            .Include(email => email.Member)
                            .Where(email => email.Member.ID == member.ID)
                            .Select(email => new GetEmailsResponse
                            {
                                ID = email.ID,
                                Primary = email.Primary,
                                Email = email.Email
                            })
                            .ToList(),
                        Phones = Context.Phones
                            .Include(phone => phone.Member)
                            .Where(phone => phone.Member.ID == member.ID)
                            .Select(phone => new GetPhonesResponse
                            {
                                ID = phone.ID,
                                Primary = phone.Primary,
                                Phone_Number = phone.Phone
                            })
                            .ToList(),
                        Work_History = Context.Works
                            .Include(work => work.Member)
                            .Where(work => work.Member.ID == member.ID)
                            .Select(work => new GetWorkResponse
                            {
                                ID = work.ID,
                                Started = work.Started,
                                Ended = work.Ended,
                                Title = work.Title,
                                Organization = work.Organization,
                                Description = work.Description
                            })
                            .ToList(),
                        Education_History = Context.Educations
                            .Include(education => education.Member)
                            .Where(education => education.Member.ID == member.ID)
                            .Select(education => new GetEducationResponse
                            {
                                ID = education.ID,
                                Started = education.Started,
                                Ended = education.Ended,
                                Title = education.Title,
                                Organization = education.Organization,
                                Description = education.Description
                            })
                            .ToList(),
                        Hobbies = Context.Hobbies
                            .Include(hobby => hobby.Member)
                            .Where(hobby => hobby.Member.ID == member.ID)
                            .Select(hobby => new GetHobbiesResponse
                            {
                                ID = hobby.ID,
                                Started = hobby.Started,
                                Ended = hobby.Ended,
                                Title = hobby.Title,
                                Organization = hobby.Organization,
                                Description = hobby.Description
                            })
                            .ToList()
                    })
                    .Single();

                return Results.Ok(Member);
            }
            catch (AuthenticationException a)
            {
                Console.WriteLine($"Issue with GET /trees/members/{{member_id}}: {a}");
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 401
                );
            }
            catch (InvalidOperationException i)
            {
                Console.WriteLine($"Issue with GET /trees/members/{{member_id}}: {i}");
                return Results.Problem(
                    detail: i.Message,
                    statusCode: 404
                );
            }
            catch (Exception e)
            {
                Console.WriteLine($"Issue with GET /trees/members/{{member_id}}: {e}");
                return Results.Problem(statusCode: 500);
            }
        })
        .WithName("GetMember")
        .WithOpenApi();

        app.MapGet("/trees/{tree_id}/members/", (int tree_id, HttpContext httpContext) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();

                // Authenticate
                var (_, UserID) = Authenticator.Authenticate(Context, httpContext: httpContext);

                // Compile response
                var Members = Context.Members
                    .Include(member => member.Tree)
                    .Where(member => member.Tree.ID == tree_id)
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

                // Compile response
                return Results.Ok(Members);
            }
            catch (AuthenticationException a)
            {
                Console.WriteLine($"Issue with GET /trees/{{tree_id}}/members/: {a}");
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 401
                );
            }
            catch (InvalidOperationException i)
            {
                Console.WriteLine($"Issue with GET /trees/{{tree_id}}/members/: {i}");
                return Results.Problem(
                    detail: i.Message,
                    statusCode: 404
                );
            }
            catch (Exception e)
            {
                Console.WriteLine($"Issue with GET /trees/{{tree_id}}/members/: {e}");
                return Results.Problem(statusCode: 500);
            }
        })
        .WithName("GetMembers")
        .WithOpenApi();

        app.MapDelete("/trees/members/{member_id}", (int member_id, HttpContext httpContext) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();

                // Authenticate
                Authenticator.Authenticate(Context, httpContext: httpContext);

                // Find the user to delete
                var MemberToDelete = Context.Members
                    .FirstOrDefault(m => m.ID == member_id) ??
                        throw new InvalidOperationException($"Member with ID {member_id} not found.");

                Context.Members.Remove(MemberToDelete);
                Context.SaveChanges();

                // Return a 204 No Content response
                return Results.NoContent();
            }
            catch (AuthenticationException a)
            {
                Console.WriteLine($"Issue with DELETE /trees/members/{{member_id}}: {a}");
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 401
                );
            }
            catch (InvalidOperationException i)
            {
                Console.WriteLine($"Issue with DELETE /trees/members/{{member_id}}: {i}");
                return Results.Problem(
                    detail: i.Message,
                    statusCode: 404
                );
            }
            catch (Exception e)
            {
                Console.WriteLine($"Issue with DELETE /trees/members/{{member_id}}: {e}");
                return Results.Problem(statusCode: 500);
            }
        })
        .WithName("DeleteMember")
        .WithOpenApi();
    }
}