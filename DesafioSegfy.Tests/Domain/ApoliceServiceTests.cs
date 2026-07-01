using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesafioSegfy.Domain;
using DesafioSegfy.Domain.Entities;
using DesafioSegfy.Domain.Enums;
using DesafioSegfy.Domain.Service;

namespace DesafioSegfy.Tests.Domain
{
    public class ApoliceServiceTests
    {
        private readonly ApoliceService _service = new();
        private const string CpfValido = "52998224725";
        private const string PlacaValida = "ABC1D23";

        private Apolice CriarPadrao(
            decimal valorPremio = 1500.00m,
            DateOnly? inicio = null,
            DateOnly? fim = null,
            int sequencial = 1)
        {
            var i = inicio ?? new DateOnly(2026, 1, 1);
            var f = fim ?? new DateOnly(2026, 12, 31);
            return _service.Criar(CpfValido, PlacaValida, valorPremio, i, f, sequencial);
        }

        [Fact]
        public void Criar_deve_gerar_numero_no_formato_correto()
        {
            var apolice = CriarPadrao(inicio: new DateOnly(2026, 3, 10), sequencial: 1);

            Assert.Equal("SEG-2026-0001", apolice.NumeroApolice);
        }

        [Fact]
        public void Criar_deve_usar_o_ano_do_inicio_e_sequencial_com_zeros()
        {
            var apolice = CriarPadrao(inicio: new DateOnly(2025, 12, 31), sequencial: 42);

            Assert.Equal("SEG-2025-0042", apolice.NumeroApolice);
        }

        [Fact]
        public void Criar_deve_normalizar_documento_e_placa()
        {
            var apolice = _service.Criar("529.982.247-25", "abc-1d23", 100m,
                new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 1);

            Assert.Equal("52998224725", apolice.CpfCnpj);
            Assert.Equal("ABC1D23", apolice.Placa);
        }

        [Fact]
        public void Apolice_nasce_ativa()
        {
            Assert.Equal(StatusApolice.Ativa, CriarPadrao().Status);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-1500.50)]
        public void Criar_com_valor_premio_zero_ou_negativo_deve_falhar(decimal valor)
        {
            var ex = Assert.Throws<DomainException>(() => CriarPadrao(valorPremio: valor));
            Assert.Contains("maior que zero", ex.Message);
        }

        [Fact]
        public void Criar_com_fim_antes_ou_igual_ao_inicio_deve_falhar()
        {
            var inicio = new DateOnly(2026, 6, 1);
            Assert.Throws<DomainException>(() => CriarPadrao(inicio: inicio, fim: new DateOnly(2026, 5, 31)));
            Assert.Throws<DomainException>(() => CriarPadrao(inicio: inicio, fim: inicio));
        }

        [Fact]
        public void StatusVigente_deve_ser_expirada_quando_a_vigencia_ja_passou()
        {
            var apolice = CriarPadrao(
                inicio: new DateOnly(2020, 1, 1),
                fim: new DateOnly(2020, 12, 31));
            var hoje = new DateOnly(2026, 6, 30);

            Assert.Equal(StatusApolice.Ativa, apolice.Status);                      
            Assert.Equal(StatusApolice.Expirada, _service.StatusVigente(apolice, hoje));
        }

        [Fact]
        public void StatusVigente_deve_ser_ativa_dentro_da_vigencia()
        {
            var apolice = CriarPadrao(inicio: new DateOnly(2026, 1, 1), fim: new DateOnly(2026, 12, 31));
            Assert.Equal(StatusApolice.Ativa, _service.StatusVigente(apolice, new DateOnly(2026, 6, 30)));
        }

        [Fact]
        public void Cancelar_uma_apolice_ativa_deve_mudar_status_para_cancelada()
        {
            var apolice = CriarPadrao();
            _service.Cancelar(apolice, new DateOnly(2026, 6, 30));
            Assert.Equal(StatusApolice.Cancelada, apolice.Status);
        }

        [Fact]
        public void Reativar_uma_apolice_cancelada_deve_ser_impossivel()
        {
            // Não existe reativação, cancelar de novo prova que ela não volta a ser Ativa.
            var apolice = CriarPadrao();
            var hoje = new DateOnly(2026, 6, 30);

            _service.Cancelar(apolice, hoje);

            var ex = Assert.Throws<DomainException>(() => _service.Cancelar(apolice, hoje));
            Assert.Contains("cancelada", ex.Message);
        }

        [Fact]
        public void Cancelar_uma_apolice_expirada_deve_falhar()
        {
            var apolice = CriarPadrao(inicio: new DateOnly(2020, 1, 1), fim: new DateOnly(2020, 12, 31));
            Assert.Throws<DomainException>(() => _service.Cancelar(apolice, new DateOnly(2026, 6, 30)));
        }
    }
}