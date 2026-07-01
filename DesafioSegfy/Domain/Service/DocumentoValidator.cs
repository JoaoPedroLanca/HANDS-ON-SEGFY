using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DesafioSegfy.Domain.Service
{
    public class DocumentoValidator
    {
        public static string Normalizar(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new DomainException("CPF/CNPJ é obrigatório");

            var digitos = new string(input
                .Where(char.IsDigit)
                .ToArray());

            var valido = digitos.Length switch
            {
                11 => CpfValido(digitos),
                14 => CpnjValido(digitos),
                _ => false
            };

            if (!valido)
                throw new DomainException($"CPF/CNPJ inválidos: {input}");

            return digitos;
        }

        private static bool CpfValido(string cpf)
        {
            if (cpf.Distinct().Count() == 1)
                return false;

            var numeros = cpf
                .Select(c => c - '0')
                .ToArray();

            var digito1 = CalcDigitoCpf(numeros, quantidade: 9, pesoInicial: 10);

            if (digito1 != numeros[9])
                return false;

            var digito2 = CalcDigitoCpf(numeros, quantidade: 10, pesoInicial: 11);

            return digito2 == numeros[10];
        }

        private static bool CpnjValido(string cnpj)
        {
            if (cnpj.Distinct().Count() == 1)
                return false;

            var numeros = cnpj
                .Select(c => c - '0')
                .ToArray();

            int[] pesos1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
            int[] pesos2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

            var digito1 = CalcDigitoPorPesos(numeros, pesos1);

            if (digito1 != numeros[12])
                return false;

            var digito2 = CalcDigitoPorPesos(numeros, pesos2);

            return digito2 == numeros[13];
        }

        private static int CalcDigitoCpf(int[] numeros, int quantidade, int pesoInicial)
        {
            var soma = 0;
            
            for (var i = 0; i < quantidade; i++)
            {
                soma += numeros[i] * (pesoInicial - i);
            }

            var resto = soma % 11;

            return resto < 2 
                ? 0 
                : 11 - resto;
        }

        private static int CalcDigitoPorPesos(int[] numeros, int[] pesos)
        {
            var soma = 0;
            
            for (var i = 0; i < pesos.Length; i++)
            {
                soma += numeros[i] * pesos[i];
            }

            var resto = soma % 11;

            return resto < 2
                ? 0
                : 11 - resto;
        }
    }
}