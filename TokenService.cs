using System;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MyyPub.Models;

namespace MyyPub.VedicHoroo
{
	public class TokenService : ITokenService
	{
		private const double EXPIRY_DURATION_MINUTES = 30;

		public Token BuildToken(string key, string issuer, string name, string role)
		{
			var claims = new[] {
			new Claim(ClaimTypes.Name, name),
			new Claim(ClaimTypes.Role, role),
			new Claim(ClaimTypes.NameIdentifier,
			Guid.NewGuid().ToString())
			};

			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
			var tokenDescriptor = new JwtSecurityToken(issuer, issuer, claims,
				expires: DateTime.UtcNow.AddDays(2), signingCredentials: credentials);
			return new Token
			{
				token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor),
				expires = tokenDescriptor.ValidTo
			};
		}

		public bool IsTokenValid(string key, string issuer, string audience, string token)
		{
			var mySecret = Encoding.UTF8.GetBytes(key);
			var mySecurityKey = new SymmetricSecurityKey(mySecret);
			var tokenHandler = new JwtSecurityTokenHandler();
			try
			{
				tokenHandler.ValidateToken(token,
				new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidIssuer = issuer,
					ValidAudience = issuer,
					IssuerSigningKey = mySecurityKey,
				}, out SecurityToken validatedToken);
			}
			catch
			{
				return false;
			}
			return true;
		}
	}
}
