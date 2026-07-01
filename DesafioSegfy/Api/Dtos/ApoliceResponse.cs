using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DesafioSegfy.Api.Dtos
{
    public class ApoliceResponse
    {
        public Guid Id { get; set; }
        public string NumeroApolice { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public decimal ValorPremio { get; set; }
        public DateOnly DataInicioVigencia { get; set; }
        public DateOnly DataFimVigencia { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}