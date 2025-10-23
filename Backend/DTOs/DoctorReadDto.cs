namespace ClinicExam.DTOs
{
    public class DoctorReadDto
    {
        public int Id { get; set; }
        public string FirstName    { get; set; } = null!;
        public string LastName     { get; set; } = null!;
        public int ClinicId        { get; set; }
        public string ClinicName   { get; set; } = null!;
        public int SpecialityId    { get; set; }
        public string SpecialityName { get; set; } = null!;
    }
}