﻿using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace SDServerTP2.Authentication
{
    public class OperadorAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly FirebaseApp _firebaseApp;


        public OperadorAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions>
            options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, FirebaseApp firebaseApp)
            : base(options, logger, encoder, clock)
        {
            _firebaseApp = firebaseApp;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Context.Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.NoResult();
            }

            string bearerToken = Context.Request.Headers["Authorization"];

            if (bearerToken == null && !bearerToken.StartsWith("Bearer "))
            {
                return AuthenticateResult.Fail("Wrong Scheme");
            }

            string token = bearerToken.Substring("Bearer ".Length);

            try
            {
                FirebaseToken firebaseToken = await FirebaseAuth.GetAuth(_firebaseApp).VerifyIdTokenAsync(token);

                return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new List<ClaimsIdentity>()
                {
                new ClaimsIdentity(ToClaims(firebaseToken.Claims), nameof(OperadorAuthenticationHandler))
                }), JwtBearerDefaults.AuthenticationScheme));
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail(ex.Message);
            }
        }

        private IEnumerable<Claim> ToClaims(IReadOnlyDictionary<string, object> claims)
        {
            return new List<Claim>
            {
               new Claim("id", claims["user_id"].ToString()),
               new Claim("email", claims["email"].ToString()),
               new Claim("name", claims["name"].ToString()),

            };
        }
    }
}
