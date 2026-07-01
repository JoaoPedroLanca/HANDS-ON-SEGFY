using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesafioSegfy.Api.Dtos;
using DesafioSegfy.Domain.Entities;
using DesafioSegfy.Domain.Service;
using DesafioSegfy.Infra;
using DesafioSegfy.Infra.Repositories;
using Microsoft.AspNetCore.Mvc;
using static DesafioSegfy.Infra.PersistenceExceptions;

namespace DesafioSegfy.Api.Controller
{
    [ApiController]
    [Route("apolices")]
    public class ApolicesController : ControllerBase
    {
        private readonly IApoliceRepository _repo;
        private readonly ApoliceService _service;
        private const int MaxTries = 3;

        public ApolicesController(IApoliceRepository repo, ApoliceService service)
        {
            _repo = repo;
            _service = service;
        }


        [HttpGet]
        public async Task<ActionResult<List<ApoliceResponse>>> Listar()
        {
            var apolices = await _repo.ListarAsync();
            return Ok(apolices.Select(ToResponse));
        }

        [HttpGet("{id:guid}", Name = nameof(ObterPorId))]
        public async Task<ActionResult<ApoliceResponse>> ObterPorId(Guid id)
        {
            var apolice = await _repo.ObterPorIdAsync(id);
            if (apolice == null)
                return NotFound(new { error = $"Apólice {id} não encontrada" });

            return Ok(ToResponse(apolice));
        }

        [HttpPost]
        public async Task<ActionResult<ApoliceResponse>> Criar(CriarApoliceRequest request)
        {
            for (var tentativa = 1; ; tentativa++)
            {
                var sequencial = await _repo.ProximoSequencialAsync(request.DataIncioVigencia.Year);

                var apolice = _service.Criar(
                    request.CpfCnpj, request.Placa, request.ValorPremio,
                    request.DataIncioVigencia, request.DataFimVigencia, sequencial
                );

                try
                {
                    await _repo.AdicionarAsync(apolice);
                    return CreatedAtRoute(nameof(ObterPorId), new { id = apolice.Id }, ToResponse(apolice));
                }
                catch (NumeroApoliceEmUsoException) when (tentativa < MaxTries)
                {
                    // Vazio para tentar de novo
                }
            }
        }

        [HttpPatch("{id:guid}")]
        public async Task<ActionResult<ApoliceResponse>> Atualizar(Guid id, AtualizarApoliceRequest request)
        {
            var apolice = await _repo.ObterPorIdAsync(id);
            if (apolice is null)
                return NotFound(new { erro = $"Apólice {id} não encontrada" });

            // PATCH não envia campo null, mantém o valor atual
            _service.Atualizar(
                apolice,
                request.Placa ?? apolice.Placa,
                request.ValorPremio ?? apolice.ValorPremio,
                request.DataInicioVigencia ?? apolice.DataIncioVigencia,
                request.DataFimVigencia ?? apolice.DataFimVigencia);
            await _repo.SalvarAsync();

            return Ok(ToResponse(apolice));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Remover(Guid id)
        {
            var apolice = await _repo.ObterPorIdAsync(id);
            if (apolice is null)
                return NotFound(new { erro = $"Apólice {id} não encontrada" });

            await _repo.RemoverAsync(apolice);
            return NoContent();
        }

        [HttpPost("{id:guid}/cancelar")]
        public async Task<ActionResult<ApoliceResponse>> Cancelar(Guid id)
        {
            var apolice = await _repo.ObterPorIdAsync(id);
            if (apolice is null)
                return NotFound(new { erro = $"Apólice {id} não encontrada." });

            var hoje = DateOnly.FromDateTime(DateTime.Today);
            _service.Cancelar(apolice, hoje);
            await _repo.SalvarAsync();

            return Ok(ToResponse(apolice));
        }

        [HttpGet("vencendo")]
        public async Task<ActionResult<List<ApoliceResponse>>> Vencendo()
        {
            var hoje = DateOnly.FromDateTime(DateTime.Today);
            var apolices = await _repo.ListarVencendoEmAsync(hoje, 30);
            return Ok(apolices.Select(ToResponse));
        }

        private ApoliceResponse ToResponse(Apolice apolice)
        {
            var hoje = DateOnly.FromDateTime(DateTime.Today);
            return new ApoliceResponse
            {
                Id = apolice.Id,
                NumeroApolice = apolice.NumeroApolice,
                CpfCnpj = apolice.CpfCnpj,
                Placa = apolice.Placa,
                ValorPremio = apolice.ValorPremio,
                DataInicioVigencia = apolice.DataIncioVigencia,
                DataFimVigencia = apolice.DataFimVigencia,
                Status = _service.StatusVigente(apolice, hoje).ToString()
            };
        }
    }
}