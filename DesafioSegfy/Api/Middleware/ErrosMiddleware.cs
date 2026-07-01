using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesafioSegfy.Domain;
using DesafioSegfy.Infra;
using static DesafioSegfy.Infra.PersistenceExceptions;

namespace DesafioSegfy.Api.Middleware
{
    public class ErrosMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrosMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DomainException ex)
            {
                await Response(context, StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (CpfCnpjDuplicadoException ex)
            {
                await Response(context, StatusCodes.Status409Conflict, ex.Message);
            }
            catch (NumeroApoliceEmUsoException)
            {
                await Response(context, StatusCodes.Status409Conflict, "Não foi possível gerar um número de apólice único. Tente novamente");
            }
        }

        private static Task Response(HttpContext context, int status, string mensagem)
        {
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json";

            return context.Response.WriteAsJsonAsync(new { erro = mensagem });
        }
    }
}