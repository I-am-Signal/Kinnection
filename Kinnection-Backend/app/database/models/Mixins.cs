public interface BaseMixin
{
    public int ID { get; set; }
    public DateTime Created { get; set; }
}

public interface ModifyMixin
{
    public DateTime? Modified { get; set; }
}

public interface PrimaryMixin
{
    public bool Primary { get; set; }
}

public interface TimelineMixin
{
    public DateOnly? Started { get; set; }
    public DateOnly? Ended { get; set; }
}

public interface EventMixin
{
    public string Title { get; set; }
    public string? Organization { get; set; }
    public string? Description { get; set; }
}