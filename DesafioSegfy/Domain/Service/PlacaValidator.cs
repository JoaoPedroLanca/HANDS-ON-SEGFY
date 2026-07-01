using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DesafioSegfy.Domain.Service
{
    public class PlacaValidator
    {
        private static readonly Regex FormatoAntigo = new ("^[A-Z]{3}[0-9]{4}$", RegexOptions.Compiled);

        private static readonly Regex Mercosul = new ("^[A-Z]{3}[0-9][A-Z][0-9]{2}$", RegexOptions.Compiled);

        public static string Normalizar(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new DomainException("Placa é obrigatória");

            var normalizada = input
                .Trim()
                .ToUpperInvariant()
                .Replace("-", string.Empty)
                .Replace(" ", string.Empty);

            if (!FormatoAntigo.IsMatch(normalizada)
                && !Mercosul.IsMatch(normalizada))
                    throw new DomainException($"Placa inválida: {input}");

            return normalizada;
        }
    }
}