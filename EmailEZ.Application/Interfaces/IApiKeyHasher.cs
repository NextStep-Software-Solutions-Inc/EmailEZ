namespace EmailEZ.Application.Interfaces;

public interface IApiKeyHasher
{
    string HashApiKey(string apiKey);
    bool VerifyApiKey(string apiKey, string hashedApiKey);
}