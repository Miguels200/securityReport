using Xunit;
using SecurityReport.Application.Common;
using SecurityReport.Domain.Entities;

namespace Tests.Unit
{
    public class PrioridadMapperTests
    {
        [Theory]
        [InlineData(NivelRiesgo.BAJO, PrioridadRiesgo.BAJA)]
        [InlineData(NivelRiesgo.MEDIO, PrioridadRiesgo.MEDIA)]
        [InlineData(NivelRiesgo.ALTO, PrioridadRiesgo.ALTA)]
        [InlineData(NivelRiesgo.CRITICO, PrioridadRiesgo.INMEDIATA)]
        public void DesdeNivel_MapsExactlyAsApprovedByThesis(NivelRiesgo nivel, PrioridadRiesgo esperado)
        {
            Assert.Equal(esperado, PrioridadMapper.DesdeNivel(nivel));
        }
    }
}
