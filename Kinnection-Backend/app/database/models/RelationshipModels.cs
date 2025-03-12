using System.ComponentModel.DataAnnotations.Schema;

namespace Kinnection
{
    public class Education : BaseMixin, TimelineMixin, EventMixin
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public required DateTime Created { get; set; } = DateTime.UtcNow;
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
        public required string Title { get; set; }
        public required string? Organization { get; set; }
        public required string? Description { get; set; }
        public required Member Member { get; set; }
    }

    public class Hobby : BaseMixin, TimelineMixin, EventMixin
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public required DateTime Created { get; set; } = DateTime.UtcNow;
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
        public required string Title { get; set; }
        public required string? Organization { get; set; }
        public required string? Description { get; set; }
        public required Member Member { get; set; }
    }

    // Figure out image storage and retrieval before adding this table
    // public class Image : BaseMixin, PrimaryMixin
    // {
    //     [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //     public int ID { get; set; }
    //     public required DateTime Created { get; set; }
    //     public required bool Primary { get; set; }
    //     public required Member Member { get; set; }
    //     public required string Name { get; set; }
    //     public required string Mime { get; set; }
    //     public required Blob Data { get; set; }
    // }

    public class MemberEmail : BaseMixin, PrimaryMixin
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public required DateTime Created { get; set; } = DateTime.UtcNow;
        public required bool Primary { get; set; }
        public required Member Member { get; set; }
        public required string Email { get; set; }
    }

    public class MemberPhone : BaseMixin, PrimaryMixin
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public required DateTime Created { get; set; } = DateTime.UtcNow;
        public required bool Primary { get; set; }
        public required Member Member { get; set; }
        public required string Phone { get; set; }
    }

    public class ParentalRelationship : BaseMixin
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public required DateTime Created { get; set; } = DateTime.UtcNow;
        public required Member Child { get; set; }
        public required Member Parent { get; set; }
        public required DateOnly Adopted { get; set; }
    }

    public class Residence : BaseMixin, TimelineMixin
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public required DateTime Created { get; set; } = DateTime.UtcNow;
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
        public required Member Member { get; set; }
        public required string Address_Line_1 { get; set; }
        public required string? Address_Line_2 { get; set; }
        public required string City { get; set; }
        public required string? State { get; set; }
        public required string Country { get; set; }
    }

    public class Spouse : BaseMixin, TimelineMixin
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public required DateTime Created { get; set; } = DateTime.UtcNow;
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
        public required Member Husband { get; set; }
        public required Member Wife { get; set; }
    }

    public class Work : BaseMixin, TimelineMixin, EventMixin
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public required DateTime Created { get; set; } = DateTime.UtcNow;
        public required DateOnly? Started { get; set; }
        public required DateOnly? Ended { get; set; }
        public required string Title { get; set; }
        public required string? Organization { get; set; }
        public required string? Description { get; set; }
        public required Member Member { get; set; }
    }
}