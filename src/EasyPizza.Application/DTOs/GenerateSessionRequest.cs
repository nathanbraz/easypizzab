namespace EasyPizza.Application.DTOs;

public class GenerateSessionRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
}
