using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace EnterpriseITToolkit.Services
{
    public interface ITotpService
    {
        Task<string> GenerateSecretAsync();
        Task<bool> ValidateCodeAsync(string secret, string code);
        Task<string> GenerateQrCodeAsync(string username, string secret);
    }

    public class TotpService : ITotpService
    {
        private readonly ILogger<TotpService> _logger;
        private const int TimeStepSeconds = 30;
        private const int CodeLength = 6;

        public TotpService(ILogger<TotpService> logger)
        {
            _logger = logger;
        }

        public Task<string> GenerateSecretAsync()
        {
            try
            {
                var bytes = new byte[20];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(bytes);
                
                var secret = Base32Encode(bytes);
                _logger.LogInformation("Generated TOTP secret");
                return Task.FromResult(secret);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating TOTP secret");
                throw;
            }
        }

        public Task<bool> ValidateCodeAsync(string secret, string code)
        {
            try
            {
                if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(code))
                    return Task.FromResult(false);

                var currentTimeStep = GetCurrentTimeStep();
                
                // Check current time step
                if (GenerateCode(secret, currentTimeStep) == code)
                    return Task.FromResult(true);

                // Check previous time step (for clock skew tolerance)
                if (GenerateCode(secret, currentTimeStep - 1) == code)
                    return Task.FromResult(true);

                // Check next time step (for clock skew tolerance)
                if (GenerateCode(secret, currentTimeStep + 1) == code)
                    return Task.FromResult(true);

                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating TOTP code");
                return Task.FromResult(false);
            }
        }

        public Task<string> GenerateQrCodeAsync(string username, string secret)
        {
            try
            {
                var issuer = "Enterprise IT Toolkit";
                var accountName = username;
                var qrCodeUrl = $"otpauth://totp/{issuer}:{accountName}?secret={secret}&issuer={issuer}";
                
                _logger.LogInformation("Generated QR code URL for user: {Username}", username);
                return Task.FromResult(qrCodeUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code for user: {Username}", username);
                throw;
            }
        }

        private string GenerateCode(string secret, long timeStep)
        {
            var key = Base32Decode(secret);
            var timeBytes = BitConverter.GetBytes(timeStep);
            
            if (BitConverter.IsLittleEndian)
                Array.Reverse(timeBytes);

            using var hmac = new HMACSHA1(key);
            var hash = hmac.ComputeHash(timeBytes);
            
            var offset = hash[hash.Length - 1] & 0x0F;
            var code = ((hash[offset] & 0x7F) << 24) |
                      ((hash[offset + 1] & 0xFF) << 16) |
                      ((hash[offset + 2] & 0xFF) << 8) |
                      (hash[offset + 3] & 0xFF);

            return (code % (int)Math.Pow(10, CodeLength)).ToString().PadLeft(CodeLength, '0');
        }

        private long GetCurrentTimeStep()
        {
            var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return unixTime / TimeStepSeconds;
        }

        private string Base32Encode(byte[] bytes)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var output = new StringBuilder();
            
            for (int i = 0; i < bytes.Length; i += 5)
            {
                var byteCount = Math.Min(5, bytes.Length - i);
                var buffer = new byte[5];
                Array.Copy(bytes, i, buffer, 0, byteCount);
                
                var bits = byteCount * 8;
                var value = 0;
                
                for (int j = 0; j < byteCount; j++)
                {
                    value = (value << 8) | buffer[j];
                }
                
                var padding = (5 - byteCount) * 8;
                value <<= padding;
                
                for (int j = 0; j < (bits + 4) / 5; j++)
                {
                    var index = (value >> (35 - j * 5)) & 0x1F;
                    output.Append(alphabet[index]);
                }
            }
            
            return output.ToString();
        }

        private byte[] Base32Decode(string input)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            input = input.ToUpper().Replace(" ", "");
            
            var output = new List<byte>();
            var bits = 0;
            var value = 0;
            
            foreach (var c in input)
            {
                var index = alphabet.IndexOf(c);
                if (index == -1) continue;
                
                value = (value << 5) | index;
                bits += 5;
                
                if (bits >= 8)
                {
                    output.Add((byte)(value >> (bits - 8)));
                    bits -= 8;
                }
            }
            
            return output.ToArray();
        }
    }
}
