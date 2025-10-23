namespace ClinicExam.DTOs
{
    public class PatientCreateDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName  { get; set; } = null!;
        public string Email     { get; set; } = null!;
        public DateTime BirthDate { get; set; }
    }
}