using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesafioSegfy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DesafioSegfy.Infra
{
    public class SegfyDbContext : DbContext
    {
        public DbSet<Apolice> Apolices { get; set; }

        public SegfyDbContext(DbContextOptions<SegfyDbContext> options) : base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Apolice>(apolice =>
            {
                apolice.ToTable("Apolice");
                apolice.HasKey(a => a.Id);

                apolice.Property(a => a.NumeroApolice)
                    .IsRequired();

                apolice.Property(a => a.CpfCnpj)
                    .IsRequired();

                apolice.Property(a => a.Placa)
                    .IsRequired();

                apolice.Property(a => a.ValorPremio)
                    .IsRequired();

                apolice.Property(a => a.Status)
                    .HasConversion<string>()
                    .IsRequired();

                apolice.Property(a => a.DataIncioVigencia)
                    .IsRequired();

                apolice.Property(a => a.DataFimVigencia)
                    .IsRequired();

                apolice.HasIndex(a => a.NumeroApolice)
                    .IsUnique();

                apolice.HasIndex(a => a.CpfCnpj)
                    .IsUnique();
            });
        }
    }
}