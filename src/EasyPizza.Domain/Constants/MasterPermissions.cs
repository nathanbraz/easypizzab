namespace EasyPizza.Domain.Constants;

public static class MasterPermissions
{
    // Lojistas (Tenants)
    public const string ViewTenants = "Tenants:View";
    public const string CreateTenants = "Tenants:Create";
    public const string EditTenants = "Tenants:Edit";
    public const string BlockTenants = "Tenants:Block";

    // Equipe Master (SaaS Owners)
    public const string ViewMasterTeam = "MasterTeam:View";
    public const string CreateMasterTeam = "MasterTeam:Create";
    public const string EditMasterTeam = "MasterTeam:Edit";
    public const string BlockMasterTeam = "MasterTeam:Block";
    public const string DeleteMasterTeam = "MasterTeam:Delete";

    // Cargos (Roles)
    public const string ViewMasterRoles = "MasterRoles:View";
    public const string CreateMasterRoles = "MasterRoles:Create";
    public const string EditMasterRoles = "MasterRoles:Edit";
    public const string DeleteMasterRoles = "MasterRoles:Delete";

    // Faturamento/Métricas do SaaS
    public const string ViewBilling = "Billing:View";
    public const string ManageBilling = "Billing:Manage";

    // Propriedade para facilitar o registro dinâmico no Program.cs e no Seed
    public static readonly IReadOnlyList<string> All = new List<string>
    {
        ViewTenants,
        CreateTenants,
        EditTenants,
        BlockTenants,
        ViewMasterTeam,
        CreateMasterTeam,
        EditMasterTeam,
        BlockMasterTeam,
        DeleteMasterTeam,
        ViewMasterRoles,
        CreateMasterRoles,
        EditMasterRoles,
        DeleteMasterRoles,
        ViewBilling,
        ManageBilling
    };
}
