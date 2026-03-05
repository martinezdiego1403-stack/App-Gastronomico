namespace SandwicheriaWalterio.Api.Services
{
    public interface ITenantService
    {
        string GetTenantId();
    }

    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetTenantId()
        {
            var tenantClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("TenantId");
            return tenantClaim?.Value ?? "default";
        }
    }
}
