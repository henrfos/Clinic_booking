namespace ClinicExam.Models
{
    public class Speciality
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
