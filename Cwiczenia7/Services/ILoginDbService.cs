using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cwiczenia7.Services
{
    public interface ILoginDbService
    {
        string GetPassword(string login);
        int GetId(string login);
        string GetName(int id);
        IEnumerable<string> GetRoles(int id);
        void AddToken(Guid token, List<Claim> claims);
        void RemoveToken(Guid token);
        bool ContainsToken(Guid token);
        List<Claim> Get(Guid token);
        string GetAll();
        int Size();
    }
}
