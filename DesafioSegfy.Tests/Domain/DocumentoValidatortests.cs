using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesafioSegfy.Domain;
using DesafioSegfy.Domain.Service;

namespace DesafioSegfy.Tests.Domain
{
    public class DocumentoValidatortests
    {
        [Theory]
        // CPF com máscara e sem
        [InlineData("52998224725", "52998224725")]
        [InlineData("529.982.247-25", "52998224725")]
        // CNPJ com máscara e sem
        [InlineData("11222333000181", "11222333000181")]
        [InlineData("11.222.333/0001-81", "11222333000181")]
        public void Documento_valido_deve_retornar_so_digitos(string entrada, string esperado)
        {
            Assert.Equal(esperado, DocumentoValidator.Normalizar(entrada));
        }

        [Theory]
        [InlineData("12345678900")]     // CPF errado
        [InlineData("11111111111")]     // CPF repetido
        [InlineData("11222333000180")]  // CNPJ errado
        [InlineData("00000000000000")]  // CNPJ repetido
        [InlineData("")]
        [InlineData("123")]             // tamanho inválido
        [InlineData("123456789012")]    // 12 dígitos
        public void Documento_invalido_deve_lancar(string entrada)
        {
            Assert.Throws<DomainException>(() => DocumentoValidator.Normalizar(entrada));
        }
    }
}