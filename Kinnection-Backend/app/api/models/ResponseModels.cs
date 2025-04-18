namespace Kinnection
{
    // Trees
    public class GetTreesResponse
    {
        public required int ID { get; set; }
        public required string Name { get; set; }
        public required int? Member_Self_ID { get; set; }
    }

    public class GetIndividualTreesResponse : GetTreesResponse
    {
        public required List<GetMembersResponse> Members { get; set; }
    }

    // Trees/Members
    public class GetMembersResponse : IID
    {
        public required int ID { get; set; }
        public required string Fname { get; set; }
        public required string? Mnames { get; set; }
        public required string Lname { get; set; }
        public required bool? Sex { get; set; }
        public required DateOnly? DOB { get; set; }
        public required DateOnly? DOD { get; set; }
        public required List<GetSpousesResponse> Spouses { get; set; }
        public required List<GetChildrenResponse> Children { get; set; }
    }
    public class GetIndividualMembersResponse : GetMembersResponse
    {
        public required string? Birthplace { get; set; }
        public required string? Deathplace { get; set; }
        public required string? Death_Cause { get; set; }
        public required string? Ethnicity { get; set; }
        public required string? Biography { get; set; }
        public required List<GetResidencesResponse> Residences { get; set; }
        public required List<GetEmailsResponse> Emails { get; set; }
        public required List<GetPhonesResponse> Phones { get; set; }
        public required List<GetWorkResponse> Work_History { get; set; }
        public required List<GetEducationResponse> Education_History { get; set; }
        public required List<GetHobbiesResponse> Hobbies { get; set; }
    }

    // Trees/Members/Children
    public class GetChildrenResponse : IID
    {
        public required int ID { get; set; }
        public required int Parent_ID { get; set; }
        public required int Child_ID { get; set; }
        public required DateOnly Adopted { get; set; }
    }

    public class GetAllChildrenResponse
    {
        public required List<GetChildrenResponse> Children { get; set; }
    }

    // Trees/Members/Education History
    public class GetEducationResponse : IID, ITimeline, IEvent
    {
        public required int ID { get; set; }
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

    public class PostEducationResponse : IID
    {
        public required int ID { get; set; }
    }

    // Trees/Members/Hobbies
    public class GetHobbiesResponse : IID, ITimeline, IEvent
    {
        public required int ID { get; set; }
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
    public class GetEmailsResponse : IID, IPrimary
    {
        public required int ID { get; set; }
        public required string Email { get; set; }
        public required bool Primary { get; set; }
    }

    public class GetAllEmailsResponse
    {
        public required List<GetEmailsResponse> emails { get; set; }
    }

    // Trees/Members/Phones
    public class GetPhonesResponse : IID, IPrimary
    {
        public required int ID { get; set; }
        public required string Phone_Number { get; set; }
        public required bool Primary { get; set; }
    }

    public class GetAllPhonesResponse
    {
        public required List<GetPhonesResponse> Phones { get; set; }
    }

    // Trees/Members/Residences
    public class GetResidencesResponse : IID
    {
        public required int ID { get; set; }
        public required string Addr_Line_1 { get; set; }
        public required string? Addr_Line_2 { get; set; }
        public required string City { get; set; }
        public required string? State { get; set; }
        public required string Country { get; set; }
    }

    public class GetAllResidencesResponse
    {
        public required List<GetResidencesResponse> Residences { get; set; }
    }

    // Trees/Members/Spouses
    public class GetSpousesResponse : IID, ITimeline
    {
        public required int ID { get; set; }
        public required int Husband_ID { get; set; }
        public required int Wife_ID { get; set; }
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
    }

    public class GetAllSpousesResponse
    {
        public required List<GetSpousesResponse> Spouses { get; set; }
    }

    // Trees/Members/Work History
    public class GetWorkResponse : IID, ITimeline, IEvent
    {
        public required int ID { get; set; }
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
        public required string Title { get; set; }
        public required string? Organization { get; set; }
        public required string? Description { get; set; }
    }

    public class GetAllWorkResponse
    {
        public required List<GetWorkResponse> Work_History { get; set; }
    }

    // Users
    public class GetUsersResponse : IID
    {
        public required int ID { get; set; }
        public required string Fname { get; set; }
        public required string Lname { get; set; }
        public required string Email { get; set; }
    }
}