using ClinicExam.Data;
using ClinicExam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicExam.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ClinicsController : ControllerBase
    {
        private readonly DataContext _db;
        public ClinicsController(DataContext db) => _db = db;

        /// <summary>Retrieves all clinics.</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Clinic>>> GetAll() =>
            await _db.Clinics.AsNoTracking().ToListAsync();

        /// <summary>Retrieves a clinic by ID.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Clinic>> Get(int id)
        {
            var item = await _db.Clinics.FindAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        /// <summary>Creates a clinic.</summary>
        /// <remarks>
        /// Clinic name must be unique.
        /// 
        /// Sample request:
        ///
        ///     {
        ///       "name": "clinic name",
        ///       "address: "123 street"
        ///     }
        ///
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Clinic>> Create(Clinic item)
        {
            if (await _db.Clinics.AnyAsync(c => c.Name == item.Name))
                return Conflict("Clinic name already exists.");

            _db.Clinics.Add(item);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        /// <summary>Updates a clinic.</summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///       "name": "clinic name",
        ///       "address: "123 street"
        ///     }
        ///
        /// </remarks>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, Clinic item)
        {
            if (id != item.Id) return BadRequest();
            _db.Entry(item).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Deletes a clinic (blocked if doctors or appointments exist).</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Clinics
                .Include(c => c.Doctors)
                .Include(c => c.Appointments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (item is null) return NotFound();

            if (item.Doctors.Any() || item.Appointments.Any())
                return Conflict("Cannot delete clinic with existing doctors or appointments.");

            _db.Clinics.Remove(item);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}