using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IncidentApplication.Data;
using IncidentApplication.Models;
using Microsoft.AspNetCore.Authorization;
using IncidentApplication.Models.Responses;

namespace IncidentApplication.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public IncidentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Incidents
        [Authorize]
        [HttpGet]
        public IEnumerable<Incidents> GetIncident()
        {
            return _context.Incidents
                .Include(incident => incident.IncidentStatus)
                .Include(incident => incident.User)
                .ThenInclude(user => user.User_Roles)
                .Include(incident => incident.Technician)
                .ThenInclude(user => user.User_Roles)
                .AsEnumerable();
        }

        // GET: api/Incidents/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetIncident([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var incident = await _context.Incidents
                .Include(inc => inc.IncidentStatus)
                .Include(inc => inc.User)
                 .ThenInclude(user => user.User_Roles)
                .Include(inc => inc.Technician)
                 .ThenInclude(user => user.User_Roles)
                .SingleOrDefaultAsync(inc => inc.Id == id);

            if (incident == null)
            {
                return NotFound();
            }

            return Ok(incident);
        }

        // GET: api/Incidents/user/5
        [Authorize(Roles = "Manager, User")]
        [HttpGet]
        [Route("user/{id}")]
        public IActionResult GetIncidentsFromUser([FromRoute] string id)
        {
            IEnumerable<Incidents> incidents = _context.Incidents
                .Include(incident => incident.IncidentStatus)
                .Include(incident => incident.User)
                .ThenInclude(user => user.User_Roles)
                .Include(incident => incident.Technician)
                .ThenInclude(user => user.User_Roles)
                .Where(incident => incident.UserId == id)
                .AsEnumerable();

            if (incidents.Count() == 0)
            {
                return NotFound();
            }

            return Ok(incidents);
        }

        // GET: api/Incidents/tech/5
        [Authorize(Roles = "Manager, Technician")]
        [HttpGet]
        [Route("tech/{id}")]
        public IActionResult GetIncidentsFromTech([FromRoute] string id)
        {
            IEnumerable<Incidents> incidents = _context.Incidents
                .Include(incident => incident.IncidentStatus)
                .Include(incident => incident.User)
                .ThenInclude(user => user.User_Roles)
                .Include(incident => incident.Technician)
                .ThenInclude(user => user.User_Roles)
                .Where(incident => incident.TechnicianId == id)
                .AsEnumerable();

            if (incidents.Count() == 0)
            {
                return NotFound();
            }

            return Ok(incidents);
        }

        // PUT: api/Incidents/5
        [Authorize(Roles = "User")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIncident([FromRoute] int id, [FromBody] Incidents incident)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != incident.Id)
            {
                return BadRequest();
            }

            _context.Entry(incident).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IncidentExists(id))
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

        // PUT: api/Incidents/man/assign/5/tech/105
        [Authorize(Roles = "Manager")]
        [HttpPut]
        [Route("man/assign/{inc_id}/tech/{tech_id}")]
        public async Task<IActionResult> AssignTechToIncident([FromRoute] int inc_id, [FromRoute] string tech_id, [FromBody] Incidents incident)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (inc_id != incident.Id || tech_id != incident.TechnicianId)
            {
                return BadRequest();
            }

            if (!TechnicianExists(tech_id))
            {
                return NotFound(new ErrorResponse { Code = 1, Message = "Technician does not exist"});
            }

            _context.Entry(incident).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return NoContent();
        }

        // PUT: api/Incidents/tech/acceptreject/5/true/reason for rejection
        [Authorize(Roles = "Technician")]
        [HttpPut]
        [Route("tech/acceptreject/{id}/{acceptReject}/{reason}")]
        public async Task<IActionResult> AcceptRejectIncident([FromRoute] int id, [FromRoute] Boolean acceptReject, [FromRoute] String reason, [FromBody] Incidents incident)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != incident.Id || incident.TechnicianId == null)
            {
                return BadRequest();
            }

            if (acceptReject)
            {
                incident.StatusId = 1;
            } else
            {
                incident.StatusId = 0;
            }

            _context.Entry(incident).State = EntityState.Modified;

            Task_Detail task_Detail = new Task_Detail();

            if (acceptReject)
            {
                task_Detail.StatusId = 4;
            }
            else
            {
                task_Detail.StatusId = 5;
                task_Detail.Reason = reason;
            }
            task_Detail.Id = id;
            task_Detail.TechnicianId = incident.TechnicianId;

            _context.TaskDetail.Add(task_Detail);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return NoContent();
        }

        // PUT: api/Incidents/tech/close/5
        [Authorize(Roles = "Technician")]
        [HttpPut]
        [Route("tech/close/{id}")]
        public async Task<IActionResult> CloseIncident([FromRoute] int id,[FromBody] Incidents incident)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != incident.Id || incident.TechnicianId == null || incident.StatusId != 1)
            {
                return BadRequest();
            }

            incident.StatusId = 2;

            _context.Entry(incident).State = EntityState.Modified;

            Task_Detail task_Detail = new Task_Detail
            {
                StatusId = 2,
                IncidentId = id,
                TechnicianId = incident.TechnicianId
            };

            _context.TaskDetail.Add(task_Detail);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return NoContent();
        }

        // POST: api/Incidents
        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> PostIncident([FromBody] Incidents incident)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Incidents.Add(incident);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetIncident", new { id = incident.Id }, incident);
        }

        private bool IncidentExists(int id)
        {
            return _context.Incidents.Any(e => e.Id == id);
        }
        private bool TechnicianExists(string id)
        {
            return _context.User.Any(e => e.Id == id && e.RoleId == 2);
        }
    }
}