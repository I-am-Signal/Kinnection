namespace Kinnection
{
    // Trees
    public class GetTreesResponse
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required int? Member_self_id { get; set; }
    }

    public class GetAllTreesResponse
    {
        public required List<GetTreesResponse> Trees { get; set; }
    }

    public class GetIndividualTreesResponse : GetTreesResponse
    {
        public required List<GetTreesMembersResponse> Members { get; set; }
    }

    
    public class GetTreesMembersResponse : IId
    {
        public required int Id { get; set; }
        public required string Fname { get; set; }
        public required string? Mnames { get; set; }
        public required string Lname { get; set; }
        public required bool Sex { get; set; }
        public required DateOnly? Dob { get; set; }
        public required DateOnly? Dod { get; set; }
        public required List<GetSpousesResponse> Spouses { get; set; }
        public required List<GetChildrenResponse> Children { get; set; }
    }

    // Trees/Members
    public class GetMembersResponse : IId
    {
        public required int Id { get; set; }
        public required string Fname { get; set; }
        public required string? Mnames { get; set; }
        public required string Lname { get; set; }
        public required bool Sex { get; set; }
        public required DateOnly? Dob { get; set; }
        public required string? Birthplace { get; set; }
        public required DateOnly? Dod { get; set; }
        public required string? Deathplace { get; set; }
        public required string? Death_cause { get; set; }
        public required string? Ethnicity { get; set; }
        public required string? Biography { get; set; }
    }
    public class GetIndividualMembersResponse : GetMembersResponse
    {
        public required List<GetChildrenResponse> Children { get; set; }
        public required List<GetEducationResponse> Education_history { get; set; }
        public required List<GetEmailsResponse> Emails { get; set; }
        public required List<GetHobbiesResponse> Hobbies { get; set; }
        public required List<GetPhonesResponse> Phones { get; set; }
        public required List<GetResidencesResponse> Residences { get; set; }
        public required List<GetSpousesResponse> Spouses { get; set; }
        public required List<GetWorkResponse> Work_history { get; set; }
    }

    // Trees/Members/Children
    public class GetChildrenResponse : IId
    {
        public required int Id { get; set; }
        public required int Parent_id { get; set; }
        public required int Child_id { get; set; }
        public required DateOnly? Adopted { get; set; }
    }

    public class GetAllChildrenResponse
    {
        public required List<GetChildrenResponse> Children { get; set; }
    }

    // Trees/Members/Education History
    public class GetEducationResponse : IId, ITimeline, IEvent
    {
        public required int Id { get; set; }
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
        public required string Title { get; set; }
        public required string? Organization { get; set; }
        public required string? Description { get; set; }
    }

    public class GetAllEducationResponse
    {
        public required List<GetEducationResponse> Education_History { get; set; }
    }

    public class PostEducationResponse : IId
    {
        public required int Id { get; set; }
    }

    // Trees/Members/Hobbies
    public class GetHobbiesResponse : IId, ITimeline, IEvent
    {
        public required int Id { get; set; }
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
        public required string Title { get; set; }
        public required string? Organization { get; set; }
        public required string? Description { get; set; }
    }

    public class GetAllHobbiesResponse
    {
        public required List<GetHobbiesResponse> Hobbies { get; set; }
    }

    // Trees/Members/Emails
    public class GetEmailsResponse : IId, IPrimary
    {
        public required int Id { get; set; }
        public required string Email { get; set; }
        public required bool Primary { get; set; }
    }

    public class GetAllEmailsResponse
    {
        public required List<GetEmailsResponse> emails { get; set; }
    }

    // Trees/Members/Phones
    public class GetPhonesResponse : IId, IPrimary
    {
        public required int Id { get; set; }
        public required string Phone_number { get; set; }
        public required bool Primary { get; set; }
    }

    public class GetAllPhonesResponse
    {
        public required List<GetPhonesResponse> Phones { get; set; }
    }

    // Trees/Members/Residences
    public class GetResidencesResponse : IId
    {
        public required int Id { get; set; }
        public required string Addr_line_1 { get; set; }
        public required string? Addr_line_2 { get; set; }
        public required string City { get; set; }
        public required string? State { get; set; }
        public required string Country { get; set; }
    }

    public class GetAllResidencesResponse
    {
        public required List<GetResidencesResponse> Residences { get; set; }
    }

    // Trees/Members/Spouses
    public class GetSpousesResponse : IId, ITimeline
    {
        public required int Id { get; set; }
        public required int Husband_id { get; set; }
        public required int Wife_id { get; set; }
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
    }

    public class GetAllSpousesResponse
    {
        public required List<GetSpousesResponse> Spouses { get; set; }
    }

    // Trees/Members/Work History
    public class GetWorkResponse : IId, ITimeline, IEvent
    {
        public required int Id { get; set; }
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
        public required string Title { get; set; }
        public required string? Organization { get; set; }
        public required string? Description { get; set; }
    }

    public class GetAllWorkResponse
    {
        public required List<GetWorkResponse> Work_history { get; set; }
    }

    // Users
    public class GetUsersResponse : IId
    {
        public required int Id { get; set; }
        public required string Fname { get; set; }
        public required string Lname { get; set; }
        public required string Email { get; set; }
    }
}