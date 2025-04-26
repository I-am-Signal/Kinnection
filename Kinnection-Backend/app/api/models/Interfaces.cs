namespace Kinnection
{
    // Interfaces
    public interface IID
    {
        public int ID { get; set; }
    }

    public interface IOptionalID
    {
        public int? ID { get; set; }
    }

    public interface IPrimary
    {
        public bool Primary { get; set; }
    }

    public interface ITimeline
    {
        public DateOnly? Started { get; set; }
        public DateOnly? Ended { get; set; }
    }

    public interface IEvent
    {
        public string Title { get; set; }
        public string? Organization { get; set; }
        public string? Description { get; set; }
    }
}