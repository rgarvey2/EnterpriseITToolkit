using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace EnterpriseITToolkit.Security
{
    public enum InputType
    {
        Username,
        Hostname,
        IPAddress,
        General
    }
    
    public class ValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<string> Errors { get; } = new List<string>();
        public string SanitizedValue { get; set; }
        
        public void AddError(string error)
        {
            Errors.Add(error);
        }
    }
    
    public class SecureInputValidator
    {
        public ValidationResult ValidateAndSanitize(string input, InputType type)
        {
            var result = new ValidationResult();
            
            if (string.IsNullOrEmpty(input))
            {
                result.AddError("Input cannot be empty");
                return result;
            }
            
            // Length validation
            if (input.Length > GetMaxLength(type))
            {
                result.AddError($"Input exceeds maximum length of {GetMaxLength(type)} characters");
                return result;
            }
            
            // Type-specific validation
            switch (type)
            {
                case InputType.Username:
                    if (!Regex.IsMatch(input, @"^[a-zA-Z0-9._-]+$"))
                        result.AddError("Username contains invalid characters");
                    break;
                    
                case InputType.Hostname:
                    if (!Regex.IsMatch(input, @"^[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                        result.AddError("Invalid hostname format");
                    break;
                    
                case InputType.IPAddress:
                    if (!System.Net.IPAddress.TryParse(input, out _))
                        result.AddError("Invalid IP address format");
                    break;
            }
            
            // SQL injection prevention
            if (ContainsSqlInjection(input))
                result.AddError("Input contains potentially malicious content");
            
            // XSS prevention
            if (ContainsXss(input))
                result.AddError("Input contains potentially malicious content");
            
            // Sanitize input
            result.SanitizedValue = SanitizeInput(input, type);
            
            return result;
        }
        
        private bool ContainsSqlInjection(string input)
        {
            var sqlPatterns = new[]
            {
                @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|UNION)\b)",
                @"(--|#|\/\*|\*\/)",
                @"(;|\||&)",
                @"(xp_|sp_)"
            };
            
            return sqlPatterns.Any(pattern => Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase));
        }
        
        private bool ContainsXss(string input)
        {
            var xssPatterns = new[]
            {
                @"<script[^>]*>.*?</script>",
                @"javascript:",
                @"on\w+\s*=",
                @"<iframe[^>]*>",
                @"<object[^>]*>",
                @"<embed[^>]*>"
            };
            
            return xssPatterns.Any(pattern => Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase));
        }
        
        private string SanitizeInput(string input, InputType type)
        {
            // HTML encode
            input = HttpUtility.HtmlEncode(input);
            
            // Remove potentially dangerous characters
            input = Regex.Replace(input, @"[<>""'&]", "");
            
            return input.Trim();
        }
        
        private int GetMaxLength(InputType type)
        {
            return type switch
            {
                InputType.Username => 50,
                InputType.Hostname => 255,
                InputType.IPAddress => 45,
                _ => 1000
            };
        }
    }
}
