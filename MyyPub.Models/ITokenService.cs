namespace MyyPub.Models
{
    public interface ITokenService
    {
        Token BuildToken(string key, string issuer, string name, string role);
        bool IsTokenValid(string key, string issuer, string audience, string token);
    }
}
