using ClinicExam.Data;
using ClinicExam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicExam.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CategoriesController : ControllerBase
    {
        private readonly DataContext _db;
        public CategoriesController(DataContext db) => _db = db;

        /// <summary>Retrieves all categories.</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Category>>> GetAll() =>
            await _db.Categories.AsNoTracking().ToListAsync();

        /// <summary>Retrieves a category by ID.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Category>> Get(int id)
        {
            var item = await _db.Categories.FindAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        /// <summary>Creates a category.</summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///       "name": "category name"
        ///     }
        ///
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Category>> Create(Category item)
        {
            if (await _db.Categories.AnyAsync(c => c.Name == item.Name))
                return Conflict("Category name already exists.");

            _db.Categories.Add(item);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        /// <summary>Updates a category.</summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///       "name": "category name"
        ///     }
        ///
        /// </remarks>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, Category item)
        {
            if (id != item.Id) return BadRequest();
            _db.Entry(item).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Deletes a category (blocked if appointments exist).</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Categories
                .Include(c => c.Appointments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (item is null) return NotFound();
            if (item.Appointments.Any())
                return Conflict("Cannot delete category used by appointments.");

            _db.Categories.Remove(item);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}