using NUnit.Framework;
using Prism.AR;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Prism.Tests.EditMode
{
    public sealed class LightEstimationMapperTests
    {
        [Test]
        public void Map_UsesDefaultsWhenValuesAreMissing()
        {
            ARLightEstimationData data = new();

            Prism.Data.LightReading reading = LightEstimationMapper.Map(data);

            Assert.AreEqual(1f, reading.Brightness);
            Assert.AreEqual(Color.white, reading.ColorCorrection);
            Assert.IsNull(reading.MainLightDirection);
        }
    }
}
