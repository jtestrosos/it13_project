using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace PharmacyManagementSystem.Services
{
	public class StaffService : IStaffService
	{
		private readonly string _connectionString;

		public StaffService(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("DefaultConnection")
								?? throw new InvalidOperationException("DefaultConnection string not found.");
		}

		// --- GET ALL STAFF (READ) ---
		public async Task<List<Staff>> GetAllStaffAsync()
		{
			var members = new List<Staff>();
			string sql = "SELECT StaffId, Name, Email, Role, Phone, Status FROM [Staff]";

			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (var command = new SqlCommand(sql, connection))
				{
					using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							members.Add(new Staff
							{
								StaffId = reader.GetInt32(reader.GetOrdinal("StaffId")),
								Name = reader.GetString(reader.GetOrdinal("Name")),
								Email = reader.GetString(reader.GetOrdinal("Email")),
								Role = reader.GetString(reader.GetOrdinal("Role")),
								Phone = reader.GetString(reader.GetOrdinal("Phone")),
								Status = reader.GetString(reader.GetOrdinal("Status")),
							});
						}
					}
				}
			}
			return members;
		}

		// --- GET ALL SHIFTS (READ) ---
		public async Task<List<Shift>> GetAllShiftsAsync()
		{
			var shifts = new List<Shift>();

			// FIX 1: Select the actual column 's.ShiftDate' and alias it as 'Date'.
			string sql = @"
                SELECT s.ShiftId, s.StaffId, s.ShiftDate AS Date, s.StartTime, s.EndTime, 
                       m.Name AS StaffName, m.Role
                FROM [Shifts] s 
                JOIN [Staff] m ON s.StaffId = m.StaffId 
                WHERE s.ShiftDate >= DATEADD(day, -7, GETDATE())
                ORDER BY s.ShiftDate DESC";

			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (var command = new SqlCommand(sql, connection))
				{
					using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							shifts.Add(new Shift
							{
								ShiftId = reader.GetInt32(reader.GetOrdinal("ShiftId")),
								StaffId = reader.GetInt32(reader.GetOrdinal("StaffId")),
								StaffName = reader.GetString(reader.GetOrdinal("StaffName")),
								Role = reader.GetString(reader.GetOrdinal("Role")),

								// Reading the aliased 'Date' column
								Date = reader.GetDateTime(reader.GetOrdinal("Date")),

								// FIX 2: Use GetTimeSpan to read time data directly, resolving cast error.
								StartTime = reader.GetTimeSpan(reader.GetOrdinal("StartTime")),
								EndTime = reader.GetTimeSpan(reader.GetOrdinal("EndTime")),
							});
						}
					}
				}
			}
			return shifts;
		}

		// --- ADD STAFF (CREATE) ---
		public async Task AddStaffAsync(Staff member)
		{
			string sql = @"
                INSERT INTO [Staff] (Name, Email, Role, Phone, Status) 
                VALUES (@Name, @Email, @Role, @Phone, @Status)";

			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (var command = new SqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@Name", member.Name);
					command.Parameters.AddWithValue("@Email", member.Email);
					command.Parameters.AddWithValue("@Role", member.Role);
					command.Parameters.AddWithValue("@Phone", member.Phone);
					command.Parameters.AddWithValue("@Status", member.Status);
					await command.ExecuteNonQueryAsync();
				}
			}
		}

		// --- ADD SHIFT (CREATE) ---
		public async Task AddShiftAsync(Shift shift)
		{
			string sql = @"
                INSERT INTO [Shifts] (StaffId, ShiftDate, StartTime, EndTime) 
                VALUES (@StaffId, @ShiftDate, @StartTime, @EndTime)";

			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (var command = new SqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@StaffId", shift.StaffId);

					// Use ShiftDate column for insertion
					command.Parameters.AddWithValue("@ShiftDate", shift.Date.Date);

					// Pass TimeSpan as string format for SQL
					command.Parameters.AddWithValue("@StartTime", shift.StartTime.ToString());
					command.Parameters.AddWithValue("@EndTime", shift.EndTime.ToString());
					await command.ExecuteNonQueryAsync();
				}
			}
		}
	

		// --- UPDATE SHIFT (UPDATE) ---
		public async Task UpdateShiftAsync(Shift shift)
		{
			string sql = @"
				UPDATE [Shifts] 
				SET StaffId = @StaffId, 
					ShiftDate = @ShiftDate, 
					StartTime = @StartTime, 
					EndTime = @EndTime
				WHERE ShiftId = @ShiftId";

			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (var command = new SqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@ShiftId", shift.ShiftId);
					command.Parameters.AddWithValue("@StaffId", shift.StaffId);
					command.Parameters.AddWithValue("@ShiftDate", shift.Date.Date);
					command.Parameters.AddWithValue("@StartTime", shift.StartTime.ToString());
					command.Parameters.AddWithValue("@EndTime", shift.EndTime.ToString());
					await command.ExecuteNonQueryAsync();
				}
			}
		}

		// --- DELETE SHIFT (DELETE) ---
		public async Task DeleteShiftAsync(int shiftId)
		{
			string sql = "DELETE FROM [Shifts] WHERE ShiftId = @ShiftId";

			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (var command = new SqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@ShiftId", shiftId);
					await command.ExecuteNonQueryAsync();
				}
			}
		}
	}
}