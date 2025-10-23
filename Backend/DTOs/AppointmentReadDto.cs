namespace ClinicExam.DTOs
{
    public class AppointmentReadDto
    {
        public int Id { get; set; }

        public int PatientId { get; set; }
        public string PatientFirstName { get; set; } = null!;
        public string PatientLastName  { get; set; } = null!;

        public int DoctorId { get; set; }
        public string DoctorFirstName  { get; set; } = null!;
        public string DoctorLastName   { get; set; } = null!;
        public int SpecialityId        { get; set; }
        public string SpecialityName   { get; set; } = null!;

        public int ClinicId { get; set; }
        public string ClinicName { get; set; } = null!;

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;

        public DateTime StartUtc { get; set; }
        public int DurationMinutes { get; set; }
    }
}