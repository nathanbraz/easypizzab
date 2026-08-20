using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

// Estratégia usada pra calcular o preço da combinação quando o cliente escolhe mais de um
// item do grupo de sabores (ex: pizza meio a meio) — configurável por loja em vez de fórmula
// fixa no código.
public enum FlavorPriceStrategy
{
    MaisCaro = 0,
    Soma = 1,
    Media = 2,
    MaisBarato = 3
}

// Grupo de opção compartilhado por toda a categoria (ex: "Tamanho", "Borda") — ao contrário de
// ProductOptionGroup (criado do zero por produto), existe uma única instância por categoria, e
// cada produto só referencia os itens que realmente oferece (ver ProductCategoryOptionPrice).
// Isso permite comparar opções entre produtos diferentes de forma confiável (ex: pizza meio a
// meio), sem depender de casar nomes de texto em tempo de execução.
public class CategoryOptionGroup : Entity
{
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; }
    public int MinChoices { get; private set; }
    public int MaxChoices { get; private set; }
    public int DisplayOrder { get; private set; }

    // Quando true, todo produto que oferecer um item deste grupo cobra o mesmo preço
    // (CategoryOptionItem.UniformPrice), definido uma única vez no item — sem precisar repetir o
    // valor produto por produto. Ideal pra Borda, onde o preço normalmente não varia por sabor.
    // Tamanho continua com HasUniformPricing=false, porque aí o preço genuinamente varia por
    // produto (duas pizzas podem custar valores diferentes no mesmo tamanho).
    public bool HasUniformPricing { get; private set; }

    // Marca o único grupo da categoria cujos itens representam outros Produtos da mesma
    // categoria (ex: "Sabores", pra pizza meio a meio) — generaliza o antigo SecondHalfProductId
    // pro mecanismo padrão de grupo compartilhado, sem caso especial no pedido.
    public bool IsFlavorGroup { get; private set; }
    public FlavorPriceStrategy FlavorPriceStrategy { get; private set; } = FlavorPriceStrategy.MaisCaro;

    public ProductCategory? Category { get; private set; }
    public ICollection<CategoryOptionItem> Items { get; private set; } = new List<CategoryOptionItem>();

    protected CategoryOptionGroup() { }

    public CategoryOptionGroup(Guid categoryId, string name, int minChoices, int maxChoices, int displayOrder = 0, bool hasUniformPricing = false, bool isFlavorGroup = false, FlavorPriceStrategy flavorPriceStrategy = FlavorPriceStrategy.MaisCaro)
    {
        CategoryId = categoryId;
        Name = name;
        MinChoices = minChoices;
        MaxChoices = maxChoices;
        DisplayOrder = displayOrder;
        HasUniformPricing = hasUniformPricing;
        IsFlavorGroup = isFlavorGroup;
        FlavorPriceStrategy = flavorPriceStrategy;
    }

    public void UpdateDetails(string name, int minChoices, int maxChoices, int displayOrder, bool hasUniformPricing, bool isFlavorGroup, FlavorPriceStrategy flavorPriceStrategy)
    {
        Name = name;
        MinChoices = minChoices;
        MaxChoices = maxChoices;
        DisplayOrder = displayOrder;
        HasUniformPricing = hasUniformPricing;
        IsFlavorGroup = isFlavorGroup;
        FlavorPriceStrategy = flavorPriceStrategy;
        SetUpdatedAt();
    }
}
