using Konscious.Security.Cryptography;
using System;
using System.Text;
namespace Testproject.Models.Encryption
{
    public static class Argon2Utils
    {
        private const int SaltSize = 16; // 128 bits
        private const int HashSize = 32; // 256 bits (32 bytes)
        private const int Iterations = 4; // Number of iterations
        private const int MemorySize = 65536; // Memory size in KB (64MB)
        private const int DegreeOfParallelism = 8; // Number of threads to use

        // Generates a random salt
        private static byte[] GenerateSalt()
        {
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                var salt = new byte[SaltSize];
                rng.GetBytes(salt);
                return salt;
            }
        }
        
        // Hashes the password with the given salt
        private static string GetHashPassword(string password, byte[] salt)
        {
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            using (var hasher = new Argon2id(passwordBytes))
            {
                hasher.Salt = salt;
                hasher.DegreeOfParallelism = DegreeOfParallelism;
                hasher.MemorySize = MemorySize;
                hasher.Iterations = Iterations;

                // Compute the hash
                var hashBytes = hasher.GetBytes(HashSize);
                return Convert.ToBase64String(hashBytes);
            }
        }
        public static (string hashPassword, byte[] salt) HashPassword(string password)
        {
            //generating random salt
            var salt = GenerateSalt();
            // encrypting password with adding addition encrypt(salt)
            return (GetHashPassword(password, salt), salt);

        }

        // Verifies the password against the stored hash and salt
        public static bool VerifyPassword(string password, string storedHash, byte[] salt)
        {
            var hash = GetHashPassword(password, salt);
            return hash.Equals(storedHash);
        }
    }



}
