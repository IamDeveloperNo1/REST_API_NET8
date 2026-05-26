using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace REST_API_NET8.Data
{
    public class FaceScanDbContext : DbContext
    {
        protected readonly IConfiguration _configuration;
        public FaceScanDbContext(IConfiguration configuration, DbContextOptions<FaceScanDbContext> option) : base(option)
        {
            _configuration = configuration;
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = _configuration.GetConnectionString("FaceScan");
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(
                    connectionString,
                    connection => connection.CommandTimeout(300)
                );
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}