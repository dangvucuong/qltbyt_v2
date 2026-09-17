namespace QLTBYT.API.Models.DTOs.Event;

public class EventKeywordDto
{
    public int Id { get; set; }
    public string Keyword { get; set; } = string.Empty;
}

public class CreateEventKeywordRequest
{
    public string Keyword { get; set; } = string.Empty;
}

