using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Cwiczenia7.DTOs;
using Cwiczenia7.Models;
using Cwiczenia7.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Cwiczenia7.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private ILoginDbService _loginDbService;
        public IConfiguration Configuration { get; set; }

        public StudentsController(IConfiguration configuration, ILoginDbService loginDbService)
        {
            Configuration = configuration;
            _loginDbService = loginDbService;
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult GetStudents()
        {
            var list = new List<Student>();
            list.Add(new Student
            {
                IdStudent = 1,
                FirstName = "Jan",
                LastName = "Kowalski"
            });
            list.Add(new Student
            {
                IdStudent = 2,
                FirstName = "Andrzej",
                LastName = "Malewski"
            });

            return Ok(list);
        }

        [HttpPut]
        [Authorize(Roles = "employee")]
        public IActionResult PromoteStudents()
        {
            return Ok();
        }

        [HttpPost("/enrollstudent")]
        [Authorize(Roles = "employee")]
        public IActionResult EnrollStudent(Student student)
        {
            return Ok();
        }

        [HttpPost]
        public IActionResult Login(LoginRequestDto request)
        {
            if (_loginDbService.GetPassword(request.Login) == null || _loginDbService.GetPassword(request.Login) != request.Haslo)
            {
                return BadRequest("Niepoprawny login lub hasło.");
            }

            int id = _loginDbService.GetId(request.Login);
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, _loginDbService.GetName(id)));
            foreach (string role in _loginDbService.GetRoles(id))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            /*
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "jan123"),
                new Claim(ClaimTypes.Role, "admin"),
                new Claim(ClaimTypes.Role, "student")
            };
            */

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: "Gakko",
                audience: "Students",
                claims: claims.ToArray(),
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );
            var refreshToken = Guid.NewGuid();

            _loginDbService.AddToken(refreshToken, claims);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken
            });
        }

        [HttpPost("{refreshToken}")]
        public IActionResult Refresh(String refreshToken)
        {
            if (!_loginDbService.ContainsToken(Guid.Parse(refreshToken)))
            {
                return BadRequest(_loginDbService.GetAll());
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = _loginDbService.Get(Guid.Parse(refreshToken));

            var token = new JwtSecurityToken
            (
                issuer: "Gakko",
                audience: "Students",
                claims: claims.ToArray(),
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );
            var rToken = Guid.NewGuid();

            _loginDbService.AddToken(rToken, claims);
            _loginDbService.RemoveToken(Guid.Parse(refreshToken));

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                rToken
            });
        }
    }
}
