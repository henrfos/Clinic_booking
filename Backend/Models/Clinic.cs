namespace ClinicExam.Models
{
    public class Clinic
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Address { get; set; }

        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}