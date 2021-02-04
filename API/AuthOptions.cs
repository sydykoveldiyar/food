using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API
{
    public class AuthOptions
    {
        public const string ISSUER = "CRMServer";
        public const string AUDIENCE = "CRMFood";
        const string KEY = "mysupersecret_secretkey!123";
        public const int LIFETIME = 28800;

        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
