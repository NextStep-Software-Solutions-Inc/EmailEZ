using System.Security.Cryptography;
using System.Text;
using EmailEZ.Application.Interfaces;

namespace EmailEZ.Infrastructure.Services.Security;

public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public AesEncryptionService(string secretKey, string secretIv)
    {
        if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(secretIv))
        {
            throw new ArgumentException("Encryption key and IV must not be null or empty.");
        }

        _key = Encoding.UTF8.GetBytes(secretKey);
        _iv = Encoding.UTF8.GetBytes(secretIv);

        if (_key.Length != 32)
        {
            throw new ArgumentException($"AES Key must be 32 bytes (256 bits). Current length: {_key.Length}");
        }
        if (_iv.Length != 16)
        {
            throw new ArgumentException($"AES IV must be 16 bytes (128 bits). Current length: {_iv.Length}");
        }
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = _key;
            aesAlg.IV = _iv;
            aesAlg.Mode = CipherMode.CBC; // Ensure CBC mode is used consistently
            aesAlg.Padding = PaddingMode.PKCS7; // Ensure PKCS7 padding is used

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = _key;
            aesAlg.IV = _iv;
            aesAlg.Mode = CipherMode.CBC; // Ensure CBC mode is used consistently
            aesAlg.Padding = PaddingMode.PKCS7; // Ensure PKCS7 padding is used

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }
}