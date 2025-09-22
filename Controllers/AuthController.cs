using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EnterpriseITToolkit.Services;
using EnterpriseITToolkit.Models;

namespace EnterpriseITToolkit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseApiController
    {
        public AuthController(
            ILogger<AuthController> logger,
            IAuditService auditService,
            IEnhancedAuthenticationService authService)
            : base(logger, auditService, authService)
        {
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { error = "Username and password are required" });
                }

                var result = await _authService.AuthenticateAsync(request.Username, request.Password, request.MfaCode);
                
                if (result.Success)
                {
                    await LogAuditEventAsync("LOGIN", "Authentication", "User logged in successfully", true);
                    return Ok(new
                    {
                        success = true,
                        sessionToken = result.SessionToken,
                        username = result.Username,
                        roles = result.Roles,
                        expiresAt = result.ExpiresAt
                    });
                }
                else
                {
                    await LogAuditEventAsync("LOGIN_FAILED", "Authentication", result.Error, false);
                    return Unauthorized(new { error = result.Error, requiresMfa = result.RequiresMfa });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Login");
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.SessionToken))
                {
                    return BadRequest(new { error = "Session token is required" });
                }

                await _authService.LogoutAsync(request.SessionToken);
                await LogAuditEventAsync("LOGOUT", "Authentication", "User logged out", true);
                
                return Ok(new { success = true, message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Logout");
            }
        }

        [HttpPost("validate-session")]
        public async Task<IActionResult> ValidateSession([FromBody] ValidateSessionRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.SessionToken))
                {
                    return BadRequest(new { error = "Session token is required" });
                }

                var isValid = await _authService.ValidateSessionAsync(request.SessionToken);
                
                if (isValid)
                {
                    var sessionInfo = await _authService.GetSessionInfoAsync(request.SessionToken);
                    return Ok(new
                    {
                        valid = true,
                        username = sessionInfo.Username,
                        roles = sessionInfo.Roles,
                        expiresAt = sessionInfo.ExpiresAt,
                        lastActivity = sessionInfo.LastActivity
                    });
                }
                else
                {
                    return Unauthorized(new { valid = false, error = "Invalid or expired session" });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, "ValidateSession");
            }
        }

        [HttpPost("enable-mfa")]
        public async Task<IActionResult> EnableMfa([FromBody] MfaRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username))
                {
                    return BadRequest(new { error = "Username is required" });
                }

                var success = await _authService.EnableMfaAsync(request.Username);
                
                if (success)
                {
                    var secret = await _authService.GenerateMfaSecretAsync(request.Username);
                    var qrCodeUrl = await _authService.GenerateQrCodeAsync(request.Username, secret);
                    
                    await LogAuditEventAsync("MFA_ENABLED", "Security", $"MFA enabled for user {request.Username}", true);
                    
                    return Ok(new
                    {
                        success = true,
                        secret = secret,
                        qrCodeUrl = qrCodeUrl,
                        message = "MFA enabled successfully"
                    });
                }
                else
                {
                    return BadRequest(new { error = "Failed to enable MFA" });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, "EnableMfa");
            }
        }

        [HttpPost("disable-mfa")]
        public async Task<IActionResult> DisableMfa([FromBody] MfaRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username))
                {
                    return BadRequest(new { error = "Username is required" });
                }

                var success = await _authService.DisableMfaAsync(request.Username);
                
                if (success)
                {
                    await LogAuditEventAsync("MFA_DISABLED", "Security", $"MFA disabled for user {request.Username}", true);
                    return Ok(new { success = true, message = "MFA disabled successfully" });
                }
                else
                {
                    return BadRequest(new { error = "Failed to disable MFA" });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, "DisableMfa");
            }
        }

        [HttpGet("sessions/{username}")]
        public async Task<IActionResult> GetActiveSessions(string username)
        {
            try
            {
                var sessions = await _authService.GetActiveSessionsAsync(username);
                
                return Ok(new
                {
                    success = true,
                    sessions = sessions.Select(s => new
                    {
                        sessionToken = s.SessionToken,
                        createdAt = s.CreatedAt,
                        expiresAt = s.ExpiresAt,
                        lastActivity = s.LastActivity,
                        ipAddress = s.IpAddress,
                        userAgent = s.UserAgent
                    })
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetActiveSessions");
            }
        }

        [HttpPost("terminate-session")]
        public async Task<IActionResult> TerminateSession([FromBody] TerminateSessionRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.SessionToken))
                {
                    return BadRequest(new { error = "Session token is required" });
                }

                var success = await _authService.TerminateSessionAsync(request.SessionToken);
                
                if (success)
                {
                    await LogAuditEventAsync("SESSION_TERMINATED", "Security", "Session terminated", true);
                    return Ok(new { success = true, message = "Session terminated successfully" });
                }
                else
                {
                    return BadRequest(new { error = "Failed to terminate session" });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, "TerminateSession");
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? MfaCode { get; set; }
    }

    public class LogoutRequest
    {
        public string SessionToken { get; set; } = string.Empty;
    }

    public class ValidateSessionRequest
    {
        public string SessionToken { get; set; } = string.Empty;
    }

    public class MfaRequest
    {
        public string Username { get; set; } = string.Empty;
    }

    public class TerminateSessionRequest
    {
        public string SessionToken { get; set; } = string.Empty;
    }
}
