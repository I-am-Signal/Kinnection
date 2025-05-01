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
            Id = member.ID,
            Fname = member.Fname,
            Mnames = member.Mnames,
            Lname = member.Lname,
            Sex = member.Sex,
            Dob = member.DOB,
            Birthplace = member.Birthplace,
            Dod = member.DOD,
            Deathplace = member.Deathplace,
            Death_cause = member.CauseOfDeath,
            Ethnicity = member.Ethnicity,
            Biography = member.Biography,
            Children = Context.ParentalRelationships
                .Select(pcr => new GetChildrenResponse
                {
                    Id = pcr.ID,
                    Parent_id = pcr.Parent.ID,
                    Child_id = pcr.Child.ID,
                    Adopted = pcr.Adopted
                })
                .OrderBy(pcr => pcr.Id)
                .Where(pcr => pcr.Parent_id == member.ID || pcr.Child_id == member.ID)
                .ToList(),
            Education_history = Context.Educations
                .Include(education => education.Member)
                .Where(education => education.Member.ID == member.ID)
                .Select(education => new GetEducationResponse
                {
                    Id = education.ID,
                    Started = education.Started,
                    Ended = education.Ended,
                    Title = education.Title,
                    Organization = education.Organization,
                    Description = education.Description
                })
                .OrderBy(education => education.Id)
                .ToList(),
            Emails = Context.Emails
                .Include(email => email.Member)
                .Where(email => email.Member.ID == member.ID)
                .Select(email => new GetEmailsResponse
                {
                    Id = email.ID,
                    Primary = email.Primary,
                    Email = email.Email
                })
                .OrderBy(email => email.Id)
                .ToList(),
            Hobbies = Context.Hobbies
                .Include(hobby => hobby.Member)
                .Where(hobby => hobby.Member.ID == member.ID)
                .Select(hobby => new GetHobbiesResponse
                {
                    Id = hobby.ID,
                    Started = hobby.Started,
                    Ended = hobby.Ended,
                    Title = hobby.Title,
                    Organization = hobby.Organization,
                    Description = hobby.Description
                })
                .OrderBy(hobby => hobby.Id)
                .ToList(),
            Phones = Context.Phones
                .Include(phone => phone.Member)
                .Where(phone => phone.Member.ID == member.ID)
                .Select(phone => new GetPhonesResponse
                {
                    Id = phone.ID,
                    Primary = phone.Primary,
                    Phone_number = phone.Phone
                })
                .OrderBy(phone => phone.Id)
                .ToList(),
            Residences = Context.Residences
                .Include(residence => residence.Member)
                .Where(residence => residence.Member.ID == member.ID)
                .Select(residence => new GetResidencesResponse
                {
                    Id = residence.ID,
                    Addr_line_1 = residence.Address_Line_1,
                    Addr_line_2 = residence.Address_Line_2,
                    City = residence.City,
                    State = residence.State,
                    Country = residence.Country
                })
                .OrderBy(residence => residence.Id)
                .ToList(),
            Spouses = Context.Spouses
                .Select(spouse => new GetSpousesResponse
                {
                    Id = spouse.ID,
                    Husband_id = spouse.Husband.ID,
                    Wife_id = spouse.Wife.ID,
                    Started = spouse.Started,
                    Ended = spouse.Ended
                })
                .OrderBy(spouse => spouse.Id)
                .Where(spouse => spouse.Husband_id == member.ID || spouse.Wife_id == member.ID)
                .ToList(),
            Work_history = Context.Works
                .Include(work => work.Member)
                .Where(work => work.Member.ID == member.ID)
                .Select(work => new GetWorkResponse
                {
                    Id = work.ID,
                    Started = work.Started,
                    Ended = work.Ended,
                    Title = work.Title,
                    Organization = work.Organization,
                    Description = work.Description
                })
                .OrderBy(work => work.Id)
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
                    DOB = Request.Dob,
                    Birthplace = Request.Birthplace,
                    DOD = Request.Dod,
                    Deathplace = Request.Deathplace,
                    CauseOfDeath = Request.Death_cause,
                    Ethnicity = Request.Ethnicity,
                    Biography = Request.Biography
                };

                Context.Members.Add(NewMember);
                Context.SaveChanges();

                // Compile response
                return Results.Created($"{NewMember.ID}", CompileWholeMember(NewMember, Context));
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


        app.MapPut("/trees/{tree_id}/members/{member_id}", (
            int tree_id, int member_id, HttpContext httpContext, PutMembersRequest Request) =>
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
                Existing.DOB = Request.Dob;
                Existing.Birthplace = Request.Birthplace;
                Existing.DOD = Request.Dod;
                Existing.Deathplace = Request.Deathplace;
                Existing.CauseOfDeath = Request.Death_cause;
                Existing.Ethnicity = Request.Ethnicity;
                Existing.Biography = Request.Biography;

                // Get list of all members of the tree
                var Members = Context.Members
                    .Where(m => m.Tree.ID == tree_id)
                    .ToDictionary(m => m.ID);

                // Remove Parental Relationships that do not exist
                var NewIDs = Request.Children.Select(c => c.Id).ToList();
                var PRsToDelete = Context.ParentalRelationships
                    .Where(p => !NewIDs.Contains(p.ID)
                        && (p.Child == Existing || p.Parent == Existing));
                Context.ParentalRelationships.RemoveRange(PRsToDelete);

                // Add or Modify ParentalRelationships
                foreach (var child in Request.Children)
                {
                    var ChildMember = Members[child.Child_id];
                    var ParentMember = Members[child.Parent_id];

                    if (child.Id == null)
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
                            .FirstOrDefault(p => p.ID == child.Id) ??
                            throw new InvalidOperationException(
                                $"An education history with ID {child.Id} was not found.");
                        rel.Child = ChildMember;
                        rel.Parent = ParentMember;
                        rel.Adopted = child.Adopted;
                    }
                }

                // Remove Education Histories that do not exist
                NewIDs = Request.Education_history.Select(e => e.Id).ToList();
                var EdusToDelete = Context.Educations
                    .Where(e => !NewIDs.Contains(e.ID) && e.Member == Existing);
                Context.Educations.RemoveRange(EdusToDelete);

                // Add or Modify Educations
                foreach (var ReqEdu in Request.Education_history)
                {
                    if (ReqEdu.Id == null)
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
                            .FirstOrDefault(e => e.ID == ReqEdu.Id) ??
                            throw new InvalidOperationException(
                                $"An education history with ID {ReqEdu.Id} was not found.");
                        Education!.Started = ReqEdu.Started;
                        Education.Ended = ReqEdu.Ended;
                        Education.Title = ReqEdu.Title;
                        Education.Organization = ReqEdu.Organization;
                        Education.Description = ReqEdu.Description;
                    }
                }

                // Remove Emails that do not exist
                NewIDs = Request.Emails.Select(e => e.Id).ToList();
                var EmailsToDelete = Context.Emails
                    .Where(e => !NewIDs.Contains(e.ID) && e.Member == Existing);
                Context.Emails.RemoveRange(EmailsToDelete);

                // Add or Modify Emails
                foreach (var ReqEmail in Request.Emails)
                {
                    if (ReqEmail.Id == null)
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
                            .FirstOrDefault(e => e.ID == ReqEmail.Id) ??
                            throw new InvalidOperationException(
                                $"An email with ID {ReqEmail.Id} was not found.");
                        Email.Email = ReqEmail.Email;
                        Email.Primary = ReqEmail.Primary;
                    }
                }

                // Remove Hobbies that do not exist
                NewIDs = Request.Hobbies.Select(h => h.Id).ToList();
                var HobbiesToDelete = Context.Hobbies
                    .Where(h => !NewIDs.Contains(h.ID) && h.Member == Existing);
                Context.Hobbies.RemoveRange(HobbiesToDelete);

                // Add or Modify Hobbies
                foreach (var ReqHobby in Request.Hobbies)
                {
                    if (ReqHobby.Id == null)
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
                            .FirstOrDefault(h => h.ID == ReqHobby.Id) ??
                            throw new InvalidOperationException(
                                $"A hobby with ID {ReqHobby.Id} was not found.");
                        Hobby.Started = ReqHobby.Started;
                        Hobby.Ended = ReqHobby.Ended;
                        Hobby.Title = ReqHobby.Title;
                        Hobby.Organization = ReqHobby.Organization;
                        Hobby.Description = ReqHobby.Description;
                    }
                }

                // Remove Phone Numbers that do not exist
                NewIDs = Request.Phones.Select(p => p.Id).ToList();
                var PhonesToDelete = Context.Phones
                    .Where(p => !NewIDs.Contains(p.ID) && p.Member == Existing);
                Context.Phones.RemoveRange(PhonesToDelete);

                // Add or Modify Phones
                foreach (var ReqPhone in Request.Phones)
                {
                    if (ReqPhone.Id == null)
                    {
                        Context.Phones.Add(new MemberPhone
                        {
                            Created = DateTime.UtcNow,
                            Phone = ReqPhone.Phone_number,
                            Primary = ReqPhone.Primary,
                            Member = Existing
                        });
                    }
                    else
                    {
                        var Phone = Context.Phones
                            .FirstOrDefault(p => p.ID == ReqPhone.Id) ??
                            throw new InvalidOperationException(
                                $"An phone number with ID {ReqPhone.Id} was not found.");
                        Phone.Phone = ReqPhone.Phone_number;
                        Phone.Primary = ReqPhone.Primary;
                    }
                }

                // Remove Residences that do not exist
                NewIDs = Request.Residences.Select(r => r.Id).ToList();
                var ResToDelete = Context.Residences
                    .Where(r => !NewIDs.Contains(r.ID) && r.Member == Existing);
                Context.Residences.RemoveRange(ResToDelete);

                // Add or Modify Residences
                foreach (var ReqRes in Request.Residences)
                {
                    if (ReqRes.Id == null)
                    {
                        Context.Residences.Add(new Residence
                        {
                            Created = DateTime.UtcNow,
                            Address_Line_1 = ReqRes.Addr_line_1,
                            Address_Line_2 = ReqRes.Addr_line_2,
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
                            .FirstOrDefault(r => r.ID == ReqRes.Id) ??
                            throw new InvalidOperationException(
                                $"A residence with ID {ReqRes.Id} was not found.");
                        Residence.Address_Line_1 = ReqRes.Addr_line_1;
                        Residence.Address_Line_2 = ReqRes.Addr_line_2;
                        Residence.City = ReqRes.City;
                        Residence.State = ReqRes.State;
                        Residence.Country = ReqRes.Country;
                        Residence.Started = ReqRes.Started;
                        Residence.Ended = ReqRes.Ended;
                    }
                }

                // Remove Spousal Relationships that do not exist
                NewIDs = Request.Spouses.Select(s => s.Id).ToList();
                var SpousesToDelete = Context.Spouses
                    .Where(s => !NewIDs.Contains(s.ID)
                        && (s.Wife == Existing || s.Husband == Existing));
                Context.Spouses.RemoveRange(SpousesToDelete);

                // Add or Modify Spouses
                foreach (var ReqSpouse in Request.Spouses)
                {
                    var Husband = Members[ReqSpouse.Husband_id];
                    var Wife = Members[ReqSpouse.Wife_id];

                    if (ReqSpouse.Id == null)
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
                        var Spouse = Context.Spouses.FirstOrDefault(s =>
                            s.ID == ReqSpouse.Id) ??
                            throw new InvalidOperationException(
                                $"An spousal relationship with ID {ReqSpouse.Id} was not found.");
                        Spouse.Husband = Husband;
                        Spouse.Wife = Wife;
                        Spouse.Started = ReqSpouse.Started;
                        Spouse.Ended = ReqSpouse.Ended;
                    }
                }

                // Remove Work Experiences that do not exist
                NewIDs = Request.Work_history.Select(w => w.Id).ToList();
                var WorksToDelete = Context.Works
                    .Where(w => !NewIDs.Contains(w.ID) && w.Member == Existing);
                Context.Works.RemoveRange(WorksToDelete);

                // Add or Modify Work Experiences
                foreach (var ReqWork in Request.Work_history)
                {
                    if (ReqWork.Id == null)
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
                            .FirstOrDefault(w => w.ID == ReqWork.Id) ??
                            throw new InvalidOperationException(
                                $"A work history with ID {ReqWork.Id} was not found.");
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
                Console.WriteLine($"Issue with PUT /trees/{tree_id}/members/{member_id}: {a}");
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 400
                );
            }
            catch (AuthenticationException a)
            {
                Console.WriteLine($"Issue with PUT /trees/{tree_id}/members/{member_id}: {a}");
                return Results.Problem(
                    detail: a.Message,
                    statusCode: 401
                );
            }
            catch (Exception e) when (e is ArgumentNullException || e is InvalidOperationException)
            {
                Console.WriteLine($"Issue with PUT /trees/{tree_id}/members/{member_id}: {e}");
                return Results.Problem(
                    detail: e.Message,
                    statusCode: 404
                );
            }
            catch (Exception e)
            {
                Console.WriteLine($"Issue with PUT /trees/{tree_id}/members/{member_id}: {e}");
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