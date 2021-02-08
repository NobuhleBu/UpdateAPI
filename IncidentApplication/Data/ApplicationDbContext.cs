using IncidentApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IncidentApplication.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<IncidentApplication.Models.User> User { get; set; }
        public DbSet<IncidentApplication.Models.UserRoles> User_Roles { get; set; }
        public DbSet<IncidentApplication.Models.Incidents> Incidents { get; set; }
        public DbSet<IncidentApplication.Models.IncidentStatus> IncidentStatus { get; set; }
        public DbSet<IncidentApplication.Models.Task_Detail> TaskDetail { get; set; }
        public DbSet<IncidentApplication.Models.RefreshToken> RefreshTokens { get; set; }

        internal Task<IdentityResult> ChangePasswordAsync(string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }
    }
}
