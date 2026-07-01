using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DesafioSegfy.Api.Dtos
{
    public class CriarApoliceRequest
    {
        public string CpfCnpj { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public decimal ValorPremio { get; set; }
        public DateOnly DataIncioVigencia { get; set; }
        public DateOnly DataFimVigencia { get; set; }
    }
}