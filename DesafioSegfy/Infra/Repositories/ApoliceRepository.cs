using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesafioSegfy.Domain.Entities;
using DesafioSegfy.Domain.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using static DesafioSegfy.Infra.PersistenceExceptions;

namespace DesafioSegfy.Infra.Repositories
{
    public class ApoliceRepository : IApoliceRepository
    {
        private readonly SegfyDbContext _context;
        
        public ApoliceRepository(SegfyDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(Apolice apolice)
        {
            if (await _context.Apolices.AnyAsync(a => a.CpfCnpj == apolice.CpfCnpj))
                throw new CpfCnpjDuplicadoException(apolice.CpfCnpj);

            await _context.Apolices.AddAsync(apolice);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ViolacaoDeUnicidade(ex))
            {
                _context.Entry(apolice).State = EntityState.Detached;
                throw new NumeroApoliceEmUsoException();
            }
        }

        public async Task<IReadOnlyList<Apolice>> ListarAsync()
        {
            var apolices = await _context.Apolices
                .AsNoTracking()
                .ToListAsync();

            return apolices;
        }

        public async Task<IReadOnlyList<Apolice>> ListarVencendoEmAsync(DateOnly hoje, int dias)
        {
            var limite = hoje.AddDays(dias);

            // Feito em SQL puro para atender ao requisito "Também implemente uma consulta SQL que liste as apólices que vencem nos próximos 30 dias;"

            // Mas poderia ser feito interiamente com Linq mesmo, que é o natural
            
            const string sql = @"
                SELECT *
                FROM Apolices
                WHERE Status = {0}
                    AND DataFimVigencia >= {1}
                    AND DataFimVigencia <= {2}
                ORDER BY DataFimVigencia
            ";

            var lista = await _context.Apolices
                .FromSqlRaw(sql, 
                    nameof(StatusApolice.Ativa),
                    hoje.ToString("yyyy-MM-dd"),
                    limite.ToString("yyyy-MM-dd"))
                .AsNoTracking()
                .ToListAsync();

            return lista;
        }

        public async Task<Apolice?> ObterPorIdAsync(Guid id)
        {
            var apolice = await _context.Apolices
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            return apolice;
        }

        public async Task<int> ProximoSequencialAsync(int ano)
        {
            var prefixo = $"SEG-{ano:D4}-";

            var ultimoNumero = await _context.Apolices
                .Where(a => a.NumeroApolice.StartsWith(prefixo))
                .MaxAsync(a => (string?)a.NumeroApolice);

            if (ultimoNumero == null)
                return 1;

            return int.Parse(ultimoNumero.Substring(prefixo.Length)) + 1;
        }

        public async Task RemoverAsync(Apolice apolice)
        {
            _context.Apolices.Remove(apolice);
            await _context.SaveChangesAsync();
        }

        public async Task SalvarAsync()
        {
            await _context.SaveChangesAsync();
        }

        // 2067 é o código para a exception SQLITE_CONSTRAINT_UNIQUE
        private static bool ViolacaoDeUnicidade(DbUpdateException ex)
            => ex.InnerException is SqliteException sqlite
                && sqlite.SqliteExtendedErrorCode == 2067;
    }
}