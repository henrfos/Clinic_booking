namespace ClinicExam.DTOs
{
    public class DoctorCreateDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName  { get; set; } = null!;
        public int ClinicId     { get; set; }
        public int SpecialityId { get; set; }
    }
}