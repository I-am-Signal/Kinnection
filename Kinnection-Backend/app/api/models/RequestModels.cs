namespace Kinnection
{
    // Auth
    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    // Trees
    public class PostTreesRequest
    {
        public required string Name { get; set; }
    }

    public class PutTreesRequest : PostTreesRequest
    {
        public required int? Member_Self_ID { get; set; }
    }

    // Trees/Members
    public class PostMembersRequest
    {
        public required string Fname { get; set; }
        public required string? Mnames { get; set; }
        public required string Lname { get; set; }
        public required bool Sex { get; set; }
        public required DateOnly? DOB { get; set; }
        public required string Birthplace { get; set; }
        public required DateOnly? DOD { get; set; }
        public required string Deathplace { get; set; }
        public required string Death_Cause { get; set; }
        public required string Ethnicity { get; set; }
        public required string Biography { get; set; }
    }

    public class PutMembersRequest : PostMembersRequest { }

    // Trees/Members/Children
    public class PostChildrenRequest
    {
        public required int Parent_ID { get; set; }
        public required int Child_ID { get; set; }
        public required DateOnly Adopted { get; set; }
    }

    public class PutChildrenRequest : PostChildrenRequest { }

    // Trees/Members/Education History
    public class PostEducationRequest : ITimeline, IEvent
    {
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
        public required string Title { get; set; }
        public required string? Organization { get; set; }
        public required string? Description { get; set; }
    }

    public class PutEducationRequest : PostEducationRequest { }

    // Trees/Members/Hobbies
    public class PostHobbiesRequest : ITimeline, IEvent
    {
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
        public required string Title { get; set; }
        public required string? Organization { get; set; }
        public required string? Description { get; set; }
    }

    public class PutHobbiesRequest : PostHobbiesRequest { }

    // Trees/Members/Emails
    public class PostEmailsRequest : IPrimary
    {
        public required string Email { get; set; }
        public bool Primary { get; set; }
    }

    public class PutEmailsRequest : PostEmailsRequest { }

    // Trees/Members/Phones
    public class PostPhonesRequest : IPrimary
    {
        public required string Phone_Number { get; set; }
        public required bool Primary { get; set; }
    }

    public class PutPhonesRequest : PostPhonesRequest { }

    // Trees/Members/Residences
    public class PostResidencesRequest
    {
        public required string Addr_Line_1 { get; set; }
        public required string Addr_Line_2 { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
        public required string Country { get; set; }
    }

    public class PutResidencesRequest : PostResidencesRequest { }

    // Trees/Members/Spouses
    public class PostSpousesRequest : ITimeline
    {
        public required int Husband_ID { get; set; }
        public required int Wife_ID { get; set; }
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
    }

    public class PutSpousesRequest : PostSpousesRequest { }

    // Trees/Members/Work History
    public class PostWorkRequest : ITimeline, IEvent
    {
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
        public required string Title { get; set; }
        public required string? Organization { get; set; }
        public required string? Description { get; set; }
    }

    public class PutWorkRequest : PostWorkRequest { }

    // Users
    public class PostUsersRequest : PutUsersRequest
    {
        public required string Password { get; set; }
    }

    public class PutUsersRequest
    {
        public required string Fname { get; set; }
        public required string Lname { get; set; }
        public required string Email { get; set; }
    }
}