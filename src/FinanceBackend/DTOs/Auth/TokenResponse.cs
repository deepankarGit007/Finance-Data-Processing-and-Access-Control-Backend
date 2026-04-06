namespace FinanceBackend.DTOs.Auth;

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType   { get; set; } = "Bearer";
    public int    ExpiresIn   { get; set; }   // seconds
    public string Role        { get; set; } = string.Empty;
}
