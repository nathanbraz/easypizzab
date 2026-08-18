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

public class Order : IntEntity
{
    public Guid CustomerId { get; private set; }
    
    // Endereço onde o pedido será entregue (nulo se for retirada)
    public Guid? CustomerAddressId { get; private set; }
    
    // Entrega ou Retirada
    public OrderType Type { get; private set; }
    
    // Método de pagamento escolhido
    public Guid PaymentTypeId { get; private set; }

    public OrderStatus Status { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal DeliveryFee { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount { get; private set; }

    // Cupom aplicado
    public Guid? CouponId { get; private set; }
    public string? CouponCode { get; private set; }
    
    // Payload do código Pix gerado
    public string? PixCopyPasteCode { get; private set; }
    
    public string? PaymentExternalId { get; private set; }
    public bool IsPaid { get; private set; }
    
    // Troco para (quando pagamento for dinheiro)
    public decimal? ChangeFor { get; private set; }

    // Só preenchido quando Status == Canceled (ver Cancel) — obrigatório cancelar com motivo,
    // pra dar contexto pro cliente e ficar registrado por que aquele pedido não seguiu.
    public string? CancellationReason { get; private set; }

    public Customer? Customer { get; private set; }
    public CustomerAddress? Address { get; private set; }
    public PaymentType? PaymentType { get; private set; }
    public Coupon? Coupon { get; private set; }
    
    public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();

    protected Order() { }

    public Order(Guid customerId, Guid? customerAddressId, OrderType type, Guid paymentTypeId, decimal subTotal, decimal deliveryFee, decimal discountAmount = 0, Guid? couponId = null, string? couponCode = null, decimal? changeFor = null)
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
        ChangeFor = changeFor;
    }

    public void SetPixCode(string copyPasteCode, string gatewayOrderId)
    {
        PixCopyPasteCode = copyPasteCode;
        PaymentExternalId = gatewayOrderId;
        SetUpdatedAt();
    }

    public void MarkAsPaid(string? externalId = null)
    {
        IsPaid = true;
        if (externalId != null) PaymentExternalId = externalId;
        
        Status = OrderStatus.Preparing; // Mover automaticamente para a cozinha
        SetUpdatedAt();
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
        SetUpdatedAt();
    }

    public void Cancel(string reason)
    {
        Status = OrderStatus.Canceled;
        CancellationReason = reason;
        SetUpdatedAt();
    }
}
