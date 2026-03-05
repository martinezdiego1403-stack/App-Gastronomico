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
            // Este middleware solo valida que exista para rutas protegidas
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var tenantId = context.User.FindFirst("TenantId")?.Value;
                if (string.IsNullOrEmpty(tenantId))
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new { error = "TenantId no encontrado en el token" });
                    return;
                }
            }

            await _next(context);
        }
    }
}
