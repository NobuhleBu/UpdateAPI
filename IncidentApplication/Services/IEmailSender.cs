using IncidentApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IncidentApplication.Services
{
    public interface IEmailSender
    {
        public Task SendConfirmEmailAsync(ApplicationUser user, string email, string url);
        public Task SendRestPasswordEmailAsync(ApplicationUser user, string email, string url);
    }
}
