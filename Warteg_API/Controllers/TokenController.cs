using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Warteg_API.Context;

namespace Warteg_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TokenController : ControllerBase
    {
        public IConfiguration _configuration;
        private readonly WartegContext _context;
        public TokenController(IConfiguration config,WartegContext context)
        {
            _configuration = config;
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromQuery] string Email,[FromQuery] string Password)
        {
            if(!string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Password))
            {
                var user = await GetUser(Email, Password);
                if(user != null)
                {
                    var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub,_configuration["Jwt:Subject"]),
                        new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat,DateTime.UtcNow.ToString()),
                        new Claim("CustomerId",user.Id.ToString()),
                        new Claim("Email",user.Email)
                    };
                    var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:key"]));
                    var SignIn = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);
                    var Token = new JwtSecurityToken(
                        _configuration["Jwt:Issuer"],
                        _configuration["Jwt:Audience"],
                        claims,
                        expires: DateTime.UtcNow.AddMinutes(60),
                        signingCredentials: SignIn
                        );
                    var Result = new
                    {
                        access_token = new JwtSecurityTokenHandler().WriteToken(Token),
                        token_type = "Bearer",
                        expires_in = Token.ValidTo
                    };
                    return Ok(Result);
                }
                else
                {
                    return BadRequest("Invalid Credentials");
                }
            }
            return BadRequest("Please fill email and password!");
        }
        private async Task<Customer> GetUser(string Email,string Password)
        {
            return await _context.Customers.FirstOrDefaultAsync(x => x.Email == Email && x.Password == Password);
        }
    }
}
