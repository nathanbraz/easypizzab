using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class Tenant : Entity
{
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? WhatsAppNumber { get; private set; }
    public string? ThemeColor { get; private set; }
    public string? LogoUrl { get; private set; }
    public string ConnectionString { get; private set; }
    public bool IsActive { get; private set; }

    public Tenant(string name, string slug, string connectionString)
    {
        Name = name;
        Slug = slug.ToLower();
        ConnectionString = connectionString;
        IsActive = true;
    }

    public void UpdateSettings(string themeColor, string logoUrl, string whatsAppNumber)
    {
        ThemeColor = themeColor;
        LogoUrl = logoUrl;
        WhatsAppNumber = whatsAppNumber;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }
}
