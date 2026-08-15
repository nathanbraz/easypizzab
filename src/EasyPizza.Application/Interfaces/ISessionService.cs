using EasyPizza.Application.DTOs;

namespace EasyPizza.Application.Interfaces;

public interface ISessionService
{
    Task<GenerateSessionResponse> GenerateMagicLinkSessionAsync(GenerateSessionRequest request);
    Task<SessionInfoResponse?> GetSessionInfoAsync(Guid token);
    Task MarkSessionAsUsedAsync(Guid sessionId);
}
