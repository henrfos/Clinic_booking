using ClinicExam.Data;
using ClinicExam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicExam.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SpecialitiesController : ControllerBase
    {
        private readonly DataContext _db;
        public SpecialitiesController(DataContext db) => _db = db;

        /// <summary>Retrieves all specialities.</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Speciality>>> GetAll() =>
            await _db.Specialities.AsNoTracking().ToListAsync();

        /// <summary>Retrieves a speciality by ID.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Speciality>> Get(int id)
        {
            var item = await _db.Specialities.FindAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        /// <summary>Creates a speciality.</summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///       "name": string
        ///     }
        ///
        /// Notes:
        /// - Name must be unique.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Speciality>> Create(Speciality item)
        {
            if (await _db.Specialities.AnyAsync(s => s.Name == item.Name))
                return Conflict("Speciality name already exists.");

            _db.Specialities.Add(item);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        /// <summary>Updates a speciality.</summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///       "name": string
        ///     }
        ///
        /// Notes:
        /// - Name must be unique.
        /// </remarks>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, Speciality item)
        {
            if (id != item.Id) return BadRequest();
            _db.Entry(item).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Deletes a speciality (blocked if doctors exist).</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Specialities
                .Include(s => s.Doctors)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item is null) return NotFound();
            if (item.Doctors.Any())
                return Conflict("Cannot delete speciality with assigned doctors.");

            _db.Specialities.Remove(item);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}