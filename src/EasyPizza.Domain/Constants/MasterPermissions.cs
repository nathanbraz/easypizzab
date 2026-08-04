namespace EasyPizza.Domain.Constants;

public static class MasterPermissions
{
    // Lojistas (Tenants)
    public const string ViewTenants = "Tenants:View";
    public const string ManageTenants = "Tenants:Manage";

    // Equipe Master (SaaS Owners)
    public const string ViewMasterTeam = "MasterTeam:View";
    public const string ManageMasterTeam = "MasterTeam:Manage";

    // Faturamento/Métricas do SaaS
    public const string ViewBilling = "Billing:View";
    public const string ManageBilling = "Billing:Manage";

    // Propriedade para facilitar o registro dinâmico no Program.cs e no Seed
    public static readonly IReadOnlyList<string> All = new List<string>
    {
        ViewTenants,
        ManageTenants,
        ViewMasterTeam,
        ManageMasterTeam,
        ViewBilling,
        ManageBilling
    };
}
