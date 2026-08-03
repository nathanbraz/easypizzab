namespace EasyPizza.Domain.Constants;

public static class Permissions
{
    // Pedidos (Orders)
    public const string ViewOrders = "Orders:View";
    public const string EditOrders = "Orders:Edit";

    // Cardápio (Catalog)
    public const string ManageCatalog = "Catalog:Manage";

    // Configurações e Financeiro (Settings)
    public const string ManageSettings = "Settings:Manage";
    public const string ManageCoupons = "Coupons:Manage";
    public const string ManageCouriers = "Couriers:Manage";

    // Equipe (Team)
    public const string ManageTeam = "Team:Manage";

    // Clientes (Customers)
    public const string ViewCustomers = "Customers:View";

    // Propriedade para facilitar o registro dinâmico no Program.cs e no Seed
    public static readonly IReadOnlyList<string> All = new List<string>
    {
        ViewOrders,
        EditOrders,
        ManageCatalog,
        ManageSettings,
        ManageCoupons,
        ManageCouriers,
        ManageTeam,
        ViewCustomers
    };
}
