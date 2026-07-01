using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesafioSegfy.Domain.Entities;

namespace DesafioSegfy.Infra.Repositories
{
    public interface IApoliceRepository
    {
        Task<IReadOnlyList<Apolice>> ListarAsync();
        Task<Apolice?> ObterPorIdAsync(Guid id);
        Task AdicionarAsync(Apolice apolice);
        Task SalvarAsync();
        Task RemoverAsync(Apolice apolice);
        Task<int> ProximoSequencialAsync(int ano);
        Task<IReadOnlyList<Apolice>> ListarVencendoEmAsync(DateOnly hoje, int dias);
    }
}