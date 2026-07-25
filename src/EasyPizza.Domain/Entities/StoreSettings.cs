using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class StoreSettings : Entity
{
    public bool IsStoreOpen { get; private set; }
    public decimal DeliveryFee { get; private set; }
    public decimal MinimumOrderAmount { get; private set; }
    public int EstimatedDeliveryTimeMin { get; private set; }
    public int EstimatedDeliveryTimeMax { get; private set; }
    public decimal? FreeDeliveryThreshold { get; private set; }
    public bool AcceptingPickup { get; private set; }
    public bool AcceptingDelivery { get; private set; }
    public string? MessageOfTheDay { get; private set; }
    public string? ActiveGlobalCouponCode { get; private set; }

    // Construtor vazio para o EF Core
    protected StoreSettings() { }

    public StoreSettings(
        bool isStoreOpen, 
        decimal deliveryFee, 
        decimal minimumOrderAmount, 
        int estimatedDeliveryTimeMin, 
        int estimatedDeliveryTimeMax, 
        decimal? freeDeliveryThreshold, 
        bool acceptingPickup, 
        bool acceptingDelivery, 
        string? messageOfTheDay,
        string? activeGlobalCouponCode = null)
    {
        IsStoreOpen = isStoreOpen;
        DeliveryFee = deliveryFee;
        MinimumOrderAmount = minimumOrderAmount;
        EstimatedDeliveryTimeMin = estimatedDeliveryTimeMin;
        EstimatedDeliveryTimeMax = estimatedDeliveryTimeMax;
        FreeDeliveryThreshold = freeDeliveryThreshold;
        AcceptingPickup = acceptingPickup;
        AcceptingDelivery = acceptingDelivery;
        MessageOfTheDay = messageOfTheDay;
        ActiveGlobalCouponCode = activeGlobalCouponCode;
    }

    public void Update(
        bool isStoreOpen, 
        decimal deliveryFee, 
        decimal minimumOrderAmount, 
        int estimatedDeliveryTimeMin, 
        int estimatedDeliveryTimeMax, 
        decimal? freeDeliveryThreshold, 
        bool acceptingPickup, 
        bool acceptingDelivery, 
        string? messageOfTheDay,
        string? activeGlobalCouponCode)
    {
        IsStoreOpen = isStoreOpen;
        DeliveryFee = deliveryFee;
        MinimumOrderAmount = minimumOrderAmount;
        EstimatedDeliveryTimeMin = estimatedDeliveryTimeMin;
        EstimatedDeliveryTimeMax = estimatedDeliveryTimeMax;
        FreeDeliveryThreshold = freeDeliveryThreshold;
        AcceptingPickup = acceptingPickup;
        AcceptingDelivery = acceptingDelivery;
        MessageOfTheDay = messageOfTheDay;
        ActiveGlobalCouponCode = activeGlobalCouponCode;
        SetUpdatedAt();
    }
}
