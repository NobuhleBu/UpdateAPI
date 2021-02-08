using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IncidentApplication.Models
{
    public class Incidents
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Location { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public DateTime Date_Logged { get; set; }
        [Required]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public IncidentStatus IncidentStatus { get; set; }
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        public string? TechnicianId { get; set; }
        [ForeignKey("TechnicianId")]
        public User Technician { get; set; }


    }
}
