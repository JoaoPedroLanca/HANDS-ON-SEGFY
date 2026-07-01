using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DesafioSegfy.Api.Dtos
{
    public class AtualizarApoliceRequest
    {
        public string? Placa { get; set; }
        public decimal? ValorPremio { get; set; }
        public DateOnly? DataInicioVigencia { get; set; }
        public DateOnly? DataFimVigencia { get; set; }
    }
}