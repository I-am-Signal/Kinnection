namespace Kinnection
{
    // Auth
    public class LoginResponse : IID
    {
        public int ID { get; set; }
    }

    public class VerifyResponse : IID
    {
        public int ID { get; set; }
    }

    // Trees
    public class GetTreesResponse
    {
        public int User_ID { get; set; }
        public string Name { get; set; }
        public int Member_Self_ID { get; set; }
    }

    public class GetIndividualTreesResponse : GetTreesResponse
    {
        public List<GetMembersResponse> Members { get; set; }
    }

    public class PostTreesResponse : IID
    {
        public int ID { get; set; }
    }

    // Trees/Members
    public class GetMembersResponse : IID
    {
        public int ID { get; set; }
        public string Fname { get; set; }
        public string Mnames { get; set; }
        public string Lname { get; set; }
        public bool Sex { get; set; }
        public DateOnly DOB { get; set; }
        public DateOnly DOD { get; set; }
        public List<GetSpousesResponse> Spouses { get; set; }
        public List<GetChildrenResponse> Children { get; set; }
    }
    public class GetIndividualMembersResponse : GetMembersResponse
    {
        public string Birthplace { get; set; }
        public string Deathplace { get; set; }
        public string Death_Cause { get; set; }
        public string Ethnicity { get; set; }
        public string Biography { get; set; }
        public List<GetResidencesResponse> Residences { get; set; }
        public List<GetEmailsResponse> Emails { get; set; }
        public List<GetPhonesResponse> Phones { get; set; }
        public List<GetWorkResponse> Work_History { get; set; }
        public List<GetEducationResponse> Education_History { get; set; }
        public List<GetHobbiesResponse> Hobbies { get; set; }
    }

    public class PostMembersResponse : IID
    {
        public int ID { get; set; }
    }

    // Trees/Members/Children
    public class GetChildrenResponse : IID
    {
        public int ID { get; set; }
        public int Parent_ID { get; set; }
        public int Child_ID { get; set; }
        public DateOnly Adopted { get; set; }
    }

    public class GetAllChildrenResponse
    {
        public List<GetChildrenResponse> Children { get; set; }
    }

    public class PostChildrenResponse : IID
    {
        public int ID { get; set; }
    }

    // Trees/Members/Education History
    public class GetEducationResponse : IID, ITimeline, IEvent
    {
        public int ID { get; set; }
        public DateOnly Started { get; set; }
        public DateOnly Ended { get; set; }
        public string Title { get; set; }
        public string Organization { get; set; }
        public string Description { get; set; }
    }

    public class GetAllEducationResponse
    {
        public List<GetEducationResponse> Education_History { get; set; }
    }

    public class PostEducationResponse : IID
    {
        public int ID { get; set; }
    }

    // Trees/Members/Hobbies
    public class GetHobbiesResponse : IID, ITimeline, IEvent
    {
        public int ID { get; set; }
        public DateOnly Started { get; set; }
        public DateOnly Ended { get; set; }
        public string Title { get; set; }
        public string Organization { get; set; }
        public string Description { get; set; }
    }

    public class GetAllHobbiesResponse
    {
        public List<GetHobbiesResponse> Hobbies { get; set; }
    }

    public class PostHobbiesResponse : IID
    {
        public int ID { get; set; }
    }

    // Trees/Members/Emails
    public class GetEmailsResponse : IID, IPrimary
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public bool Primary { get; set; }
    }

    public class GetAllEmailsResponse
    {
        public List<GetEmailsResponse> emails { get; set; }
    }

    public class PostEmailsResponse : IID
    {
        public int ID { get; set; }
    }

    // Trees/Members/Phones
    public class GetPhonesResponse : IID, IPrimary
    {
        public int ID { get; set; }
        public string Phone_Number { get; set; }
        public bool Primary { get; set; }
    }

    public class GetAllPhonesResponse
    {
        public List<GetPhonesResponse> Phones { get; set; }
    }

    public class PostPhonesResponse : IID
    {
        public int ID { get; set; }
    }

    // Trees/Members/Residences
    public class GetResidencesResponse : IID
    {
        public int ID { get; set; }
        public string Addr_Line_1 { get; set; }
        public string Addr_Line_2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }

    public class GetAllResidencesResponse
    {
        public List<GetResidencesResponse> Residences { get; set; }
    }

    public class PostResidencesResponse : IID
    {
        public int ID { get; set; }
    }

    // Trees/Members/Spouses
    public class GetSpousesResponse : IID, ITimeline
    {
        public int ID { get; set; }
        public int Husband_ID { get; set; }
        public int Wife_ID { get; set; }
        public DateOnly Started { get; set; }
        public DateOnly Ended { get; set; }
    }

    public class GetAllSpousesResponse
    {
        public List<GetSpousesResponse> Spouses { get; set; }
    }

    public class PostSpousesResponse : IID
    {
        public int ID { get; set; }
    }

    // Trees/Members/Work History
    public class GetWorkResponse : IID, ITimeline, IEvent
    {
        public int ID { get; set; }
        public DateOnly Started { get; set; }
        public DateOnly Ended { get; set; }
        public string Title { get; set; }
        public string Organization { get; set; }
        public string Description { get; set; }
    }

    public class GetAllWorkResponse
    {
        public List<GetWorkResponse> Work_History { get; set; }
    }
    
    public class PostWorkResponse : IID
    {
        public int ID { get; set; }
    }

    // Users
    public class GetUsersResponse : IID
    {
        public int ID { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string Email { get; set; }
    }

    public class PostUsersResponse : IID
    {
        public int ID { get; set; }
    }
}