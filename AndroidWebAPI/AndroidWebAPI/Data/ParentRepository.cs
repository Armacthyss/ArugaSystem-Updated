using AndroidWebAPI.Models;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;

namespace AndroidWebAPI.Data
{
    public class ParentRepository(IConfiguration configuration)
    {
        private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection");

        // ── Login ─────────────────────────────────────────────────
        public async Task<Parent?> LoginAsync(string email, string password)
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
    WHERE Email = @Email
      AND Password = @Password";

    return await connection.QueryFirstOrDefaultAsync<Parent>(
        query,
        new
        {
            Email = email,
            Password = password
        });
}
   
        // ── Change Password ───────────────────────────────────────
        public async Task<bool> ChangePasswordAsync(Guid parentId, string currentPassword, string newPassword)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);

            // Check current password matches
            var row = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT Password FROM dbo.Parents WHERE ParentID = @ParentID",
                new { ParentID = parentId }
            );

            if (row == null) return false;

            // Plain text check — swap for BCrypt.Verify() in production
            string storedPassword = row.Password;
            if (storedPassword != currentPassword) return false;

            // Update to new password
            await connection.ExecuteAsync(
                "UPDATE dbo.Parents SET Password = @NewPassword WHERE ParentID = @ParentID",
                new { NewPassword = newPassword, ParentID = parentId }
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

public async Task<Parent> CreateAsync(Parent parent)
{
    using IDbConnection connection = new SqlConnection(_connectionString);

    string sql = @"
    INSERT INTO Parents
    (
        ParentID,
        FirstName,
        MiddleName,
        LastName,
        Email,
        ContactNo,
        BarangayNo,
        Address,
        Password,
        CreatedAt,
        UpdatedAt
    )
    VALUES
    (
        @ParentID,
        @FirstName,
        @MiddleName,
        @LastName,
        @Email,
        @ContactNo,
        @BarangayNo,
        @Address,
        @Password,
        GETDATE(),
        GETDATE()
    )";

    await connection.ExecuteAsync(sql, parent);

    return parent;
}
public async Task<Parent?> GetByIdAsync(Guid id)
{
    using IDbConnection connection = new SqlConnection(_connectionString);

    string sql = @"
    SELECT *
    FROM Parents
    WHERE ParentID = @Id";

    return await connection.QueryFirstOrDefaultAsync<Parent>(sql, new { Id = id });
}

public async Task<IEnumerable<Parent>> GetAllAsync()
{
    using IDbConnection connection = new SqlConnection(_connectionString);

    string sql = @"
    SELECT *
    FROM Parents
    ORDER BY LastName, FirstName";

    return await connection.QueryAsync<Parent>(sql);
}

public async Task<Parent> UpdateAsync(Parent parent)
{
    using IDbConnection connection = new SqlConnection(_connectionString);

    string sql = @"
    UPDATE Parents
    SET
        FirstName = @FirstName,
        MiddleName = @MiddleName,
        LastName = @LastName,
        Email = @Email,
        ContactNo = @ContactNo,
        BarangayNo = @BarangayNo,
        Address = @Address,
        UpdatedAt = GETDATE()
    WHERE ParentID = @ParentID";

    await connection.ExecuteAsync(sql, parent);

    return parent;
}
public async Task<bool> DeleteAsync(Guid id)
{
    using IDbConnection connection = new SqlConnection(_connectionString);

    string sql = @"
    DELETE FROM Parents
    WHERE ParentID = @Id";

    int rows = await connection.ExecuteAsync(sql, new { Id = id });

    return rows > 0;
}

    }
}