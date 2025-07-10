using EmailEZ.Application.Interfaces;
using Konscious.Security.Cryptography; // Correct library namespace
using System.Security.Cryptography;
using System.Text;

namespace EmailEZ.Infrastructure.Services.Security;

public class ApiKeyHasher : IApiKeyHasher
{
    // Define Argon2 parameters with the CORRECT PROPERTY NAMES
    private const int MemorySize = 65536; // 64 MB (was MemoryCost)
    private const int Iterations = 3;       // Number of iterations (was TimeCost)
    private const int Parallelism = 1;    // Number of lanes/threads
    private const int SaltSize = 16;      // 16 bytes = 128 bits of salt
    private const int HashSize = 32;      // 32 bytes = 256 bits of hash output

    public string ComputeFastHash(string apiKey)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(hash);
    }

    public string HashApiKey(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new ArgumentNullException(nameof(apiKey), "API Key cannot be null or empty.");
        }

        byte[] salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt);

        using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(apiKey)))
        {
            argon2.Salt = salt;
            argon2.MemorySize = MemorySize; 
            argon2.Iterations = Iterations; 
            argon2.DegreeOfParallelism = Parallelism; // Typically same as Parallelism

            // Note on AssociatedData: If you want to use AssociatedData (e.g., userUuidBytes)
            // you would set it here: argon2.AssociatedData = userUuidBytes;
            // However, this data MUST also be passed during verification.
            // For general API keys, it's often omitted.

            byte[] hash = argon2.GetBytes(HashSize);

            // Combine salt, parameters, and hash into a single string for storage
            // Format: $argon2id$v=19$m={memory},t={time},p={parallelism}${salt_base64}${hash_base64}
            string hashedString = $"$argon2id$v=19$m={MemorySize},i={Iterations},p={Parallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
            return hashedString;
        }
    }

    public bool VerifyApiKey(string apiKey, string hashedApiKey)
    {
        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(hashedApiKey))
        {
            return false;
        }

        try
        {
            string[] parts = hashedApiKey.Split('$');

            if (parts.Length != 6 || parts[1] != "argon2id" || !parts[2].StartsWith("v="))
            {
                return false;
            }

            string[] paramParts = parts[3].Split(',');
            if (paramParts.Length != 3 || !paramParts[0].StartsWith("m=") || !paramParts[1].StartsWith("i=") || !paramParts[2].StartsWith("p="))
            {
                return false;
            }
            int memorySize = int.Parse(paramParts[0].Substring(2));  
            int iterations = int.Parse(paramParts[1].Substring(2)); 
            int parallelism = int.Parse(paramParts[2].Substring(2));

            byte[] salt = Convert.FromBase64String(parts[4]);
            byte[] storedHash = Convert.FromBase64String(parts[5]);

            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(apiKey)))
            {
                argon2.Salt = salt;
                argon2.MemorySize = memorySize;     
                argon2.Iterations = iterations;     
                argon2.DegreeOfParallelism = parallelism;

                // If you used AssociatedData during hashing, you MUST set it here as well for verification
                // argon2.AssociatedData = userUuidBytes; // e.g., if binding API key to a specific user

                byte[] computedHash = argon2.GetBytes(storedHash.Length);

                return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
            }
        }
        catch (FormatException)
        {
            Console.Error.WriteLine("API Key verification failed: Format exception during parsing hash.");
            return false;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An unexpected error occurred during API key verification: {ex.Message}");
            return false;
        }
    }
}