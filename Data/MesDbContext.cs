using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using REST_API_NET8.Models.SAP;

namespace REST_API_NET8.Data
{
    public class MesDbContext : DbContext
    {
        protected readonly IConfiguration _configuration;
        public MesDbContext(IConfiguration configuration, DbContextOptions<MesDbContext> option) : base(option)
        {
            _configuration = configuration;
        }

        public DbSet<MesForJoinSapDto> mesForJoinSapDtos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = _configuration.GetConnectionString("CosmoMes");
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
            modelBuilder.Entity<MesForJoinSapDto>().HasNoKey();
            base.OnModelCreating(modelBuilder);
        }
    }
}