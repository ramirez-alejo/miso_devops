using System.Text;

namespace BlackListApi.Infrastructure;

public static class TokenProvider
{
	public static string GenerateToken(string id, string salt, int tokenLifetimeHours)
	{
		var expireAt = DateTime.UtcNow.AddHours(tokenLifetimeHours);
		// Let's create an encoded string that contains the user id and an expiration date
		var token = $"{id}|{expireAt:O}";
		// Let's encrypt the token
		token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token + salt));
		return token;
	}
}