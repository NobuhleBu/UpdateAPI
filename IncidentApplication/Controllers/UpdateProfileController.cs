using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IncidentApplication.Data;
using IncidentApplication.Models;
using IncidentApplication.Models.Responses;
using Microsoft.AspNetCore.Identity;
using System.Security.Principal;

namespace IncidentApplication.Controllers
{
    public class UpdateProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ChangePasswordBindingModel> _userManager;

        public UpdateProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UpdateProfile
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.User.Include(u => u.User_Roles);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: UpdateProfile/Details/5
        public async Task<IActionResult> Details(string id)
        {
            //if (id == null)
            //{
            //    return NotFound();
            //}

            //var user = await _context.User
            //    .Include(u => u.User_Roles)
            //    .FirstOrDefaultAsync(m => m.Id == id);
            //if (user == null)
            //{
            //    return NotFound();
            //}

            //return View(user);
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

     
        // GET: UpdateProfile/Create
        public IActionResult Create()
        {
            ViewData["RoleId"] = new SelectList(_context.User_Roles, "Id", "Description");
            return View();
        }

        // POST: UpdateProfile/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Email,PhoneNumber,RoleId")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new SelectList(_context.User_Roles, "Id", "Description", user.RoleId);
            return View(user);
        }

        // GET: UpdateProfile/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewData["RoleId"] = new SelectList(_context.User_Roles, "Id", "Description", user.RoleId);
            return View(user);
        }


        // POST: UpdateProfile/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,FirstName,LastName,Email,PhoneNumber,RoleId")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new SelectList(_context.User_Roles, "Id", "Description", user.RoleId);
            return View(user);
        }

       /* [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


           // IdentityResult result = await this._userManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);


            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }*/




        // GET: UpdateProfile/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .Include(u => u.User_Roles)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }


        // POST: UpdateProfile/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _context.User.FindAsync(id);
            _context.User.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(string id)
        {
            return _context.User.Any(e => e.Id == id);
        }


    }
}
