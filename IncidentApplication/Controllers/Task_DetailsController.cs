using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IncidentApplication.Data;
using IncidentApplication.Models;

namespace IncidentApplication.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class Task_DetailsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public Task_DetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TaskDetail
        [HttpGet]
        public IEnumerable<Task_Detail> GetTask_Details()
        {
            return _context.TaskDetail
                .Include(task => task.Status)
                .Include(task => task.Incident)
                .Include(task => task.Technician)
                .AsEnumerable(); ;
        }

        // GET: api/TaskDetail/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask_Details([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var task_Details = await _context.TaskDetail
                .Include(task => task.Status)
                .Include(task => task.Incident)
                .Include(task => task.Technician)
                .SingleOrDefaultAsync(task => task.Id == id);

            if (task_Details == null)
            {
                return NotFound();
            }

            return Ok(task_Details);
        }

        [HttpGet]
        [Route("incident/{id}")]
        public IEnumerable<Task_Detail> GetTaskDetaiFromIncident([FromRoute] int id)
        {
            return _context.TaskDetail
                .Include(task => task.Status)
                .Include(task => task.Incident)
                .Include(task => task.Technician)
                .Where(task => task.IncidentId == id)
                .AsEnumerable();
        }

        // PUT: api/TaskDetail/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTask_Details([FromRoute] int id, [FromBody] Task_Detail task_Details)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != task_Details.Id)
            {
                return BadRequest();
            }

            _context.Entry(task_Details).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Task_DetailsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/TaskDetail
        [HttpPost]
        public async Task<IActionResult> PostTask_Details([FromBody] Task_Detail task_Details)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.TaskDetail.Add(task_Details);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTask_Details", new { id = task_Details.Id }, task_Details);
        }

        // DELETE: api/TaskDetail/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask_Details([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var task_Details = await _context.TaskDetail.FindAsync(id);
            if (task_Details == null)
            {
                return NotFound();
            }

            _context.TaskDetail.Remove(task_Details);
            await _context.SaveChangesAsync();

            return Ok(task_Details);
        }

        private bool Task_DetailsExists(int id)
        {
            return _context.TaskDetail.Any(e => e.Id == id);
        }
    }
}