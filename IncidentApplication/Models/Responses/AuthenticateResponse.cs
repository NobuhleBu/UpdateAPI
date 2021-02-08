using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IncidentApplication.Models.Responses
{
    public class AuthenticateResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Id { get; set; }

        public AuthenticateResponse(string a_token, string r_token, string id)
        {
            AccessToken = a_token;
            RefreshToken = r_token;
            this.Id = id;
        }
    }
}
