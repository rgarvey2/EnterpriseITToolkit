using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EnterpriseITToolkit.Services;
using System.Security.Claims;

namespace EnterpriseITToolkit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected readonly ILogger _logger;
        protected readonly IAuditService _auditService;
        protected readonly IEnhancedAuthenticationService _authService;

        protected BaseApiController(
            ILogger logger,
            IAuditService auditService,
            IEnhancedAuthenticationService authService)
        {
            _logger = logger;
            _auditService = auditService;
            _authService = authService;
        }

        protected string? GetCurrentUsername()
        {
            return User?.Identity?.Name ?? User?.FindFirst(ClaimTypes.Name)?.Value;
        }

        protected async Task<bool> HasPermissionAsync(string permission)
        {
            var username = GetCurrentUsername();
            if (string.IsNullOrEmpty(username)) return false;

            return await _auditService.HasPermissionAsync(username, permission);
        }

        protected async Task LogAuditEventAsync(string action, string? resource = null, string? details = null, bool success = true)
        {
            var username = GetCurrentUsername();
            await _auditService.LogAuditEventAsync(action, username, resource, details, success);
        }

        protected IActionResult HandleException(Exception ex, string operation)
        {
            _logger.LogError(ex, "Error in {Operation}", operation);
            return StatusCode(500, new { error = "An internal server error occurred", operation });
        }
    }
}
