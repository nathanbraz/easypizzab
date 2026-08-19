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

    // Identidade visual do cardápio do cliente (topo da página)
    public string? LogoUrl { get; private set; }
    public string? BannerUrl { get; private set; }

    // Configurações do Bot do WhatsApp. WhatsappServerUrl não existe mais como campo por
    // loja — é infraestrutura compartilhada (mesmo Evolution API pra toda loja), fica em
    // config global (Whatsapp:ServerUrl). WhatsappInstanceName também não existe mais como
    // campo: por convenção é sempre igual ao Slug da loja, deduzido em tempo de uso (ver
    // EvolutionApiWhatsappSender) em vez de guardado/editável. WhatsappApiKey só é definido
    // pelo Master (endpoint próprio em TenantsController), nunca pelo lojista — é ele quem
    // cria a instância no Evolution API e recebe o token.
    public bool WhatsappBotEnabled { get; private set; }
    public string? WhatsappApiKey { get; private set; }
    public string? WhatsappSupportPhone { get; private set; }
    public string? WhatsappGreetingMessage { get; private set; }

    // Gateway de pagamento usado pra gerar cobranças online (Pix, cartão, etc.) desta loja.
    // Nomeado de forma genérica de propósito: hoje só existe integração com o Mercado Pago, mas o
    // campo já fica pronto pra outro gateway no futuro sem precisar renomear coluna/migration.
    // A credencial nunca deve ser exposta em texto puro por nenhum endpoint público — só o backend
    // a lê, pra chamar a API do gateway. Ver SettingsController para o tratamento de leitura/escrita.
    public string? PaymentGatewayProvider { get; private set; }
    public string? PaymentGatewayAccessToken { get; private set; }

    // O Mercado Pago não dá jeito confiável de saber, só pelo Access Token, se é uma
    // credencial de teste ou de produção (unificaram o prefixo APP_USR- em nov/2025 pros
    // dois). Isso importa porque sandbox e produção exigem e-mail sintético de pagador
    // DIFERENTE (sandbox exige @testuser.com; produção rejeita esse mesmo domínio com erro
    // "invalid_users_involved", confirmado testando de verdade) — sem essa flag explícita,
    // não tem como o backend saber qual formato usar. Default true (sandbox) porque é o
    // estado inicial de qualquer loja nova, antes de configurar credencial de produção.
    public bool PaymentGatewaySandboxMode { get; private set; } = true;

    // Segredo usado para validar a assinatura HMAC dos webhooks do gateway (garante que a
    // notificação realmente veio dele, não de alguém forjando uma chamada pro nosso endpoint).
    // É um segredo diferente do Access Token — gerado separadamente no painel do gateway.
    public string? PaymentGatewayWebhookSecret { get; private set; }

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
        string? activeGlobalCouponCode = null,
        bool whatsappBotEnabled = false,
        string? whatsappApiKey = null,
        string? whatsappSupportPhone = null,
        string? whatsappGreetingMessage = null,
        string? logoUrl = null,
        string? bannerUrl = null,
        string? paymentGatewayProvider = null,
        string? paymentGatewayAccessToken = null,
        string? paymentGatewayWebhookSecret = null,
        bool paymentGatewaySandboxMode = true)
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
        WhatsappBotEnabled = whatsappBotEnabled;
        WhatsappApiKey = whatsappApiKey;
        WhatsappSupportPhone = whatsappSupportPhone;
        WhatsappGreetingMessage = whatsappGreetingMessage;
        LogoUrl = logoUrl;
        BannerUrl = bannerUrl;
        PaymentGatewayProvider = paymentGatewayProvider;
        PaymentGatewayAccessToken = paymentGatewayAccessToken;
        PaymentGatewayWebhookSecret = paymentGatewayWebhookSecret;
        PaymentGatewaySandboxMode = paymentGatewaySandboxMode;
    }

    // Só o Master pode definir isso (ver TenantsController) — não passa pelo Update() geral
    // que o próprio lojista aciona via SettingsController.
    public void SetWhatsappApiKey(string? whatsappApiKey)
    {
        WhatsappApiKey = whatsappApiKey;
        SetUpdatedAt();
    }

    // Mesmo racional do WhatsappApiKey: só o Master decide se a credencial de pagamento colada
    // é de teste ou de produção (é ele quem sabe qual token pediu pro lojista, ou qual configurou
    // durante o onboarding) — o lojista nunca vê nem edita esse campo (ver SettingsController).
    public void SetPaymentGatewaySandboxMode(bool paymentGatewaySandboxMode)
    {
        PaymentGatewaySandboxMode = paymentGatewaySandboxMode;
        SetUpdatedAt();
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
        string? activeGlobalCouponCode,
        bool whatsappBotEnabled = false,
        string? whatsappSupportPhone = null,
        string? whatsappGreetingMessage = null,
        string? logoUrl = null,
        string? bannerUrl = null,
        string? paymentGatewayProvider = null,
        string? paymentGatewayAccessToken = null,
        string? paymentGatewayWebhookSecret = null)
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
        WhatsappBotEnabled = whatsappBotEnabled;
        WhatsappSupportPhone = whatsappSupportPhone;
        WhatsappGreetingMessage = whatsappGreetingMessage;
        LogoUrl = logoUrl;
        BannerUrl = bannerUrl;
        PaymentGatewayProvider = paymentGatewayProvider;
        PaymentGatewayAccessToken = paymentGatewayAccessToken;
        PaymentGatewayWebhookSecret = paymentGatewayWebhookSecret;
        SetUpdatedAt();
    }
}
