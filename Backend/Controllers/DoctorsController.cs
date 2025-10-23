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
    public class DoctorsController : ControllerBase
    {
        private readonly DataContext _db;
        public DoctorsController(DataContext db) => _db = db;

        /// <summary>
        /// Retrieves all doctors.
        /// </summary>
        /// <remarks>
        /// Returns a list of doctors including their clinic and speciality names.
        /// </remarks>
        /// <response code="200">Returns the list of doctors</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DoctorReadDto>>> GetAll()
        {
            var items = await _db.Doctors
                .Include(d => d.Clinic)
                .Include(d => d.Speciality)
                .AsNoTracking()
                .Select(d => new DoctorReadDto
                {
                    Id = d.Id,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    ClinicId = d.ClinicId,
                    ClinicName = d.Clinic.Name,
                    SpecialityId = d.SpecialityId,
                    SpecialityName = d.Speciality.Name
                })
                .ToListAsync();

            return Ok(items);
        }

        /// <summary>
        /// Retrieves a doctor by ID.
        /// </summary>
        /// <param name="id">The ID of the doctor.</param>
        /// <response code="200">Returns the requested doctor</response>
        /// <response code="404">If the doctor is not found</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DoctorReadDto>> Get(int id)
        {
            var d = await _db.Doctors
                .Include(x => x.Clinic)
                .Include(x => x.Speciality)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (d is null) return NotFound();

            var dto = new DoctorReadDto
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName,
                ClinicId = d.ClinicId,
                ClinicName = d.Clinic.Name,
                SpecialityId = d.SpecialityId,
                SpecialityName = d.Speciality.Name
            };
            return Ok(dto);
        }

        /// <summary>
        /// Creates a new doctor.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///         "firstName": "Meredith",
        ///         "lastName": "Grey",
        ///         "clinicId": 1,
        ///         "specialityId": 2
        ///     }
        ///
        /// Notes:
        /// - <c>ClinicId</c> and <c>SpecialityId</c> must exist in the database.
        /// - The combination (FirstName, LastName, ClinicId, SpecialityId) must be unique.
        /// </remarks>
        /// <param name="dto">The doctor data to create.</param>
        /// <response code="201">Returns the newly created doctor</response>
        /// <response code="400">If ClinicId or SpecialityId are invalid</response>
        /// <response code="409">If a duplicate doctor exists</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<DoctorReadDto>> Create(DoctorCreateDto dto)
        {
            var clinicExists = await _db.Clinics.AnyAsync(c => c.Id == dto.ClinicId);
            var specExists   = await _db.Specialities.AnyAsync(s => s.Id == dto.SpecialityId);
            if (!clinicExists || !specExists)
                return BadRequest("Invalid ClinicId or SpecialityId.");

            var dup = await _db.Doctors.AnyAsync(d =>
                d.FirstName == dto.FirstName &&
                d.LastName == dto.LastName &&
                d.ClinicId == dto.ClinicId &&
                d.SpecialityId == dto.SpecialityId);

            if (dup) return Conflict("Duplicate doctor at the same clinic/speciality.");

            var entity = new Doctor
            {
                FirstName    = dto.FirstName,
                LastName     = dto.LastName,
                ClinicId     = dto.ClinicId,
                SpecialityId = dto.SpecialityId
            };

            _db.Doctors.Add(entity);
            await _db.SaveChangesAsync();

            var read = await _db.Doctors
                .Include(d => d.Clinic)
                .Include(d => d.Speciality)
                .Where(d => d.Id == entity.Id)
                .Select(d => new DoctorReadDto
                {
                    Id = d.Id,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    ClinicId = d.ClinicId,
                    ClinicName = d.Clinic.Name,
                    SpecialityId = d.SpecialityId,
                    SpecialityName = d.Speciality.Name
                })
                .FirstAsync();

            return CreatedAtAction(nameof(Get), new { id = entity.Id }, read);
        }

        /// <summary>
        /// Updates an existing doctor.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///         "id": 5,
        ///         "firstName": "Meredith",
        ///         "lastName": "Grey",
        ///         "clinicId": 1,
        ///         "specialityId": 3
        ///     }
        ///
        /// Notes:
        /// - The <c>id</c> in the route must match the <c>id</c> in the body.
        /// - The combination (FirstName, LastName, ClinicId, SpecialityId) must remain unique.
        /// </remarks>
        /// <param name="id">The ID of the doctor to update.</param>
        /// <param name="dto">The updated doctor data.</param>
        /// <response code="204">Doctor updated successfully</response>
        /// <response code="400">If route ID does not match body ID</response>
        /// <response code="404">If the doctor is not found</response>
        /// <response code="409">If the update would create a duplicate doctor</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, DoctorUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest();

            var entity = await _db.Doctors.FindAsync(id);
            if (entity is null) return NotFound();

            var dup = await _db.Doctors.AnyAsync(d =>
                d.Id != id &&
                d.FirstName == dto.FirstName &&
                d.LastName == dto.LastName &&
                d.ClinicId == dto.ClinicId &&
                d.SpecialityId == dto.SpecialityId);

            if (dup) return Conflict("Duplicate doctor at the same clinic/speciality.");

            entity.FirstName     = dto.FirstName;
            entity.LastName      = dto.LastName;
            entity.ClinicId      = dto.ClinicId;
            entity.SpecialityId  = dto.SpecialityId;

            _db.Entry(entity).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Deletes a doctor by ID.
        /// </summary>
        /// <param name="id">The ID of the doctor to delete.</param>
        /// <response code="204">Doctor deleted successfully</response>
        /// <response code="404">If the doctor is not found</response>
        /// <response code="409">If the doctor has existing appointments</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Doctors
                .Include(d => d.Appointments)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (item is null) return NotFound();
            if (item.Appointments.Any())
                return Conflict("Cannot delete doctor with existing appointments.");

            _db.Doctors.Remove(item);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Searches doctors by first or last name.
        /// </summary>
        /// <remarks>
        /// Returns a list of objects with:
        /// - <c>doctorFullName</c>
        /// - <c>clinicName</c>
        /// - <c>specialityName</c>
        ///
        /// Example:
        ///
        ///     GET /api/doctors/search?q=grey
        ///
        /// </remarks>
        /// <param name="q">First name or last name (partial allowed).</param>
        /// <response code="200">Returns matching doctors with clinic and speciality info</response>
        /// <response code="404">If no doctors match the query</response>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return NotFound("No doctors found.");

            var query = q.Trim();
            var results = await _db.Doctors
                .Include(d => d.Clinic)
                .Include(d => d.Speciality)
                .Where(d => EF.Functions.Like(d.FirstName, $"%{query}%") ||
                            EF.Functions.Like(d.LastName,  $"%{query}%"))
                .Select(d => new
                {
                    doctorFullName = d.FirstName + " " + d.LastName,
                    clinicName = d.Clinic.Name,
                    specialityName = d.Speciality.Name
                })
                .ToListAsync();

            if (results.Count == 0) return NotFound("No doctors found.");
            return Ok(results);
        }
    }
}

