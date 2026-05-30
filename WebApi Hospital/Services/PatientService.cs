using Microsoft.EntityFrameworkCore;
using WebApi_Hospital.DTO;
using WebApi_Hospital.Models;

namespace WebApi_Hospital.Services;

public class PatientService : IPatientService
{
    private readonly HospitalDbContext _context;

    public PatientService(HospitalDbContext context)
    {
        _context = context;
    }

    public async Task<List<PatientDto>> GetPatientsAsync(string? search)
    {
        var query = _context.Patients
            .Include(p => p.Admissions)
                .ThenInclude(a => a.Ward)
            .Include(p => p.BedAssignments)
                .ThenInclude(b => b.Bed)
                    .ThenInclude(b => b.BedType)
            .Include(p => p.BedAssignments)
                .ThenInclude(b => b.Bed)
                    .ThenInclude(b => b.Room)
                        .ThenInclude(r => r.Ward)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p =>
                p.FirstName.Contains(search) ||
                p.LastName.Contains(search));
        }

        return await query.Select(p => new PatientDto
        {
            Pesel = p.Pesel,
            FirstName = p.FirstName,
            LastName = p.LastName,
            Age = p.Age,
            Sex = p.Sex ? "Male" : "Female",

            Admissions = p.Admissions.Select(a => new AdmissionDto
            {
                Id = a.Id,
                AdmissionDate = a.AdmissionDate,
                DischargeDate = a.DischargeDate,
                Ward = new WardDto
                {
                    Id = a.Ward.Id,
                    Name = a.Ward.Name,
                    Description = a.Ward.Description
                }
            }).ToList(),

            BedAssignments = p.BedAssignments.Select(b => new BedAssignmentDto
            {
                Id = b.Id,
                From = b.From,
                To = b.To,
                Bed = new BedDto
                {
                    Id = b.Bed.Id,
                    BedType = new BedTypeDto
                    {
                        Id = b.Bed.BedType.Id,
                        Name = b.Bed.BedType.Name,
                        Description = b.Bed.BedType.Description
                    },
                    Room = new RoomDto
                    {
                        Id = b.Bed.Room.Id,
                        HasTv = b.Bed.Room.HasTv,
                        Ward = new WardDto
                        {
                            Id = b.Bed.Room.Ward.Id,
                            Name = b.Bed.Room.Ward.Name,
                            Description = b.Bed.Room.Ward.Description
                        }
                    }
                }
            }).ToList()
        }).ToListAsync();
    }

    public async Task AssignBedAsync(string pesel, DateTime from, DateTime? to, string bedType, string ward)
    {
        var patient = await _context.Patients.FindAsync(pesel);
        if (patient == null)
            throw new Exception("Patient not found");

        var availableBed = await _context.Beds
            .Include(b => b.BedType)
            .Include(b => b.Room)
                .ThenInclude(r => r.Ward)
            .Where(b =>
                b.BedType.Name == bedType &&
                b.Room.Ward.Name == ward &&
                !_context.BedAssignments.Any(a =>
                    a.BedId == b.Id &&
                    (a.To == null || a.To > from)))
            .FirstOrDefaultAsync();

        if (availableBed == null)
            throw new Exception("No available bed");

        var assignment = new BedAssignment
        {
            PatientPesel = pesel,
            BedId = availableBed.Id,
            From = from,
            To = to
        };

        _context.BedAssignments.Add(assignment);
        await _context.SaveChangesAsync();
    }
}