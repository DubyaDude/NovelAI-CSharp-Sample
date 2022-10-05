using Konscious.Security.Cryptography;
using NovelAI.OpenApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NovelAI
{
    public static class NovelUtils
    {
        public static byte[] NaiHashArgon(int size, string plaintext, string secret, string domain)
        {
            HMACBlake2B encoder = new HMACBlake2B(null, 16 * 8);
            var salt = encoder.ComputeHash(Encoding.UTF8.GetBytes(secret + domain));
            
            var hash = new Argon2id(Encoding.UTF8.GetBytes(plaintext))
            {
                Salt = salt,
                DegreeOfParallelism = 1,
                MemorySize = 2000000 / 1024,
                Iterations = 2
            }.GetBytes(size);
            return hash;
        }

        public static string GenerateLoginString(string email, string password)
        {
            string secret = password.Substring(0, 6) + email;

            byte[] loginHash = NaiHashArgon(64, password, secret, "novelai_data_access_key");
            
            return Convert.ToBase64String(loginHash)
                .Substring(0, 64)
                .Replace('/', '_')
                .Replace('+', '-');
        }
    }
}
