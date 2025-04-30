namespace Kinnection
{
    // Interfaces
    public interface IId
    {
        public int Id { get; set; }
    }

    public interface IOptionalId
    {
        public int? Id { get; set; }
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