namespace SandwicheriaWalterio.Models
{
    /// <summary>
    /// Interfaz para entidades multi-tenant
    /// </summary>
    public interface ITenantEntity
    {
        string TenantId { get; set; }
    }
}
