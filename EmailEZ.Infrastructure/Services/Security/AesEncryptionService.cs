using System.Security.Cryptography;
using EmailEZ.Application.Interfaces;

namespace EmailEZ.Infrastructure.Services.Security; // Ensure correct namespace

public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    public AesEncryptionService(string secretKeyBase64) // Constructor now only takes the key
    {
        if (string.IsNullOrWhiteSpace(secretKeyBase64))
        {
            throw new ArgumentException("Encryption key must not be null or empty Base64 string.");
        }

        try
        {
            _key = Convert.FromBase64String(secretKeyBase64);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Encryption key is not a valid Base64 string.", ex);
        }

        if (_key.Length != 32)
        {
            throw new ArgumentException($"AES Key must be 32 bytes (256 bits). Current decoded length: {_key.Length}");
        }

        // Removed the _iv field and related checks here
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText; // Or throw ArgumentException if empty string encryption is not desired

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = _key;
            aesAlg.GenerateIV(); // Generates a NEW random IV for each encryption
            byte[] iv = aesAlg.IV; // Get the generated IV

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, iv);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                // Prepend the IV to the ciphertext.
                // This is crucial for decryption, as the IV is now random for each operation.
                msEncrypt.Write(iv, 0, iv.Length);

                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    public string Decrypt(string cipherTextBase64)
    {
        if (string.IsNullOrEmpty(cipherTextBase64))
            return cipherTextBase64; // Or throw ArgumentException

        byte[] cipherTextWithIvBytes = Convert.FromBase64String(cipherTextBase64);

        // Ensure the ciphertext is long enough to contain the IV
        if (cipherTextWithIvBytes.Length < 16)
        {
            throw new ArgumentException("Ciphertext is too short to contain a valid IV and encrypted data.");
        }

        // Extract the IV from the beginning of the ciphertext
        byte[] iv = new byte[16];
        Buffer.BlockCopy(cipherTextWithIvBytes, 0, iv, 0, 16);

        // Get the actual encrypted data (ciphertext without the IV)
        byte[] cipherTextBytes = new byte[cipherTextWithIvBytes.Length - 16];
        Buffer.BlockCopy(cipherTextWithIvBytes, 16, cipherTextBytes, 0, cipherTextBytes.Length);

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = _key;
            aesAlg.IV = iv; // Use the extracted IV

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(cipherTextBytes))
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
            {
                return srDecrypt.ReadToEnd();
            }
        }
    }
}