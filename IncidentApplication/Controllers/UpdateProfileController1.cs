using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IncidentApplication.Data;
using IncidentApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IncidentApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateProfileController1 : ControllerBase
    { 
        private UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UpdateProfileController1(ApplicationDbContext context)
        {
            _context = context;
            return;
        }

        public UpdateProfileController1(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;

        }
      
        [HttpPost]
        [Route("update-password")]
        public async Task<IActionResult>ChangePassword([FromRoute] string id, [FromBody]ChangePasswordBindingModel model)
        {
            /// var user = await _userManager.FindByIdAsync(id);
            /// Get the current logged in user
            var user = await _userManager.GetUserAsync(HttpContext.User);
           // var password = await _userManager.CheckPasswordAsync(user, model.OldPassword);

            if(user == null)
            {
                
            };

            //Attempt to change password
            var password = await _userManager.ChangePasswordAsync(user, model.NewPassword, model.OldPassword);

            if(password.Succeeded)
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Password matches" });
            }else
            {
                return  BadRequest(password);
            }

            //if(! await _userManager.CheckPasswordAsync(user, model.OldPassword))
            //    {

            //    return BadRequest("Please enter the correct old password");
            //}else{
            //    return Ok(new{message ="Password matches"});
            //}
        }
    }
}
