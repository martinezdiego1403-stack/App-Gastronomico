using SandwicheriaWalterio.Api.Data;

namespace SandwicheriaWalterio.Api.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // El TenantId se extrae del JWT en TenantService
            // Este middleware valida que exista para rutas protegidas
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var tenantId = context.User.FindFirst("TenantId")?.Value;
                if (string.IsNullOrEmpty(tenantId))
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new { error = "TenantId no encontrado en el token" });
                    return;
                }

                // SuperAdmin con TenantId "platform" tiene acceso total
                if (tenantId == "platform")
                {
                    await _next(context);
                    return;
                }

                // Verificar si el tenant está activo y no tiene trial expirado
                // Solo para rutas que NO son auth (para permitir login/registro)
                var path = context.Request.Path.Value?.ToLower() ?? "";
                if (!path.Contains("/api/auth/"))
                {
                    var db = context.RequestServices.GetRequiredService<ApiDbContext>();
                    var tenant = db.Tenants.FirstOrDefault(t => t.TenantId == tenantId);

                    if (tenant != null)
                    {
                        if (!tenant.Activo)
                        {
                            context.Response.StatusCode = 403;
                            await context.Response.WriteAsJsonAsync(new { error = "Tu cuenta ha sido desactivada. Contactá al soporte." });
                            return;
                        }

                        if (tenant.TrialExpirado)
                        {
                            context.Response.StatusCode = 402; // Payment Required
                            await context.Response.WriteAsJsonAsync(new
                            {
                                error = "Tu período de prueba ha expirado.",
                                trialExpirado = true,
                                plan = tenant.Plan
                            });
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
