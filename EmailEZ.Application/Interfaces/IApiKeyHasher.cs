namespace EmailEZ.Application.Interfaces;

public interface IApiKeyHasher
{
    string ComputeFastHash(string v);
    string HashApiKey(string apiKey);
    bool VerifyApiKey(string apiKey, string hashedApiKey);
}