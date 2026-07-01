using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesafioSegfy.Domain.Entities;
using DesafioSegfy.Domain.Enums;

namespace DesafioSegfy.Domain.Service
{
    public class ApoliceService
    {
        public Apolice Criar(
            string cpfCnpj,
            string placa,
            decimal ValorPremio,
            DateOnly dataInicioVigencia,
            DateOnly dataFimVigencia,
            int sequencial
        )
        {
            if (ValorPremio <= 0)
                throw new DomainException("Valor do premio deve ser maior que zero");

            if (dataFimVigencia <= dataInicioVigencia)
                throw new DomainException("Data do fim da vigencia deve ser posterior a data de início da vigencia");

            if (sequencial is < 1 or > 9999)
                throw new DomainException("Sequencial da apólice deve estar entre 1 e 9999");

            return new Apolice
            {
                Id = Guid.NewGuid(),
                NumeroApolice = GerarNumero(dataInicioVigencia, sequencial),
                CpfCnpj = DocumentoValidator.Normalizar(cpfCnpj),
                Placa = PlacaValidator.Normalizar(placa),
                ValorPremio = ValorPremio,
                DataIncioVigencia = dataInicioVigencia,
                DataFimVigencia = dataFimVigencia,
                Status = StatusApolice.Ativa
            };
        }

        private static string GerarNumero(DateOnly inicio, int sequencial)
            => $"SEG-{inicio.Year:D4}-{sequencial:D4}";

        public StatusApolice StatusVigente(Apolice apolice, DateOnly dataReferencia)
        {
            if (apolice.Status == StatusApolice.Cancelada)
                return StatusApolice.Cancelada;

            if (dataReferencia > apolice.DataFimVigencia)
                return StatusApolice.Expirada;

            return StatusApolice.Ativa;
        }

        public void Cancelar(Apolice apolice, DateOnly dataReferencia)
        {
            var atual = StatusVigente(apolice, dataReferencia);

            if (atual == StatusApolice.Cancelada)
                throw new DomainException("Apólice já está cancelada");

            if (atual == StatusApolice.Expirada)
                throw new DomainException("Apólice expirada não pode ser cancelada");

            apolice.Status = StatusApolice.Cancelada;
        }

        public void Atualizar(
            Apolice apolice,
            string placa,
            decimal valorPremio,
            DateOnly dataIncioVigencia,
            DateOnly dataFimVigencia
        )
        {
            if (valorPremio <= 0)
                throw new DomainException("Valor premio deve ser maior que zero");

            if (dataFimVigencia <= dataIncioVigencia)
                throw new DomainException("Data do fim da vigencia deve ser posterior a data de início da vigencia");

            apolice.Placa = PlacaValidator.Normalizar(placa);
            apolice.ValorPremio = valorPremio;
            apolice.DataIncioVigencia = dataIncioVigencia;
            apolice.DataFimVigencia = dataFimVigencia;
        }
    }
}