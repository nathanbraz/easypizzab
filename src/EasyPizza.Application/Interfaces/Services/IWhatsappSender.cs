namespace EasyPizza.Application.Interfaces.Services;

public interface IWhatsappSender
{
    Task SendTextMessageAsync(string phone, string text);
}
