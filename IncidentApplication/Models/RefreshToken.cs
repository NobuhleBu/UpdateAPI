using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IncidentApplication.Models
{
    [Table("RefreshTokens")]
    public class RefreshToken
    {
        [Key]
        public string Token { get; set; }
        public string UserId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string IPAddress { get; set; }
        public string Application { get; set; }
        public string Device { get; set; }
        public string InstanceId { get; set; }
        public int UsedCount { get; set; }

    }
}
