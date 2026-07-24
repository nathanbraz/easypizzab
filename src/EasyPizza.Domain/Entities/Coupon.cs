using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class Coupon : Entity
{
    public string Code { get; private set; }
    public decimal? DiscountPercentage { get; private set; }
    public decimal? DiscountFixedAmount { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; }
    public int UsageLimit { get; private set; }
    public int TimesUsed { get; private set; }

    public Coupon(string code, decimal? discountPercentage, decimal? discountFixedAmount, DateTime? expiresAt, int usageLimit = 0)
    {
        Code = code.ToUpperInvariant();
        DiscountPercentage = discountPercentage;
        DiscountFixedAmount = discountFixedAmount;
        ExpiresAt = expiresAt;
        IsActive = true;
        UsageLimit = usageLimit;
        TimesUsed = 0;
    }

    public void UpdateDetails(decimal? discountPercentage, decimal? discountFixedAmount, DateTime? expiresAt, int usageLimit, bool isActive)
    {
        DiscountPercentage = discountPercentage;
        DiscountFixedAmount = discountFixedAmount;
        ExpiresAt = expiresAt;
        UsageLimit = usageLimit;
        IsActive = isActive;
        SetUpdatedAt();
    }

    public void RegisterUsage()
    {
        TimesUsed++;
        SetUpdatedAt();
    }

    public bool IsValid()
    {
        if (!IsActive) return false;
        if (ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow) return false;
        if (UsageLimit > 0 && TimesUsed >= UsageLimit) return false;
        
        return true;
    }
}
