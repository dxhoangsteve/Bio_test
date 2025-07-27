using System;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("🔐 BioWeb Admin Password Changer");
        Console.WriteLine("================================");
        
        if (args.Length >= 2)
        {
            // Command line mode
            string newUsername = args[0];
            string newPassword = args[1];
            
            string hashedPassword = HashPassword(newPassword);
            
            Console.WriteLine($"New Username: {newUsername}");
            Console.WriteLine($"Hashed Password: {hashedPassword}");
            Console.WriteLine();
            Console.WriteLine("SQLite Commands:");
            Console.WriteLine($"UPDATE AdminUsers SET Username = '{newUsername}' WHERE Username = 'admin';");
            Console.WriteLine($"UPDATE AdminUsers SET PasswordHash = '{hashedPassword}' WHERE Username = '{newUsername}';");
        }
        else
        {
            // Interactive mode
            Console.Write("Enter new username: ");
            string newUsername = Console.ReadLine() ?? "admin";
            
            Console.Write("Enter new password: ");
            string newPassword = Console.ReadLine() ?? "";
            
            if (string.IsNullOrEmpty(newPassword))
            {
                Console.WriteLine("Password cannot be empty!");
                return;
            }
            
            string hashedPassword = HashPassword(newPassword);
            
            Console.WriteLine();
            Console.WriteLine("=== COPY THESE COMMANDS ===");
            Console.WriteLine($"UPDATE AdminUsers SET Username = '{newUsername}' WHERE Username = 'admin';");
            Console.WriteLine($"UPDATE AdminUsers SET PasswordHash = '{hashedPassword}' WHERE Username = '{newUsername}';");
            Console.WriteLine("===========================");
        }
    }
    
    static string HashPassword(string password)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
