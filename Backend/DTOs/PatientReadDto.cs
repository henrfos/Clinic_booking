namespace ClinicExam.DTOs
{
    public class PatientReadDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName  { get; set; } = null!;
        public string Email     { get; set; } = null!;
        public DateTime BirthDate { get; set; }
    }
}