using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IncidentApplication.Models
{
    public class Task_Detail
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Column("Task_Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public IncidentStatus Status { get; set; }
        public string Reason { get; set; }
        [Required]
        public int IncidentId { get; set; }
        [Required]
        [ForeignKey("IncidentId")]
        public Incidents Incident { get; set; }
        [Required]
        public string TechnicianId { get; set; }
        [ForeignKey("TechnicianId")]
        public User Technician { get; set; }
    }
}
