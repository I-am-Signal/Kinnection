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
        public required string? Birthplace { get; set; }
        public required DateOnly? DOD { get; set; }
        public required string? Deathplace { get; set; }
        public required string? Death_Cause { get; set; }
        public required string? Ethnicity { get; set; }
        public required string? Biography { get; set; }
    }

    public class PutMembersRequest : PostMembersRequest
    {
        public required List<PutChildrenRequest> Children { get; set; }
        public required List<PutEducationRequest> Education_history { get; set; }
        public required List<PutEmailsRequest> Emails { get; set; }
        public required List<PutHobbiesRequest> Hobbies { get; set; }
        public required List<PutPhonesRequest> Phones { get; set; }
        public required List<PutResidencesRequest> Residences { get; set; }
        public required List<PutSpousesRequest> Spouses { get; set; }
        public required List<PutWorkRequest> Work_history { get; set; }
    }

    // Trees/Members/Children
    public class PutChildrenRequest : IOptionalID
    {
        public required int? ID { get; set; }
        public required int Parent_ID { get; set; }
        public required int Child_ID { get; set; }
        public required DateOnly Adopted { get; set; }
    }

    // Trees/Members/Education History
    public class PutEducationRequest : ITimeline, IEvent, IOptionalID
    {
        public required int? ID { get; set; }
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
        public required string Title { get; set; }
        public required string? Organization { get; set; }
        public required string? Description { get; set; }
    }

    // Trees/Members/Hobbies
    public class PutHobbiesRequest : ITimeline, IEvent, IOptionalID
    {
        public required int? ID { get; set; }
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
        public required string Title { get; set; }
        public required string? Organization { get; set; }
        public required string? Description { get; set; }
    }

    // Trees/Members/Emails
    public class PutEmailsRequest : IPrimary, IOptionalID
    {
        public required int? ID { get; set; }
        public required string Email { get; set; }
        public bool Primary { get; set; }
    }

    // Trees/Members/Phones
    public class PutPhonesRequest : IPrimary, IOptionalID
    {
        public required int? ID { get; set; }
        public required string Phone_Number { get; set; }
        public required bool Primary { get; set; }
    }

    // Trees/Members/Residences
    public class PutResidencesRequest : IOptionalID, ITimeline
    {
        public required int? ID { get; set; }
        public required string Addr_Line_1 { get; set; }
        public required string Addr_Line_2 { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
        public required string Country { get; set; }
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
    }

    // Trees/Members/Spouses
    public class PutSpousesRequest : ITimeline, IOptionalID
    {
        public required int? ID { get; set; }
        public required int Husband_ID { get; set; }
        public required int Wife_ID { get; set; }
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
    }

    // Trees/Members/Work History
    public class PutWorkRequest : ITimeline, IEvent, IOptionalID
    {
        public required int? ID { get; set; }
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
        public required string Title { get; set; }
        public required string? Organization { get; set; }
        public required string? Description { get; set; }
    }

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