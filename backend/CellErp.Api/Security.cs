using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CellErp.Api;

public static class PasswordService
{
    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 120_000, HashAlgorithmName.SHA256, 32);
        return $"pbkdf2-sha256$120000${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string encoded)
    {
        var parts = encoded.Split('$');
        if (parts.Length != 4 || parts[0] != "pbkdf2-sha256" || !int.TryParse(parts[1], out var iterations)) return false;
        var salt = Convert.FromBase64String(parts[2]);
        var expected = Convert.FromBase64String(parts[3]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}

public sealed class JwtService(IConfiguration cfg)
{
    public string Create(AppUser user)
    {
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? cfg["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret missing");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role),
            new("display_name", user.DisplayName),
            new("permissions", user.PermissionsCsv)
        };
        if (user.StoreId.HasValue) claims.Add(new("store_id", user.StoreId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: cfg["Jwt:Issuer"],
            audience: cfg["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public static class UserClaims
{
    public static bool IsSuperAdmin(this ClaimsPrincipal user) => user.IsInRole(Roles.SuperAdmin);
    public static bool HasPermission(this ClaimsPrincipal user, string permission)
    {
        if (user.IsSuperAdmin() || user.IsInRole(Roles.Owner)) return true;
        var raw = user.FindFirst("permissions")?.Value ?? "";
        return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    public static Guid UserId(this ClaimsPrincipal user)
        => Guid.Parse(user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? user.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException());
}
