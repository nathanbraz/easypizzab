namespace EasyPizza.Application.Interfaces.Services;

public interface IWhatsappBotService
{
    Task ProcessIncomingMessageAsync(string instanceName, string senderPhone, string messageText, string? senderName = null);
    Task<string> GenerateOrderLinkAsync(string phone, string? name = null);
}
