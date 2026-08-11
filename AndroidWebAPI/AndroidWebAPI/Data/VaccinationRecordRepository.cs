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
        

        public async Task RecordVaccinationAsync(VaccinationRecord record)
{
    await using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
        // Prevent duplicate vaccination
        if (await AlreadyVaccinatedAsync(record.ChildID, record.VaccineID, record.DoseNumber))
            throw new Exception("This vaccine dose has already been administered.");

        // Check inventory
        var inventory = await _context.VaccineInventory
            .FirstOrDefaultAsync(i => i.InventoryID == record.InventoryID);

        if (inventory == null)
            throw new Exception("Vaccine inventory not found.");

        if (inventory.CurrentQuantity <= 0)
            throw new Exception("No vaccine stock remaining.");

        // Deduct stock
        inventory.CurrentQuantity--;

        // Save vaccination record
        record.VaccinationRecordID = Guid.NewGuid();
        record.CreatedAt = DateTime.UtcNow;

        await _context.VaccinationRecords.AddAsync(record);

        // Find corresponding timeline
        var timeline = await _context.VaccinationTimelines
    .FirstOrDefaultAsync(t =>
        t.ChildID == record.ChildID &&
        t.VaccineID == record.VaccineID &&
        t.DoseNumber == record.DoseNumber &&
        t.Status == "Pending");

        if (timeline != null)
        {
            timeline.Status = "Completed";
            timeline.VaccinationRecordID = record.VaccinationRecordID;
            timeline.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}

    }
}