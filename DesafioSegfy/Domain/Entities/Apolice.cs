using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesafioSegfy.Domain.Enums;

namespace DesafioSegfy.Domain.Entities
{
    public class Apolice
    {
        public Guid Id { get; set; }
        public string NumeroApolice { get; set; } = string.Empty;
        public string CpfCnpj { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public decimal ValorPremio { get; set; }
        public DateOnly DataIncioVigencia { get; set; }
        public DateOnly DataFimVigencia { get; set; }
        public StatusApolice Status { get; set; }
    }
}