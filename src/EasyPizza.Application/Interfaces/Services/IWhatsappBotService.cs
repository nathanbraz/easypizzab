namespace EasyPizza.Application.Interfaces.Services;

public interface IWhatsappBotService
{
    Task ProcessIncomingMessageAsync(string instanceName, string senderPhone, string messageText);
    Task<string> GenerateOrderLinkAsync(string phone);
}
