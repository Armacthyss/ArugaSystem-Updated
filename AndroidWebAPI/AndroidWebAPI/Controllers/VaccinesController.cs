using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AndroidWebAPI.Data;

[ApiController]
[Route("api/[controller]")]
public class VaccinesController : ControllerBase
{
    private readonly AppDbContext _context;
    public VaccinesController(AppDbContext context) => _context = context;

    // GET api/Vaccines/with-doses
    [HttpGet("with-doses")]
    public async Task<IActionResult> GetWithDoses()
    {
        var vaccines = await _context.Vaccines.ToListAsync();
        var doses    = await _context.VaccineDoses.ToListAsync();

        var result = vaccines.Select(v => new
        {
            vaccineID   = v.VaccineID,
            vaccineName = v.VaccineName,
            doses       = doses
                .Where(d => d.VaccineID == v.VaccineID)
                .OrderBy(d => d.DoseNumber)
                .Select(d => new {
                    doseNumber      = d.DoseNumber,
                    minIntervalDays = d.MinIntervalDays
                })
                .ToList()
        });

        return Ok(result);
    }
}