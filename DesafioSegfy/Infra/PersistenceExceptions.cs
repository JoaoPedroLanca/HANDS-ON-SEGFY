using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DesafioSegfy.Infra
{
    public class PersistenceExceptions
    {
        public sealed class CpfCnpjDuplicadoException : Exception
        {
            public CpfCnpjDuplicadoException(string cpfCnpj) 
                : base($"Já existe uma apólice para o CPF/CNPJ {cpfCnpj}") {}
        }

        public sealed class NumeroApoliceEmUsoException : Exception
        {
            public NumeroApoliceEmUsoException()
                : base("Número de apólice já em uso") {}
        }
    }
}