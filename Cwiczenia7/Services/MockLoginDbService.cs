using Cwiczenia7.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cwiczenia7.Services
{
    public class MockLoginDbService : ILoginDbService
    {
        private static IEnumerable<Credential> _credentials = new List<Credential>
        {
            new Credential{Login = "jajecznica", Password = "alamakota", Id = 1},
            new Credential{Login = "judym", Password = "pannajoanna", Id = 2}
        };
        private static IEnumerable<Student> _students = new List<Student>
        {
            new Student{FirstName = "January", LastName = "Jajecznica", IdStudent = 1},
            new Student{FirstName = "Tomasz", LastName = "Judym", IdStudent = 2}
        };
        private static IEnumerable<Role> _roles = new List<Role>
        {
            new Role{Id = 1, Name = "admin"},
            new Role{Id = 1, Name = "student"},
            new Role{Id = 2, Name = "admin"},
            new Role{Id = 2, Name = "employee"}
        };
        private static Dictionary<Guid, List<Claim>> _refreshTokens = new Dictionary<Guid, List<Claim>>();

        public int GetId(string login)
        {
            var id = _credentials
                .Where(c => c.Login == login)
                .Select(c => c.Id);
            return id.ToArray()[0];
        }

        public string GetName(int id)
        {
            var name = _students
                .Where(s => s.IdStudent == id)
                .Select(s => s.FirstName);
            return name.ToArray()[0];
        }

        public string GetPassword(string login)
        {
            var password = _credentials
                .Where(c => c.Login == login)
                .Select(c => c.Password);
            return password.ToArray()[0];
        }

        public IEnumerable<string> GetRoles(int id)
        {
            return _roles
                .Where(r => r.Id == id)
                .Select(r => r.Name).ToList();
        }

        public void AddToken(Guid token, List<Claim> claims)
        {
            _refreshTokens.Add(token, claims);
        }

        public void RemoveToken(Guid token)
        {
            _refreshTokens.Remove(token);
        }

        public bool ContainsToken(Guid token)
        {
            return _refreshTokens.ContainsKey(token);
        }

        public List<Claim> Get(Guid token)
        {
            var claims = new List<Claim>();
            _refreshTokens.TryGetValue(token, out claims);
            return claims;
        }

        public int Size()
        {
            return _refreshTokens.Count;
        }

        public String GetAll()
        {
            return _refreshTokens.ToString();
        }
    }
}
