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
    public class AppointmentsController : ControllerBase
    {
        private readonly DataContext _db;
        public AppointmentsController(DataContext db) => _db = db;

        /// <summary>
        /// Retrieves all appointments.
        /// </summary>
        /// <remarks>
        /// Returns patient/doctor/clinic/category names with each appointment.
        /// </remarks>
        /// <response code="200">Returns the list of appointments</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AppointmentReadDto>>> GetAll()
        {
            var items = await _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor).ThenInclude(d => d.Speciality)
                .Include(a => a.Clinic)
                .Include(a => a.Category)
                .AsNoTracking()
                .Select(a => new AppointmentReadDto
                {
                    Id = a.Id,
                    PatientId = a.PatientId,
                    PatientFirstName = a.Patient.FirstName,
                    PatientLastName  = a.Patient.LastName,
                    DoctorId = a.DoctorId,
                    DoctorFirstName = a.Doctor.FirstName,
                    DoctorLastName  = a.Doctor.LastName,
                    SpecialityId = a.Doctor.SpecialityId,
                    SpecialityName = a.Doctor.Speciality.Name,
                    ClinicId = a.ClinicId,
                    ClinicName = a.Clinic.Name,
                    CategoryId = a.CategoryId,
                    CategoryName = a.Category.Name,
                    StartUtc = a.StartUtc,
                    DurationMinutes = a.DurationMinutes
                })
                .ToListAsync();

            return Ok(items);
        }

        /// <summary>
        /// Retrieves an appointment by ID.
        /// </summary>
        /// <param name="id">The appointment ID.</param>
        /// <response code="200">Returns the requested appointment</response>
        /// <response code="404">If the appointment is not found</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AppointmentReadDto>> Get(int id)
        {
            var a = await _db.Appointments
                .Include(x => x.Patient)
                .Include(x => x.Doctor).ThenInclude(d => d.Speciality)
                .Include(x => x.Clinic)
                .Include(x => x.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (a is null) return NotFound();

            var dto = new AppointmentReadDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                PatientFirstName = a.Patient.FirstName,
                PatientLastName  = a.Patient.LastName,
                DoctorId = a.DoctorId,
                DoctorFirstName = a.Doctor.FirstName,
                DoctorLastName  = a.Doctor.LastName,
                SpecialityId = a.Doctor.SpecialityId,
                SpecialityName = a.Doctor.Speciality.Name,
                ClinicId = a.ClinicId,
                ClinicName = a.Clinic.Name,
                CategoryId = a.CategoryId,
                CategoryName = a.Category.Name,
                StartUtc = a.StartUtc,
                DurationMinutes = a.DurationMinutes
            };
            return Ok(dto);
        }

        /// <summary>
        /// Creates a new appointment with clash validation.
        /// </summary>
        /// <remarks>
        /// Validations:
        /// - Patient, Doctor, Clinic, Category must exist.<br/>
        /// - Doctor must belong to the selected Clinic.<br/>
        /// - <b>No overlapping appointment</b> for the same <b>patient + clinic</b>:
        ///   <c>existing.Start &lt; new.End</c> AND <c>new.Start &lt; existing.End</c>
        ///
        /// Sample request:
        /// 
        ///     {
        ///       "patientId": 1,
        ///       "doctorId": 2,
        ///       "clinicId": 1,
        ///       "categoryId": 3,
        ///       "startUtc": "2025-09-28T09:30:00Z",
        ///       "durationMinutes": 30
        ///     }
        /// </remarks>
        /// <param name="dto">The appointment to create.</param>
        /// <response code="201">Returns the newly created appointment</response>
        /// <response code="400">If IDs are invalid, duration is non-positive, or doctor doesn't belong to clinic</response>
        /// <response code="409">If the appointment clashes with an existing one</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AppointmentReadDto>> Create(AppointmentCreateDto dto)
        {
            var patientExists = await _db.Patients.AnyAsync(p => p.Id == dto.PatientId);
            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == dto.DoctorId);
            var clinic = await _db.Clinics.FirstOrDefaultAsync(c => c.Id == dto.ClinicId);
            var categoryExists = await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId);

            if (!patientExists || doctor is null || clinic is null || !categoryExists)
                return BadRequest("Invalid PatientId, DoctorId, ClinicId, or CategoryId.");

            if (doctor.ClinicId != dto.ClinicId)
                return BadRequest("Doctor is not assigned to the selected clinic.");

            if (dto.DurationMinutes <= 0)
                return BadRequest("Duration must be positive.");

            var start = dto.StartUtc;
            var end = start.AddMinutes(dto.DurationMinutes);

            // Clash validation: same patient + same clinic overlapping time
            var overlap = await _db.Appointments.AnyAsync(a =>
                a.PatientId == dto.PatientId &&
                a.ClinicId == dto.ClinicId &&
                a.StartUtc < end &&
                start < a.StartUtc.AddMinutes(a.DurationMinutes));

            if (overlap)
                return Conflict("This patient already has an appointment at this clinic during the selected time.");

            var entity = new Appointment
            {
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                ClinicId = dto.ClinicId,
                CategoryId = dto.CategoryId,
                StartUtc = dto.StartUtc,
                DurationMinutes = dto.DurationMinutes
            };

            _db.Appointments.Add(entity);
            await _db.SaveChangesAsync();

            var read = await _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor).ThenInclude(d => d.Speciality)
                .Include(a => a.Clinic)
                .Include(a => a.Category)
                .Where(a => a.Id == entity.Id)
                .Select(a => new AppointmentReadDto
                {
                    Id = a.Id,
                    PatientId = a.PatientId,
                    PatientFirstName = a.Patient.FirstName,
                    PatientLastName  = a.Patient.LastName,
                    DoctorId = a.DoctorId,
                    DoctorFirstName = a.Doctor.FirstName,
                    DoctorLastName  = a.Doctor.LastName,
                    SpecialityId = a.Doctor.SpecialityId,
                    SpecialityName = a.Doctor.Speciality.Name,
                    ClinicId = a.ClinicId,
                    ClinicName = a.Clinic.Name,
                    CategoryId = a.CategoryId,
                    CategoryName = a.Category.Name,
                    StartUtc = a.StartUtc,
                    DurationMinutes = a.DurationMinutes
                })
                .FirstAsync();

            return CreatedAtAction(nameof(Get), new { id = entity.Id }, read);
        }

        /// <summary>
        /// Updates an existing appointment (re-validates for clashes).
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///       "id": 10,
        ///       "patientId": 1,
        ///       "doctorId": 2,
        ///       "clinicId": 1,
        ///       "categoryId": 4,
        ///       "startUtc": "2025-09-28T10:00:00Z",
        ///       "durationMinutes": 45
        ///     }
        ///
        /// Notes:
        /// - Route <c>id</c> must match body <c>id</c>.<br/>
        /// - Doctor must belong to clinic; overlap rule is re-checked.
        /// </remarks>
        /// <param name="id">The appointment ID.</param>
        /// <param name="dto">The updated appointment data.</param>
        /// <response code="204">Appointment updated successfully</response>
        /// <response code="400">If route ID mismatches or data invalid</response>
        /// <response code="404">If the appointment is not found</response>
        /// <response code="409">If the update would create a clash</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, AppointmentUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest();

            var entity = await _db.Appointments.FindAsync(id);
            if (entity is null) return NotFound();

            // FK checks & doctor-clinic consistency
            var patientExists = await _db.Patients.AnyAsync(p => p.Id == dto.PatientId);
            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == dto.DoctorId);
            var clinic = await _db.Clinics.FirstOrDefaultAsync(c => c.Id == dto.ClinicId);
            var categoryExists = await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId);

            if (!patientExists || doctor is null || clinic is null || !categoryExists)
                return BadRequest("Invalid PatientId, DoctorId, ClinicId, or CategoryId.");

            if (doctor.ClinicId != dto.ClinicId)
                return BadRequest("Doctor is not assigned to the selected clinic.");

            if (dto.DurationMinutes <= 0)
                return BadRequest("Duration must be positive.");

            var start = dto.StartUtc;
            var end = start.AddMinutes(dto.DurationMinutes);

            var overlap = await _db.Appointments.AnyAsync(a =>
                a.Id != dto.Id &&
                a.PatientId == dto.PatientId &&
                a.ClinicId == dto.ClinicId &&
                a.StartUtc < end &&
                start < a.StartUtc.AddMinutes(a.DurationMinutes));

            if (overlap)
                return Conflict("This patient already has an appointment at this clinic during the selected time.");

            // Apply updates
            entity.PatientId = dto.PatientId;
            entity.DoctorId = dto.DoctorId;
            entity.ClinicId = dto.ClinicId;
            entity.CategoryId = dto.CategoryId;
            entity.StartUtc = dto.StartUtc;
            entity.DurationMinutes = dto.DurationMinutes;

            _db.Entry(entity).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Deletes an appointment by ID.
        /// </summary>
        /// <param name="id">The appointment ID.</param>
        /// <response code="204">Appointment deleted successfully</response>
        /// <response code="404">If the appointment is not found</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Appointments.FindAsync(id);
            if (item is null) return NotFound();

            _db.Appointments.Remove(item);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}