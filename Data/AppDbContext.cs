using Microsoft.EntityFrameworkCore;
using Medicine_ERP.Data.Models.ViewModels;

namespace Medicine_ERP_Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<Medicine> Medicines => Set<Medicine>();
	}
}