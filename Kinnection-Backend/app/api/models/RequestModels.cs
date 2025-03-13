namespace Kinnection
{
    // Auth
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // Trees
    public class PostTreesRequest
    {
        public int User_ID { get; set; }
        public string Name { get; set; }
    }

    public class PutTreesRequest : PostTreesRequest
    {
        public int Member_Self_ID { get; set; }
    }

    // Trees/Members
    public class PostMembersRequest
    {
        public int Tree_ID { get; set; }
        public string Fname { get; set; }
        public string Mnames { get; set; }
        public string Lname { get; set; }
        public bool Sex { get; set; }
        public DateOnly DOB { get; set; }
        public string Birthplace { get; set; }
        public DateOnly DOD { get; set; }
        public string Deathplace { get; set; }
        public string Death_Cause { get; set; }
        public string Ethnicity { get; set; }
        public string Biography { get; set; }
    }

    public class PutMembersRequest : PostMembersRequest { }

    // Trees/Members/Children
    public class PostChildrenRequest
    {
        public int Parent_ID { get; set; }
        public int Child_ID { get; set; }
        public DateOnly Adopted { get; set; }
    }

    public class PutChildrenRequest : PostChildrenRequest { }

    // Trees/Members/Education History
    public class PostEducationRequest : ITimeline, IEvent
    {
        public DateOnly Started { get; set; }
        public DateOnly Ended { get; set; }
        public string Title { get; set; }
        public string Organization { get; set; }
        public string Description { get; set; }
    }

    public class PutEducationRequest : PostEducationRequest { }

    // Trees/Members/Hobbies
    public class PostHobbiesRequest : ITimeline, IEvent
    {
        public DateOnly Started { get; set; }
        public DateOnly Ended { get; set; }
        public string Title { get; set; }
        public string Organization { get; set; }
        public string Description { get; set; }
    }

    public class PutHobbiesRequest : PostHobbiesRequest { }

    // Trees/Members/Emails
    public class PostEmailsRequest : IPrimary
    {
        public string Email { get; set; }
        public bool Primary { get; set; }
    }

    public class PutEmailsRequest : PostEmailsRequest { }

    // Trees/Members/Phones
    public class PostPhonesRequest : IPrimary
    {
        public string Phone_Number { get; set; }
        public bool Primary { get; set; }
    }

    public class PutPhonesRequest : PostPhonesRequest { }

    // Trees/Members/Residences
    public class PostResidencesRequest
    {
        public string Addr_Line_1 { get; set; }
        public string Addr_Line_2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }

    public class PutResidencesRequest : PostResidencesRequest { }

    // Trees/Members/Spouses
    public class PostSpousesRequest : ITimeline
    {
        public int Husband_ID { get; set; }
        public int Wife_ID { get; set; }
        public DateOnly Started { get; set; }
        public DateOnly Ended { get; set; }
    }

    public class PutSpousesRequest : PostSpousesRequest { }

    // Trees/Members/Work History
    public class PostWorkRequest : ITimeline, IEvent
    {
        public DateOnly Started { get; set; }
        public DateOnly Ended { get; set; }
        public string Title { get; set; }
        public string Organization { get; set; }
        public string Description { get; set; }
    }

    public class PutWorkRequest : PostWorkRequest { }

    // Users
    public class PostUsersRequest : PutUsersRequest
    {
        public string Password { get; set; }
    }

    public class PutUsersRequest
    {
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string Email { get; set; }
    }
}