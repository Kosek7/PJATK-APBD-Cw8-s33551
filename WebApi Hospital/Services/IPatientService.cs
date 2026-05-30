using WebApi_Hospital.DTO;

namespace WebApi_Hospital.Services;

public interface IPatientService
{
    Task<List<PatientDto>> GetPatientsAsync(string? search);
    Task AssignBedAsync(string pesel, DateTime from, DateTime? to, string bedType, string ward);
}