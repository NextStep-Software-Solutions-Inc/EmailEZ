## Security Configuration: Encryption Keys (AES)

This application uses AES encryption for sensitive data, such as email configuration passwords. The encryption relies on a **Secret Key (32 bytes)** and an **Initialization Vector (IV) (16 bytes)**.

**It is absolutely CRITICAL that these keys are kept secret and are NOT hardcoded in your source code or committed to your version control system.**

### Key Management Guidelines:

1.  **For Local Development:**
    * Use **User Secrets**. This is the recommended way to store development-time secrets locally without checking them into your repository.
    * To set them up, navigate to your `EmailEZ.Api` project in Visual Studio, right-click, and select "Manage User Secrets". Add the following to the opened `secrets.json` file:
        ```json
        {
          "EncryptionSettings:Key": "YourActualDevSecretKey32BytesLongxxxxxx",
          "EncryptionSettings:IV": "YourActualDevIV16Bytes"
        }
        ```
        * **Replace the placeholder values** with your own unique, freshly generated keys.

2.  **For Production/Staging Deployments:**
    * Use **Environment Variables**. ASP.NET Core's configuration system automatically reads environment variables, overriding values from `appsettings.json`. This is the most common and robust way to manage secrets in production.
    * When deploying (e.g., to Docker, Kubernetes, Azure App Service, AWS EC2/ECS), set environment variables with the following names:
        * `EncryptionSettings__Key` (for Linux/Docker, double underscore)
        * `EncryptionSettings__IV` (for Linux/Docker, double underscore)
        * Alternatively, `EncryptionSettings:Key` and `EncryptionSettings:IV` can be used, depending on your environment.

### How to Generate New Keys:

You can generate new, cryptographically strong keys using the following C# code snippet:

```csharp
using System.Security.Cryptography;
using System;
using System.Text; // Required for Encoding.UTF8.GetBytes if you convert to bytes

public class KeyGenerator
{
    public static void GenerateAesKeys()
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.GenerateKey(); // Generates a 256-bit (32-byte) key
            aesAlg.GenerateIV();  // Generates a 128-bit (16-byte) IV

            string base64Key = Convert.ToBase64String(aesAlg.Key);
            string base64IV = Convert.ToBase64String(aesAlg.IV);

            Console.WriteLine("--- New AES Encryption Keys ---");
            Console.WriteLine($"Key (Base64):   \"{base64Key}\"");
            Console.WriteLine($"IV (Base64):    \"{base64IV}\"");
            Console.WriteLine("-----------------------------");
            Console.WriteLine("Important: Keep these values secure and do not hardcode them!");
        }
    }
}
// You can call KeyGenerator.GenerateAesKeys() from a temporary console app or a test.