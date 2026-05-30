using Microsoft.AspNetCore.Mvc;
using WebApi_Hospital.Services;

namespace WebApi_Hospital.Controllers;

[ApiController]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _service;

    public PatientsController(IPatientService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search)
    {
        var result = await _service.GetPatientsAsync(search);
        return Ok(result);
    }

    [HttpPost("{pesel}/bedassignments")]
    public async Task<IActionResult> AssignBed(
        string pesel,
        [FromBody] BedAssignmentRequest request)
    {
        try
        {
            await _service.AssignBedAsync(
                pesel,
                request.From,
                request.To,
                request.BedType,
                request.Ward);

            return Ok("Bed assigned successfully");
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }
}

public class BedAssignmentRequest
{
    public DateTime From { get; set; }
    public DateTime? To { get; set; }
    public string BedType { get; set; } = string.Empty;
    public string Ward { get; set; } = string.Empty;
}