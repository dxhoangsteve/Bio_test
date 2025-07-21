using System.Security.Cryptography;
using System.Text;

namespace BioWeb.Server.Services
{
    public class PasswordService
    {
        /// <summary>
        /// Hash password sử dụng SHA256
        /// </summary>
        /// <param name="password">Password gốc</param>
        /// <returns>Password đã hash</returns>
        public static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Chuyển password thành byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Chuyển byte array thành string hex
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Verify password
        /// </summary>
        /// <param name="password">Password gốc</param>
        /// <param name="hashedPassword">Password đã hash</param>
        /// <returns>True nếu password đúng</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            string hashOfInput = HashPassword(password);
            return hashOfInput.Equals(hashedPassword, StringComparison.OrdinalIgnoreCase);
        }
    }
}
