using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace PharmacyManagementSystem.Services
{

	public class Staff
	{
		public int StaffId { get; set; }
		public string Name { get; set; } = "";
		public string Email { get; set; } = "";
		public string Role { get; set; } = "";
		public string Phone { get; set; } = "";
		public string Status { get; set; } = "active";
	}

	public class Shift
	{
		public int ShiftId { get; set; }
		public int StaffId { get; set; }
		public string StaffName { get; set; } = "";
		public string Role { get; set; } = "";

		public DateTime Date { get; set; } = DateTime.Today;
		public TimeSpan StartTime { get; set; }
		public TimeSpan EndTime { get; set; }

		public string DateString { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");
		public string StartTimeString { get; set; } = "09:00";
		public string EndTimeString { get; set; } = "17:00";
	}

	public interface IStaffService
	{
		Task<List<Staff>> GetAllStaffAsync();
		Task<List<Shift>> GetAllShiftsAsync();
		Task AddStaffAsync(Staff member);
		Task AddShiftAsync(Shift shift);
	}
}