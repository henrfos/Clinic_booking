using ClinicExam.Data;
using ClinicExam.Models;
using ClinicExam.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicExam.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PatientsController : ControllerBase
    {
        private readonly DataContext _db;
        public PatientsController(DataContext db) => _db = db;

        /// <summary>
        /// Retrieves all patients.
        /// </summary>
        /// <remarks>
        /// Returns basic, non-sensitive PII only (FirstName, LastName, Email, BirthDate).
        /// </remarks>
        /// <response code="200">Returns the list of patients</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PatientReadDto>>> GetAll()
        {
            var items = await _db.Patients
                .AsNoTracking()
                .Select(p => new PatientReadDto
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName  = p.LastName,
                    Email = p.Email,
                    BirthDate = p.BirthDate
                })
                .ToListAsync();

            return Ok(items);
        }

        /// <summary>
        /// Retrieves a patient by ID.
        /// </summary>
        /// <param name="id">The ID of the patient.</param>
        /// <response code="200">Returns the requested patient</response>
        /// <response code="404">If the patient is not found</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PatientReadDto>> Get(int id)
        {
            var p = await _db.Patients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (p is null) return NotFound();

            return Ok(new PatientReadDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName  = p.LastName,
                Email = p.Email,
                BirthDate = p.BirthDate
            });
        }

        /// <summary>
        /// Retrieves a patient by email (case-insensitive).
        /// </summary>
        /// <remarks>
        /// Example:
        ///
        ///     GET /api/patients/by-email?email=jane.doe%40example.com
        ///
        /// </remarks>
        /// <param name="email">Patient email address.</param>
        /// <response code="200">Returns the matching patient</response>
        /// <response code="404">If no patient with the email exists</response>
        [HttpGet("by-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PatientReadDto>> GetByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return NotFound();

            var normalized = email.Trim().ToLowerInvariant();
            var p = await _db.Patients.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Email.ToLower() == normalized);

            if (p is null) return NotFound();

            return Ok(new PatientReadDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName  = p.LastName,
                Email = p.Email,
                BirthDate = p.BirthDate
            });
        }

        /// <summary>
        /// Creates a new patient.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///       "firstName": "Jane",
        ///       "lastName": "Doe",
        ///       "email": "jane.doe@example.com",
        ///       "birthDate": "1997-04-12"
        ///     }
        ///
        /// Notes:
        /// - Email is treated case-insensitively and stored in lowercase.
        /// - Email must be unique.
        /// </remarks>
        /// <param name="dto">The patient data to create.</param>
        /// <response code="201">Returns the newly created patient</response>
        /// <response code="400">If the body is invalid</response>
        /// <response code="409">If a patient with the email already exists</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<PatientReadDto>> Create(PatientCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email is required.");

            var normalized = dto.Email.Trim().ToLowerInvariant();

            var exists = await _db.Patients.AnyAsync(p => p.Email.ToLower() == normalized);
            if (exists) return Conflict("Email already exists.");

            var entity = new Patient
            {
                FirstName = dto.FirstName,
                LastName  = dto.LastName,
                Email     = normalized,
                BirthDate = dto.BirthDate
            };

            _db.Patients.Add(entity);
            await _db.SaveChangesAsync();

            var read = new PatientReadDto
            {
                Id = entity.Id,
                FirstName = entity.FirstName,
                LastName  = entity.LastName,
                Email = entity.Email,
                BirthDate = entity.BirthDate
            };

            return CreatedAtAction(nameof(Get), new { id = entity.Id }, read);
        }

        /// <summary>
        /// Updates an existing patient.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///       "id": 4,
        ///       "firstName": "Jane",
        ///       "lastName": "Doe",
        ///       "email": "jane.doe@example.com",
        ///       "birthDate": "1997-04-12"
        ///     }
        ///
        /// Notes:
        /// - Route <c>id</c> must match body <c>id</c>.
        /// - Email is normalized to lowercase and must remain unique.
        /// </remarks>
        /// <param name="id">The patient ID.</param>
        /// <param name="dto">The updated patient data.</param>
        /// <response code="204">Patient updated successfully</response>
        /// <response code="400">If route ID mismatches or email invalid</response>
        /// <response code="404">If the patient is not found</response>
        /// <response code="409">If the new email conflicts with another patient</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, PatientUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest();
            if (string.IsNullOrWhiteSpace(dto.Email)) return BadRequest("Email is required.");

            var entity = await _db.Patients.FindAsync(id);
            if (entity is null) return NotFound();

            var normalized = dto.Email.Trim().ToLowerInvariant();

            var emailTaken = await _db.Patients.AnyAsync(p =>
                p.Id != id && p.Email.ToLower() == normalized);
            if (emailTaken) return Conflict("Email already exists.");

            entity.FirstName = dto.FirstName;
            entity.LastName  = dto.LastName;
            entity.Email     = normalized;
            entity.BirthDate = dto.BirthDate;

            _db.Entry(entity).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Deletes a patient by ID.
        /// </summary>
        /// <remarks>
        /// Deletion is blocked if the patient has existing appointments.
        /// </remarks>
        /// <param name="id">The patient ID.</param>
        /// <response code="204">Patient deleted successfully</response>
        /// <response code="404">If the patient is not found</response>
        /// <response code="409">If the patient has existing appointments</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Patients
                .Include(p => p.Appointments)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (item is null) return NotFound();

            if (item.Appointments.Any())
                return Conflict("Cannot delete patient with existing appointments.");

            _db.Patients.Remove(item);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}