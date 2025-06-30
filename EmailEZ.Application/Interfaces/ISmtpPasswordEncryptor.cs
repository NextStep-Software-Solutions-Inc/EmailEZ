namespace EmailEZ.Application.Interfaces;

public interface ISmtpPasswordEncryptor
{
    string Encrypt(string plainText);
    string Decrypt(string encryptedText);
}