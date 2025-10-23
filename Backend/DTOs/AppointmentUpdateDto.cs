namespace ClinicExam.DTOs
{
    public class AppointmentUpdateDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int ClinicId { get; set; }
        public int CategoryId { get; set; }
        public DateTime StartUtc { get; set; }
        public int DurationMinutes { get; set; }
    }
}