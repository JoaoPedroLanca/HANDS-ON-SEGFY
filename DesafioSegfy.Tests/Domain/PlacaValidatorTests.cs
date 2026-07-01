using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesafioSegfy.Domain;
using DesafioSegfy.Domain.Service;

namespace DesafioSegfy.Tests.Domain
{
    public class PlacaValidatorTests
    {
        [Theory]
        [InlineData("ABC1234", "ABC1234")]   // antigo
        [InlineData("abc1234", "ABC1234")]   // maiúsculas
        [InlineData("abc-1234", "ABC1234")]  // remove hífen
        [InlineData("ABC1D23", "ABC1D23")]   // Mercosul
        [InlineData(" abc1d23 ", "ABC1D23")] // trim e upper
        public void Placa_valida_deve_ser_normalizada(string entrada, string esperado)
        {
            Assert.Equal(esperado, PlacaValidator.Normalizar(entrada));
        }

        [Theory]
        [InlineData("")]
        [InlineData("AB1234")]    // menos letras do que precisa
        [InlineData("ABCD123")]   // formato errado
        [InlineData("ABC12E3")]   // não bate antigo nem Mercosul
        [InlineData("1234567")]   // só números
        public void Placa_invalida_deve_lancar(string entrada)
        {
            Assert.Throws<DomainException>(() => PlacaValidator.Normalizar(entrada));
        }
    }
}