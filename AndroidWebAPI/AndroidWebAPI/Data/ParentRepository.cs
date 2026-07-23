using AndroidWebAPI.Models;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using BCrypt.Net;

namespace AndroidWebAPI.Data
{
    public class ParentRepository
    {
        private readonly string _connectionString;

        public ParentRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ── Create Parent (Registration) ──────────────────────────
        public async Task<Parent> CreateAsync(Parent parent)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);
            
            // Check if email already exists
            var existing = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT ParentID FROM dbo.Parents WHERE Email = @Email",
                new { Email = parent.Email }
            );
            if (existing != null)
                throw new InvalidOperationException($"Email {parent.Email} is already registered.");

            // Hash the password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(parent.Password);

            string query = @"
                INSERT INTO dbo.Parents 
                    (ParentID, FirstName, MiddleName, LastName, Email, Password, ContactNo, BarangayNo, Address)
                VALUES 
                    (@ParentID, @FirstName, @MiddleName, @LastName, @Email, @Password, @ContactNo, @BarangayNo, @Address)";

            await connection.ExecuteAsync(query, new
            {
                ParentID = parent.ParentID,
                FirstName = parent.FirstName,
                MiddleName = parent.MiddleName,
                LastName = parent.LastName,
                Email = parent.Email,
                Password = hashedPassword,
                ContactNo = parent.ContactNo,
                BarangayNo = parent.BarangayNo,
                Address = parent.Address
            });

            return parent;
        }

        // ── Get Single Parent by ID ───────────────────────────────
        public async Task<Parent> GetByIdAsync(Guid parentId)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);
            string query = @"
                SELECT
                    ParentID,
                    FirstName,
                    MiddleName,
                    LastName,
                    Email,
                    ContactNo,
                    BarangayNo,
                    Address
                FROM dbo.Parents
                WHERE ParentID = @ParentID";

            return await connection.QueryFirstOrDefaultAsync<Parent>(query, new { ParentID = parentId });
        }

        // ── Get All Parents ──────────────────────────────────────
        public async Task<IEnumerable<Parent>> GetAllAsync()
        {
            using IDbConnection connection = new SqlConnection(_connectionString);
            string query = @"
                SELECT
                    ParentID,
                    FirstName,
                    MiddleName,
                    LastName,
                    Email,
                    ContactNo,
                    BarangayNo,
                    Address
                FROM dbo.Parents
                ORDER BY LastName, FirstName";

            return await connection.QueryAsync<Parent>(query);
        }

        // ── Update Parent ────────────────────────────────────────
        public async Task<Parent> UpdateAsync(Parent parent)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);

            string query = @"
                UPDATE dbo.Parents
                SET
                    FirstName = @FirstName,
                    MiddleName = @MiddleName,
                    LastName = @LastName,
                    Email = @Email,
                    ContactNo = @ContactNo,
                    BarangayNo = @BarangayNo,
                    Address = @Address
                WHERE ParentID = @ParentID";

            await connection.ExecuteAsync(query, new
            {
                ParentID = parent.ParentID,
                FirstName = parent.FirstName,
                MiddleName = parent.MiddleName,
                LastName = parent.LastName,
                Email = parent.Email,
                ContactNo = parent.ContactNo,
                BarangayNo = parent.BarangayNo,
                Address = parent.Address
            });

            return parent;
        }

        // ── Login ─────────────────────────────────────────────────
        public async Task<Parent> LoginAsync(string email, string password)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);
            string query = @"
                SELECT
                    ParentID,
                    FirstName,
                    MiddleName,
                    LastName,
                    Email,
                    Password,
                    ContactNo,
                    BarangayNo,
                    Address
                FROM dbo.Parents
                WHERE Email = @Email";

            var parent = await connection.QueryFirstOrDefaultAsync<Parent>(query, new { Email = email });
            if (parent == null) return null;

            // Verify password with BCrypt
            bool isValid = BCrypt.Net.BCrypt.Verify(password, parent.Password);
            return isValid ? parent : null;
        }

        // ── Change Password ───────────────────────────────────────
        public async Task<bool> ChangePasswordAsync(Guid parentId, string currentPassword, string newPassword)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);

            // Get current password hash
            var row = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT Password FROM dbo.Parents WHERE ParentID = @ParentID",
                new { ParentID = parentId }
            );

            if (row == null) return false;

            // Verify current password
            string storedHash = row.Password;
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, storedHash)) return false;

            // Hash and update new password
            string newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await connection.ExecuteAsync(
                "UPDATE dbo.Parents SET Password = @NewPassword WHERE ParentID = @ParentID",
                new { NewPassword = newHash, ParentID = parentId }
            );

            return true;
        }

        // ── Dashboard ─────────────────────────────────────────────
        public async Task<dynamic> GetDashboardData(Guid parentId)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT
                    p.FirstName + ' ' + p.LastName AS ParentFullName,
                    c.*
                FROM dbo.Parents p
                LEFT JOIN dbo.Children c ON p.ParentID = c.ParentID
                WHERE p.ParentID = @Id";

            return await connection.QueryAsync<dynamic>(sql, new { Id = parentId });
        }
    }
}