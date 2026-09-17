namespace QLTBYT.API.Configuration;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "QLTBYT-API";
    public string Audience { get; set; } = "QLTBYT-Frontend";
    public int ExpiryHours { get; set; } = 24;
}
