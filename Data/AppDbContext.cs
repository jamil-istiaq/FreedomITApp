using Microsoft.EntityFrameworkCore;
using FreedomITAS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;



namespace FreedomITAS.Data
{
   
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ClientModel> Clients { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // VERY IMPORTANT for Identity
            modelBuilder.Entity<ClientModel>().HasKey(c => c.ClientId);
            modelBuilder.Entity<ClientModel>().Property(c => c.Id).ValueGeneratedOnAdd();
        }
    }



}
