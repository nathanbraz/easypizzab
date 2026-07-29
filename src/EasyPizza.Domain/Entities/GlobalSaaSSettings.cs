using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class GlobalSaaSSettings : Entity
{
    public string? GlobalAnnouncementMessage { get; private set; }
    public bool IsAnnouncementActive { get; private set; }

    public GlobalSaaSSettings()
    {
    }

    public void UpdateAnnouncement(string? message, bool isActive)
    {
        GlobalAnnouncementMessage = message;
        IsAnnouncementActive = isActive;
        SetUpdatedAt();
    }
}
