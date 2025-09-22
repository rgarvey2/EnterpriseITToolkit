using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace EnterpriseITToolkit.Security
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        
        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            // Remove server header
            context.Response.Headers.Remove("Server");
            
            // Security headers
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Add("X-Frame-Options", "DENY");
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
            context.Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=(), payment=(), usb=()");
            
            // Content Security Policy
            var csp = "default-src 'self'; " +
                     "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
                     "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://fonts.googleapis.com; " +
                     "font-src 'self' https://fonts.gstatic.com; " +
                     "img-src 'self' data: https:; " +
                     "connect-src 'self' http://localhost:5001; " +
                     "frame-ancestors 'none'; " +
                     "base-uri 'self'; " +
                     "form-action 'self'";
            
            context.Response.Headers.Add("Content-Security-Policy", csp);
            
            // HSTS (only for HTTPS)
            if (context.Request.IsHttps)
            {
                context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
            }
            
            await _next(context);
        }
    }
}
