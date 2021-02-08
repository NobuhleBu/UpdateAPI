using IncidentApplication.Data;
using IncidentApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using IncidentApplication.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using IncidentApplication.Models.Responses;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using IncidentApplication.Services;
using System.Text.Encodings.Web;
using IdentityModel;

namespace IncidentApplication.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IConfiguration config, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _config = config;
            _emailSender = emailSender;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ApplicationUser user = await RegisterUser(request);

            if (user == null)
            {
                return BadRequest(new ErrorResponse { Code = 1, Message = "Account failed to be registered" });
            }
            else
            {
                Task task = await GenerateConfirmationEmailAsync(user);
                await task;
                return Ok(new SuccessResponse { Code = 1, Message = "Account registered successfully" });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ApplicationUser user = await AuthenticateUser(request);

            if (user == null)
            {
                return Unauthorized(new ErrorResponse { Code = 1, Message = "Wrong Credentials or account does not exist" });
            } else
            {
                var accessToken = GenerateJWT(user);
                var refreshToken = GenerateRefreshToken();
                await AddRefreshTokenAsync(refreshToken, user.Id, "", "", "", "");
                return Ok(new AuthenticateResponse(accessToken, refreshToken, user.Id));
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("confirm-email/{id}/{code}")]
        public async Task<IActionResult> ConfirmEmailAsync([FromRoute] string id, [FromRoute] string code)
        {
            if(id == null || code == null)
            {
                return BadRequest();
            }

            //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            string token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            ApplicationUser user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return BadRequest();
            }

            var emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            if (!emailConfirmed)
            {
                var results = await _userManager.ConfirmEmailAsync(user, token);

                if (results.Succeeded)
                {
                    return Ok(new SuccessResponse { Code = 1, Message = "Email confirmed" });
                } else
                {
                    return BadRequest(new ErrorResponse { Code = 1, Message = "Token is invalid or has expired" });
                }
            } else
            {
                return Ok(new SuccessResponse { Code = 1, Message = "Email already confirmed" });
            }

        }

        [Authorize]
        [HttpPost]
        [Route("sign-out")]
        public async Task<IActionResult> SignOut([FromBody] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _signInManager.SignOutAsync();

            return Ok("Signed out successfully");
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> RefreshAccessToken([FromBody] RefreshAccessTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            RefreshToken refreshToken = await _context.RefreshTokens
                                .SingleOrDefaultAsync(token => token.Token == request.RefreshToken);
            
            if (refreshToken == null)
            {
                return NotFound(new ErrorResponse { Code = 5, Message = "Token not found" });
            } else
            {
                if (refreshToken.UserId != request.Id)
                {
                    return BadRequest(new ErrorResponse { Code = 4, Message = "User not matching" });
                } else
                {
                    if (refreshToken.ExpiryDate <= DateTime.Now)
                    {
                        return Unauthorized(new ErrorResponse { Code = 2, Message = "Refresh token expired" });
                    } else
                    {

                        ApplicationUser user = await _context.Users
                                                .SingleOrDefaultAsync(u => u.Id == request.Id);

                        if (user == null)
                        {
                            return BadRequest(new ErrorResponse { Code = 5, Message = "Could not find user" });
                        }
                        else if (refreshToken.UsedCount != 0)
                        {
                            refreshToken.UsedCount += 1;
                            _context.Entry(refreshToken).State = EntityState.Modified;
                            await _context.SaveChangesAsync();

                            return Unauthorized(new ErrorResponse { Code = 3, Message = "Refresh token might be compromised" });
                        }
                        else
                        {
                            refreshToken.UsedCount += 1;
                            _context.Entry(refreshToken).State = EntityState.Modified;
                            await _context.SaveChangesAsync();

                            var newAccessToken = GenerateJWT(user);
                            var newRefreshToken = GenerateRefreshToken();
                            await AddRefreshTokenAsync(newRefreshToken, user.Id, "", "", "", "");
                            return Ok(new AuthenticateResponse(newAccessToken, newRefreshToken, user.Id));
                        }

                    }
                }
            }

            
            
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfile([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            User user = await _context.User
                                .Include(u => u.User_Roles)
                                .SingleOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new ErrorResponse { Code = 3, Message = "Failed to find user" });
            }
            else
            {
                return Ok(user);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("send-reset-password")]
        public async Task<IActionResult> SendResetPasswordLinkAsync([FromBody] string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ApplicationUser user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return NotFound(new ErrorResponse { Code = 1, Message = "Account not found" });
            }else
            {
                Task task = GenerateResetPasswordEmailAsync(user);
                await task;
                return Ok(new SuccessResponse { Code = 1, Message = "Reset password email is sent" });
            }

        }

        [AllowAnonymous]
        [HttpPost]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ApplicationUser user = await _userManager.FindByIdAsync(request.Id);

            if (user == null)
            {
                return NotFound(new ErrorResponse { Code = 1, Message = "Account not found" });
            }
            else
            {
                string token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
                IdentityResult identityResult = await _userManager.ResetPasswordAsync(user, token, request.Password);

                if (identityResult.Succeeded)
                {
                    return Ok(new SuccessResponse { Code = 1, Message = "Password has been changed successfully" });
                } else
                {
                    return BadRequest(new ErrorResponse { Code = 1, Message = "Token is invalid or has expired" });
                }

            }

        }

        private async Task<ApplicationUser> RegisterUser(RegisterRequest request)
        {
            string role = request.RoleId switch
            {
                0 => "Manager",
                1 => "User",
                2 => "Technician",
                _ => "",
            };

            var user = new ApplicationUser { UserName = request.Email, Email = request.Email, FirstName = request.FirstName, LastName = request.LastName, PhoneNumber = request.PhoneNumber, Role = role };
            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                _context.User.Add(new User { Id = user.Id, FirstName = user.FirstName, LastName = user.LastName, Email = user.Email, RoleId = request.RoleId, PhoneNumber = request.PhoneNumber });
                await _context.SaveChangesAsync();

                var claims = new List<Claim>
                {
                    new Claim(JwtClaimTypes.Role, user.Role)
                };

                await _userManager.AddClaimsAsync(user, claims);
                IdentityResult identityResult = await AddRoleToUserAsync(user, role);

                if (identityResult.Succeeded)
                {
                    return user;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private async Task<IdentityResult> AddRoleToUserAsync(ApplicationUser user, string role)
        {
            IdentityResult identityResult = null;
            if (_roleManager == null)
            {
                throw new Exception("roleManager null");
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                identityResult = await _roleManager.CreateAsync(new IdentityRole(role));
            }

            identityResult = await _userManager.AddToRoleAsync(user, role);

            return identityResult;
        }

        private async Task<ApplicationUser> AuthenticateUser(AuthenticateRequest request)
        {
            //var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user != null)
            {
                var result = await _userManager.CheckPasswordAsync(user, request.Password);

                if (result)
                {
                    return user;
                }
            }

            return null;
        }

        private string GenerateJWT(ApplicationUser user)
        {

            var securityKey = new SymmetricSecurityKey(Base64UrlEncoder.DecodeBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtClaimTypes.Id, user.Id),
                new Claim(JwtClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private async Task AddRefreshTokenAsync(string token, string userId,string ip, string app, string device, string instanceId, double daysToExpire=15)
        {
            _context.RefreshTokens.Add(
                new RefreshToken { Token = token, 
                                    UserId = userId, 
                                    IPAddress = ip, 
                                    Application = app, 
                                    Device = device, 
                                    InstanceId = instanceId , 
                                    ExpiryDate = DateTime.Now.AddMinutes(daysToExpire),
                                    UsedCount = 0 });
            await _context.SaveChangesAsync();
        }

        private async Task<Task> GenerateConfirmationEmailAsync(ApplicationUser user)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            string EmailConfirmationUrl = "https://localhost:5001/api/Auth/confirm-email/" + user.Id + "/" + code;
            Console.WriteLine(EmailConfirmationUrl);
            return _emailSender.SendConfirmEmailAsync(user, user.Email, EmailConfirmationUrl);
        }

        private async Task<Task> GenerateResetPasswordEmailAsync(ApplicationUser user)
        {
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            string ResetPasswordUrl = "https://localhost:5001/auth/reset-password/" + user.Id + "/" + code;
            Console.WriteLine(ResetPasswordUrl);
            return _emailSender.SendRestPasswordEmailAsync(user, user.Email, ResetPasswordUrl);
        }
        
 
    }
}
