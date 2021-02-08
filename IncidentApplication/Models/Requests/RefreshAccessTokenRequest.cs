using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IncidentApplication.Models.Requests
{
    public class RefreshAccessTokenRequest
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
