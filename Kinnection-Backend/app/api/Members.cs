using System.Security.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Kinnection;

static class MemberAPIs
{
    /// <summary>
    /// Compiles and returns a complete GetIndividualMembersResponse object from a Member object.
    /// </summary>
    /// <param name="member"></param>
    /// <param name="Context"></param>
    /// <returns>GetIndividualMembersresponse</returns>
    private static GetIndividualMembersResponse CompileWholeMember(Member member, KinnectionContext Context)
    {
        return new GetIndividualMembersResponse
        {
            ID = member.ID,
            Fname = member.Fname,
            Mnames = member.Mnames,
            Lname = member.Lname,
            Sex = member.Sex,
            DOB = member.DOB,
            Birthplace = member.Birthplace,
            DOD = member.DOD,
            Deathplace = member.Deathplace,
            Death_cause = member.CauseOfDeath,
            Ethnicity = member.Ethnicity,
            Biography = member.Biography,
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
            Education_history = Context.Educations
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
            Work_history = Context.Works
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
                .ToList()
        };
    }

    public static void APIs(WebApplication app)
    {
        app.MapPost("/trees/{tree_id}/members", (int tree_id, HttpContext httpContext, PostMembersRequest Request) =>
        {
            try
            {
                using var Context = DatabaseManager.GetActiveContext();
                Authenticator.Authenticate(Context: Context, httpContext: httpContext);

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
                    Birthplace = NewMember.Birthplace,
                    DOD = NewMember.DOD,
                    Deathplace = NewMember.Deathplace,
                    Death_cause = NewMember.CauseOfDeath,
                    Ethnicity = NewMember.Ethnicity,
                    Biography = NewMember.Biography
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

                // Modify User
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

                // Add or Modify ParentChildRelationships
                foreach (var child in Request.Children)
                {
                    var ChildMember = Context.Members
                        .First(m => m.ID == child.Child_ID);
                    var ParentMember = Context.Members
                        .First(m => m.ID == child.Parent_ID);

                    if (child.ID == null)
                    {
                        Context.ParentalRelationships.Add(new ParentalRelationship
                        {
                            Created = DateTime.UtcNow,
                            Child = ChildMember,
                            Parent = ParentMember,
                            Adopted = child.Adopted
                        });
                    }
                    else
                    {
                        var rel = Context.ParentalRelationships
                            .First(p => p.ID == child.ID);
                        rel.Child = ChildMember;
                        rel.Parent = ParentMember;
                        rel.Adopted = child.Adopted;
                    }
                }

                // Add or Modify Educations
                foreach (var ReqEdu in Request.Education_history)
                {
                    if (ReqEdu.ID == null)
                    {
                        Context.Educations.Add(new Education
                        {
                            Created = DateTime.UtcNow,
                            Started = ReqEdu.Started,
                            Ended = ReqEdu.Ended,
                            Title = ReqEdu.Title,
                            Organization = ReqEdu.Organization,
                            Description = ReqEdu.Description,
                            Member = Existing
                        });
                    }
                    else
                    {
                        var Education = Context.Educations
                            .First(e => e.ID == ReqEdu.ID);
                        Education.Started = ReqEdu.Started;
                        Education.Ended = ReqEdu.Ended;
                        Education.Title = ReqEdu.Title;
                        Education.Organization = ReqEdu.Organization;
                        Education.Description = ReqEdu.Description;
                    }
                }

                // Add or Modify Emails
                foreach (var ReqEmail in Request.Emails)
                {
                    if (ReqEmail.ID == null)
                    {
                        Context.Emails.Add(new MemberEmail
                        {
                            Created = DateTime.UtcNow,
                            Email = ReqEmail.Email,
                            Primary = ReqEmail.Primary,
                            Member = Existing
                        });
                    }
                    else
                    {
                        var Email = Context.Emails
                            .First(e => e.ID == ReqEmail.ID);
                        Email.Email = ReqEmail.Email;
                        Email.Primary = ReqEmail.Primary;
                    }
                }

                // Add or Modify Hobbies
                foreach (var ReqHobby in Request.Hobbies)
                {
                    if (ReqHobby.ID == null)
                    {
                        Context.Hobbies.Add(new Hobby
                        {
                            Created = DateTime.UtcNow,
                            Started = ReqHobby.Started,
                            Ended = ReqHobby.Ended,
                            Title = ReqHobby.Title,
                            Organization = ReqHobby.Organization,
                            Description = ReqHobby.Description,
                            Member = Existing
                        });
                    }
                    else
                    {
                        var Hobby = Context.Hobbies
                            .First(h => h.ID == ReqHobby.ID);
                        Hobby.Started = ReqHobby.Started;
                        Hobby.Ended = ReqHobby.Ended;
                        Hobby.Title = ReqHobby.Title;
                        Hobby.Organization = ReqHobby.Organization;
                        Hobby.Description = ReqHobby.Description;
                    }
                }

                // Add or Modify Phones
                foreach (var ReqPhone in Request.Phones)
                {
                    if (ReqPhone.ID == null)
                    {
                        Context.Phones.Add(new MemberPhone
                        {
                            Created = DateTime.UtcNow,
                            Phone = ReqPhone.Phone_Number,
                            Primary = ReqPhone.Primary,
                            Member = Existing
                        });
                    }
                    else
                    {
                        var Phone = Context.Phones
                            .First(p => p.ID == ReqPhone.ID);
                        Phone.Phone = ReqPhone.Phone_Number;
                        Phone.Primary = ReqPhone.Primary;
                    }
                }

                // Add or Modify Residences
                foreach (var ReqRes in Request.Residences)
                {
                    if (ReqRes.ID == null)
                    {
                        Context.Residences.Add(new Residence
                        {
                            Created = DateTime.UtcNow,
                            Address_Line_1 = ReqRes.Addr_Line_1,
                            Address_Line_2 = ReqRes.Addr_Line_2,
                            City = ReqRes.City,
                            State = ReqRes.State,
                            Country = ReqRes.Country,
                            Started = ReqRes.Started,
                            Ended = ReqRes.Ended,
                            Member = Existing
                        });
                    }
                    else
                    {
                        var Residence = Context.Residences
                            .First(r => r.ID == ReqRes.ID);
                        Residence.Address_Line_1 = ReqRes.Addr_Line_1;
                        Residence.Address_Line_2 = ReqRes.Addr_Line_2;
                        Residence.City = ReqRes.City;
                        Residence.State = ReqRes.State;
                        Residence.Country = ReqRes.Country;
                        Residence.Started = ReqRes.Started;
                        Residence.Ended = ReqRes.Ended;
                    }
                }

                // Add or Modify Spouses
                foreach (var ReqSpouse in Request.Spouses)
                {
                    var Husband = Context.Members.First(m => m.ID == ReqSpouse.Husband_ID);
                    var Wife = Context.Members.First(m => m.ID == ReqSpouse.Wife_ID);

                    if (ReqSpouse.ID == null)
                    {
                        Context.Spouses.Add(new Spouse
                        {
                            Created = DateTime.UtcNow,
                            Husband = Husband,
                            Wife = Wife,
                            Started = ReqSpouse.Started,
                            Ended = ReqSpouse.Ended
                        });
                    }
                    else
                    {
                        var Spouse = Context.Spouses.First(s =>
                            s.Husband.ID == ReqSpouse.Husband_ID ||
                            s.Wife.ID == ReqSpouse.Wife_ID);
                        Spouse.Husband = Husband;
                        Spouse.Wife = Wife;
                        Spouse.Started = ReqSpouse.Started;
                        Spouse.Ended = ReqSpouse.Ended;
                    }
                }

                // Add or Modify Works
                foreach (var ReqWork in Request.Work_history)
                {
                    if (ReqWork.ID == null)
                    {
                        Context.Works.Add(new Work
                        {
                            Created = DateTime.UtcNow,
                            Started = ReqWork.Started,
                            Ended = ReqWork.Ended,
                            Title = ReqWork.Title,
                            Organization = ReqWork.Organization,
                            Description = ReqWork.Description,
                            Member = Existing
                        });
                    }
                    else
                    {
                        var Work = Context.Works
                            .First(w => w.ID == ReqWork.ID);
                        Work.Started = ReqWork.Started;
                        Work.Ended = ReqWork.Ended;
                        Work.Title = ReqWork.Title;
                        Work.Organization = ReqWork.Organization;
                        Work.Description = ReqWork.Description;
                    }
                }

                Context.SaveChanges();

                // Compile response
                return Results.Ok(CompileWholeMember(Existing, Context));
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
                    .Single();

                return Results.Ok(CompileWholeMember(Member, Context));
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

                // Remove Parent/Child relationships
                Context.ParentalRelationships.RemoveRange(
                    Context.ParentalRelationships.Where(
                        pcr => pcr.Parent == MemberToDelete || pcr.Child == MemberToDelete));

                // Remove Education history
                Context.Educations.RemoveRange(
                    Context.Educations.Where(e => e.Member == MemberToDelete));
                
                // Remove Emails
                Context.Emails.RemoveRange(
                    Context.Emails.Where(e => e.Member == MemberToDelete));
                
                // Remove Hobbies
                Context.Hobbies.RemoveRange(
                    Context.Hobbies.Where(h => h.Member == MemberToDelete));
                
                // Remove Phones
                Context.Phones.RemoveRange(
                    Context.Phones.Where(p => p.Member == MemberToDelete));
                
                // Remove Residences
                Context.Residences.RemoveRange(
                    Context.Residences.Where(r => r.Member == MemberToDelete));
                
                // Remove Spouses
                Context.Spouses.RemoveRange(
                    Context.Spouses.Where(
                        s => s.Wife == MemberToDelete || s.Husband == MemberToDelete));
                
                // Remove Work history
                Context.Works.RemoveRange(
                    Context.Works.Where(w => w.Member == MemberToDelete));
                
                // Finally remove the Member
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