using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Models.Encryption
{
    public class JwtTokenGenerator
    {
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtTokenGenerator(string key, string issuer, string audience)
        {
            _key = key;
            _issuer = issuer;
            _audience = audience;
        }

        public string GenerateToken(IEnumerable<Claim> claims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        //public string Gettoken(IEnumerable<Claim> claims)
        //{
        //    var sequritykey= new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        //    var credentials = new SigningCredentials(sequritykey, SecurityAlgorithms.HmacSha256);
        //    var token = new JwtSecurityToken(issuer:_issuer,
        //        audience:_audience,
        //        expires:DateTime.Now.AddHours(1),
        //        claims:claims,
        //        signingCredentials:credentials
        //        );
        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}
    }
}

