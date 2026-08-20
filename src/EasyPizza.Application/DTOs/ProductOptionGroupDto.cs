namespace EasyPizza.Application.DTOs;

// Formato unificado devolvido por GET /productoptions/{tenantSlug}/product/{id} — o mesmo shape
// que o frontend já consome hoje (vindo de ProductOptionGroup), agora podendo vir de duas fontes:
// grupos próprios do produto (Adicionais extras) e grupos compartilhados da categoria (Tamanho,
// Borda), sem o consumidor precisar saber a diferença.
public class ProductOptionGroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GroupType { get; set; } = "single";
    public bool IsRequired { get; set; }
    public int MinChoices { get; set; }
    public int MaxChoices { get; set; }
    public int DisplayOrder { get; set; }
    public List<ProductOptionItemDto> Options { get; set; } = new();

    // true = veio de CategoryOptionGroup (Tamanho, Borda — gerenciado na categoria, via
    // CategoryOptionsController). false = ProductOptionGroup próprio do produto (Adicionais
    // extras — gerenciado aqui mesmo, via ProductOptionsController). O admin usa isso pra saber
    // qual CRUD chamar; editar/excluir um grupo compartilhado pelas rotas de produto não funciona.
    public bool IsShared { get; set; }
}

public class ProductOptionItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal AdditionalPrice { get; set; }
    public int DisplayOrder { get; set; }
}

// Resposta de GET /menu/{tenantSlug} — mesmos campos que Product/ProductCategory sempre tiveram,
// só que OptionGroups agora vem combinado (grupos próprios + grupos compartilhados da categoria),
// igual ao GET /productoptions/{tenantSlug}/product/{id}.
public class CatalogCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool AllowsHalfAndHalf { get; set; }
    public List<CatalogProductDto> Products { get; set; } = new();
}

public class CatalogProductDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public bool IsAvailable { get; set; }
    public bool ShowInCrossSell { get; set; }
    public decimal? CrossSellDiscountPrice { get; set; }
    public List<ProductOptionGroupDto> OptionGroups { get; set; } = new();
}
