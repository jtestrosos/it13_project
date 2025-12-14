using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmacyManagementSystem.Services
{
	// === SUPPLIER MODEL (Matches the structure in Suppliers.razor) ===
	public class Supplier
	{
		public int SupplierId { get; set; }
		public string Name { get; set; } = "";
		public string ContactPerson { get; set; } = "";
		public string Email { get; set; } = "";
		public string Phone { get; set; } = "";
		public string Address { get; set; } = "";
		public string City { get; set; } = "";
		public string Country { get; set; } = "";
	}

	// === INTERFACE/CONTRACT ===
	public interface ISupplierService
	{
		Task<List<Supplier>> GetAllSuppliersAsync();
		Task AddSupplierAsync(Supplier supplier);
		Task UpdateSupplierAsync(Supplier supplier);
		Task DeleteSupplierAsync(int id);
		
		// Archive / Restore
		Task<List<Supplier>> GetArchivedSuppliersAsync();
		Task RestoreSupplierAsync(int id);
	}
}
