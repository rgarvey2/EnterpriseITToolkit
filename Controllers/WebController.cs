using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EnterpriseITToolkit.Services;

namespace EnterpriseITToolkit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebController : BaseApiController
    {
        public WebController(ILogger<WebController> logger, IAuditService auditService, IEnhancedAuthenticationService authService) 
            : base(logger, auditService, authService)
        {
        }

        [HttpGet]
        public IActionResult GetWebInterface()
        {
            return Redirect("/index.html");
        }

        [HttpGet("status")]
        public IActionResult GetWebStatus()
        {
            return Ok(new
            {
                status = "online",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                features = new[]
                {
                    "Dashboard",
                    "System Health",
                    "Network Tools",
                    "Security Audit",
                    "Performance Monitoring"
                }
            });
        }
    }
}
