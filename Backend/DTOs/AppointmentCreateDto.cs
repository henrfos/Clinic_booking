namespace ClinicExam.DTOs
{
    public class AppointmentCreateDto
    {
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int ClinicId { get; set; }
        public int CategoryId { get; set; }
        /// <summary>UTC start time, e.g. "2025-09-28T09:30:00Z".</summary>
        public DateTime StartUtc { get; set; }
        /// <summary>Duration in minutes (must be positive).</summary>
        public int DurationMinutes { get; set; }
    }
}
