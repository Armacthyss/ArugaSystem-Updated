using AndroidWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AndroidWebAPI.Data.Repositories
{
    public class VaccinationRecordRepository : IVaccinationRecordRepository
    {
        private readonly AppDbContext _context;

        public VaccinationRecordRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VaccinationRecord>> GetAllAsync()
        {
            return await _context.VaccinationRecords
                .Include(r => r.Child)
                .Include(r => r.Vaccine)
                .Include(r => r.Inventory)
                .ToListAsync();
        }

        public async Task<VaccinationRecord?> GetByIdAsync(Guid vaccinationRecordId)
        {
            return await _context.VaccinationRecords
                .FirstOrDefaultAsync(r => r.VaccinationRecordID == vaccinationRecordId);
        }

        public async Task<IEnumerable<VaccinationRecord>> GetByChildAsync(Guid childId)
        {
            return await _context.VaccinationRecords
                .Where(r => r.ChildID == childId)
                .OrderBy(r => r.VaccinationDate)
                .ToListAsync();
        }

        public async Task AddAsync(VaccinationRecord record)
        {
            await _context.VaccinationRecords.AddAsync(record);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(VaccinationRecord record)
        {
            _context.VaccinationRecords.Update(record);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid vaccinationRecordId)
        {
            var record = await _context.VaccinationRecords.FindAsync(vaccinationRecordId);

            if (record != null)
            {
                _context.VaccinationRecords.Remove(record);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> AlreadyVaccinatedAsync(Guid childId, int vaccineId, int doseNumber)
        {
            return await _context.VaccinationRecords.AnyAsync(r =>
                r.ChildID == childId &&
                r.VaccineID == vaccineId &&
                r.DoseNumber == doseNumber);
        }
    }
}