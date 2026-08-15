namespace EasyPizza.Application.Interfaces.Services;

public interface IWhatsappBotService
{
    Task ProcessIncomingMessageAsync(string instanceName, string senderPhone, string messageText, string? senderName = null);

    // Retorna null quando a loja (tenant) não pôde ser identificada a partir da requisição atual.
    Task<string?> GenerateOrderLinkAsync(string phone, string? name = null);
}
