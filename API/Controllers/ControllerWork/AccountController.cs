using API.Models;
using DataTier.Entities.Abstract;
using DataTier.Entities.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace API.Controllers.ControllerWork
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private EFDbContext _context;

        public AccountController(EFDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("Login")]
        public async Task Token([FromBody] LoginModel authUser)
        {
            var identity = GetIdentity(authUser.Login, authUser.Password);
            if (identity == null)
            {
                var error = new
                {
                    status = 400,
                    message = "Неправильный логин или пароль"
                };
                Response.ContentType = "application/json";
                await Response.WriteAsync(JsonConvert.SerializeObject(error, new JsonSerializerSettings { Formatting = Formatting.Indented }));
                return;
            }
            var dismissUser = _context.Users.FirstOrDefault(u => u.Login == authUser.Login && u.Password == authUser.Password);
            if (dismissUser.Status == EmployeeStatus.NotActive)
            {
                var dismissError = new
                {
                    status = 400,
                    message = "Сотрудник уволен"
                };
                Response.ContentType = "application/json";
                await Response.WriteAsync(JsonConvert.SerializeObject(dismissError, new JsonSerializerSettings { Formatting = Formatting.Indented }));
                return;
            }
            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var user = _context.Users.FirstOrDefault(u => u.Id == int.Parse(identity.Claims.First(i => i.Type == "UserId").Value));

            var response = new
            {
                access_token = encodedJwt,
                username = identity.Name,
                role = user.Role
        };

            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }

        private ClaimsIdentity GetIdentity(string login, string password)
        {
            User user = _context.Users.FirstOrDefault(u => u.Login == login && u.Password == password);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim("UserId", user.Id.ToString()),
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role.ToString())
                };
                ClaimsIdentity claimsIdentity =
                    new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
                return claimsIdentity;
            }
            return null;
        }
    }
}