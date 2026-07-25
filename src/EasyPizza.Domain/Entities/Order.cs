using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public enum OrderStatus
{
    New = 1,
    Preparing = 2,
    Delivering = 3,
    Completed = 4,
    Canceled = 5
}

public enum OrderType
{
    Delivery = 1,
    Pickup = 2
}

public class Order : Entity
{
    public Guid CustomerId { get; private set; }
    
    // Address where the order will be delivered (null if it's pickup)
    public Guid? CustomerAddressId { get; private set; }
    
    // Delivery or Pickup
    public OrderType Type { get; private set; }
    
    // Payment method chosen
    public Guid PaymentTypeId { get; private set; }

    public OrderStatus Status { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal DeliveryFee { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount { get; private set; }

    // Coupon applied
    public Guid? CouponId { get; private set; }
    public string? CouponCode { get; private set; }
    
    // Pix generated code payload
    public string? PixCopyPasteCode { get; private set; }
    
    // For external gateway if integrated in the future
    public string? PaymentExternalId { get; private set; }
    public bool IsPaid { get; private set; }

    public Customer? Customer { get; private set; }
    public CustomerAddress? Address { get; private set; }
    public PaymentType? PaymentType { get; private set; }
    public Coupon? Coupon { get; private set; }
    
    public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();

    protected Order() { }

    public Order(Guid customerId, Guid? customerAddressId, OrderType type, Guid paymentTypeId, decimal subTotal, decimal deliveryFee, decimal discountAmount = 0, Guid? couponId = null, string? couponCode = null)
    {
        CustomerId = customerId;
        CustomerAddressId = customerAddressId;
        Type = type;
        PaymentTypeId = paymentTypeId;
        SubTotal = subTotal;
        DeliveryFee = type == OrderType.Pickup ? 0 : deliveryFee;
        DiscountAmount = discountAmount;
        CouponId = couponId;
        CouponCode = couponCode;
        TotalAmount = (subTotal + deliveryFee) - discountAmount;
        if (TotalAmount < 0) TotalAmount = 0;
        
        Status = OrderStatus.New;
        IsPaid = false;
    }

    public void SetPixCode(string copyPasteCode)
    {
        PixCopyPasteCode = copyPasteCode;
        SetUpdatedAt();
    }

    public void MarkAsPaid(string? externalId = null)
    {
        IsPaid = true;
        if (externalId != null) PaymentExternalId = externalId;
        
        Status = OrderStatus.Preparing; // Automatically move to kitchen
        SetUpdatedAt();
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
        SetUpdatedAt();
    }
}
