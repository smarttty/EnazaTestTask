using EnazaTestTask.Models;
using EnazaTestTask.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace EnazaTestTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class SecurityController : ControllerBase
    {
        EnazaTestTaskContext _context;
        IConfiguration _configuration;
        public SecurityController(EnazaTestTaskContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Метод получения токена для пользователя
        /// </summary>
        /// <param name="user">Логин и пароль пользователя</param>
        /// <returns>Объект с токеном или ошибку</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IResult> GetToken(UserAuth user)
        {
            User validUser = await _context.Users
                .Include(u => u.UserGroup)
                .Include(u => u.UserState)
                .FirstOrDefaultAsync(u =>
                u.Password == PasswordHasher.SHA512(user.Password) &&
                u.Login == user.Login &&
                u.UserState.Code == "Active");

            if (validUser != null)
            {
                var issuer = _configuration["Jwt:Issuer"];
                var audience = _configuration["Jwt:Audience"];
                var key = Encoding.ASCII.GetBytes
                (_configuration["Jwt:Key"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                new Claim("Id", Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Login),
                new Claim(JwtRegisteredClaimNames.Email, user.Login),
                new Claim(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),
                new Claim("Role", validUser.UserGroup.Code)
             }),
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials
                    (new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = tokenHandler.WriteToken(token);
                var stringToken = tokenHandler.WriteToken(token);
                return Results.Ok(stringToken);
            }
            return Results.Unauthorized();
        }
    }
}
