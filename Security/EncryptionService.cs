using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Security;

namespace EnterpriseITToolkit.Security
{
    public interface IEncryptionService
    {
        string EncryptString(string plainText, string password);
        string DecryptString(string cipherText, string password);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
        string GenerateSecureToken();
        byte[] EncryptFile(byte[] fileData, string password);
        byte[] DecryptFile(byte[] encryptedData, string password);
    }

    public class EncryptionService : IEncryptionService
    {
        private readonly ILogger<EncryptionService> _logger;
        private const int KeySize = 256;
        private const int IvSize = 128;
        private const int SaltSize = 32;
        private const int Iterations = 10000;

        public EncryptionService(ILogger<EncryptionService> logger)
        {
            _logger = logger;
        }

        public string EncryptString(string plainText, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(password))
                {
                    throw new ArgumentException("Plain text and password cannot be null or empty");
                }

                // Generate random salt
                var salt = new byte[SaltSize];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                // Derive key from password
                var key = DeriveKeyFromPassword(password, salt);

                // Generate random IV
                var iv = new byte[IvSize / 8];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(iv);
                }

                // Encrypt the data
                using var aes = Aes.Create();
                aes.KeySize = KeySize;
                aes.BlockSize = IvSize;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = key;
                aes.IV = iv;

                using var encryptor = aes.CreateEncryptor();
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                // Combine salt, IV, and cipher text
                var result = new byte[salt.Length + iv.Length + cipherBytes.Length];
                Array.Copy(salt, 0, result, 0, salt.Length);
                Array.Copy(iv, 0, result, salt.Length, iv.Length);
                Array.Copy(cipherBytes, 0, result, salt.Length + iv.Length, cipherBytes.Length);

                return Convert.ToBase64String(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting string");
                throw new SecurityException("Encryption failed", ex);
            }
        }

        public string DecryptString(string cipherText, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(cipherText) || string.IsNullOrEmpty(password))
                {
                    throw new ArgumentException("Cipher text and password cannot be null or empty");
                }

                var cipherBytes = Convert.FromBase64String(cipherText);

                // Extract salt, IV, and cipher text
                var salt = new byte[SaltSize];
                var iv = new byte[IvSize / 8];
                var encryptedData = new byte[cipherBytes.Length - salt.Length - iv.Length];

                Array.Copy(cipherBytes, 0, salt, 0, salt.Length);
                Array.Copy(cipherBytes, salt.Length, iv, 0, iv.Length);
                Array.Copy(cipherBytes, salt.Length + iv.Length, encryptedData, 0, encryptedData.Length);

                // Derive key from password
                var key = DeriveKeyFromPassword(password, salt);

                // Decrypt the data
                using var aes = Aes.Create();
                aes.KeySize = KeySize;
                aes.BlockSize = IvSize;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = key;
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor();
                var plainBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting string");
                throw new SecurityException("Decryption failed", ex);
            }
        }

        public string HashPassword(string password)
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                {
                    throw new ArgumentException("Password cannot be null or empty");
                }

                // Generate random salt
                var salt = new byte[SaltSize];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                // Hash the password with salt
                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
                var hash = pbkdf2.GetBytes(32);

                // Combine salt and hash
                var result = new byte[salt.Length + hash.Length];
                Array.Copy(salt, 0, result, 0, salt.Length);
                Array.Copy(hash, 0, result, salt.Length, hash.Length);

                return Convert.ToBase64String(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hashing password");
                throw new SecurityException("Password hashing failed", ex);
            }
        }

        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
                {
                    return false;
                }

                var hashBytes = Convert.FromBase64String(hash);

                // Extract salt
                var salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, salt.Length);

                // Hash the provided password with the same salt
                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
                var testHash = pbkdf2.GetBytes(32);

                // Compare hashes
                for (int i = 0; i < 32; i++)
                {
                    if (hashBytes[i + salt.Length] != testHash[i])
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password");
                return false;
            }
        }

        public string GenerateSecureToken()
        {
            try
            {
                var bytes = new byte[32];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating secure token");
                throw new SecurityException("Token generation failed", ex);
            }
        }

        public byte[] EncryptFile(byte[] fileData, string password)
        {
            try
            {
                if (fileData == null || fileData.Length == 0 || string.IsNullOrEmpty(password))
                {
                    throw new ArgumentException("File data and password cannot be null or empty");
                }

                // Generate random salt and IV
                var salt = new byte[SaltSize];
                var iv = new byte[IvSize / 8];
                
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                    rng.GetBytes(iv);
                }

                // Derive key from password
                var key = DeriveKeyFromPassword(password, salt);

                // Encrypt the file data
                using var aes = Aes.Create();
                aes.KeySize = KeySize;
                aes.BlockSize = IvSize;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = key;
                aes.IV = iv;

                using var encryptor = aes.CreateEncryptor();
                var encryptedData = encryptor.TransformFinalBlock(fileData, 0, fileData.Length);

                // Combine salt, IV, and encrypted data
                var result = new byte[salt.Length + iv.Length + encryptedData.Length];
                Array.Copy(salt, 0, result, 0, salt.Length);
                Array.Copy(iv, 0, result, salt.Length, iv.Length);
                Array.Copy(encryptedData, 0, result, salt.Length + iv.Length, encryptedData.Length);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting file");
                throw new SecurityException("File encryption failed", ex);
            }
        }

        public byte[] DecryptFile(byte[] encryptedData, string password)
        {
            try
            {
                if (encryptedData == null || encryptedData.Length == 0 || string.IsNullOrEmpty(password))
                {
                    throw new ArgumentException("Encrypted data and password cannot be null or empty");
                }

                // Extract salt, IV, and encrypted data
                var salt = new byte[SaltSize];
                var iv = new byte[IvSize / 8];
                var cipherData = new byte[encryptedData.Length - salt.Length - iv.Length];

                Array.Copy(encryptedData, 0, salt, 0, salt.Length);
                Array.Copy(encryptedData, salt.Length, iv, 0, iv.Length);
                Array.Copy(encryptedData, salt.Length + iv.Length, cipherData, 0, cipherData.Length);

                // Derive key from password
                var key = DeriveKeyFromPassword(password, salt);

                // Decrypt the data
                using var aes = Aes.Create();
                aes.KeySize = KeySize;
                aes.BlockSize = IvSize;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = key;
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor();
                var decryptedData = decryptor.TransformFinalBlock(cipherData, 0, cipherData.Length);

                return decryptedData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting file");
                throw new SecurityException("File decryption failed", ex);
            }
        }

        private byte[] DeriveKeyFromPassword(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(KeySize / 8);
        }
    }
}
