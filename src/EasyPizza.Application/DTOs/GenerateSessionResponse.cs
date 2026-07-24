namespace EasyPizza.Application.DTOs;

public class GenerateSessionResponse
{
    public Guid SessionId { get; set; }
    public string MagicLink { get; set; } = string.Empty;
}
