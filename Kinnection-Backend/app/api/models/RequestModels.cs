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
        public required int? Member_self_id { get; set; }
    }

    // Trees/Members
    public class PostMembersRequest
    {
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
    public class PutChildrenRequest : IOptionalId
    {
        public required int? Id { get; set; }
        public required int Parent_id { get; set; }
        public required int Child_id { get; set; }
        public required DateOnly? Adopted { get; set; }
    }

    // Trees/Members/Education History
    public class PutEducationRequest : ITimeline, IEvent, IOptionalId
    {
        public required int? Id { get; set; }
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
        public required string Title { get; set; }
        public required string? Organization { get; set; }
        public required string? Description { get; set; }
    }

    // Trees/Members/Emails
    public class PutEmailsRequest : IPrimary, IOptionalId
    {
        public required int? Id { get; set; }
        public required string Email { get; set; }
        public bool Primary { get; set; }
    }

    // Trees/Members/Hobbies
    public class PutHobbiesRequest : ITimeline, IEvent, IOptionalId
    {
        public required int? Id { get; set; }
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
        public required string Title { get; set; }
        public required string? Organization { get; set; }
        public required string? Description { get; set; }
    }

    // Trees/Members/Phones
    public class PutPhonesRequest : IPrimary, IOptionalId
    {
        public required int? Id { get; set; }
        public required string Phone_number { get; set; }
        public required bool Primary { get; set; }
    }

    // Trees/Members/Residences
    public class PutResidencesRequest : IOptionalId, ITimeline
    {
        public required int? Id { get; set; }
        public required string Addr_line_1 { get; set; }
        public required string? Addr_line_2 { get; set; }
        public required string City { get; set; }
        public required string? State { get; set; }
        public required string Country { get; set; }
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
    }

    // Trees/Members/Spouses
    public class PutSpousesRequest : ITimeline, IOptionalId
    {
        public required int? Id { get; set; }
        public required int Husband_id { get; set; }
        public required int Wife_id { get; set; }
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
    }

    // Trees/Members/Work History
    public class PutWorkRequest : ITimeline, IEvent, IOptionalId
    {
        public required int? Id { get; set; }
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