namespace WebApi_Hospital.DTO;

public class PatientDto
{
    public string Pesel { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Sex { get; set; }  = string.Empty;

    public List<AdmissionDto> Admissions { get; set; } = [];
    public List<BedAssignmentDto> BedAssignments { get; set; } = [];
}