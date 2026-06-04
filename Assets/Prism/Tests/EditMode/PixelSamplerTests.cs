using NUnit.Framework;
using Prism.Data;
using Prism.Gameplay;
using UnityEngine;

namespace Prism.Tests.EditMode
{
    public sealed class PixelSamplerTests
    {
        [Test]
        public void Sample_ReturnsRedStatsForRedFrame()
        {
            CameraImageFrame frame = new(2, 2, new byte[]
            {
                255, 0, 0, 255,
                255, 0, 0, 255,
                255, 0, 0, 255,
                255, 0, 0, 255
            });

            PixelStats stats = new PixelSampler().Sample(frame, new RectInt(0, 0, 2, 2), 2);

            Assert.That(stats.AvgHue, Is.EqualTo(0f).Within(0.001f).Or.EqualTo(1f).Within(0.001f));
            Assert.That(stats.AvgSat, Is.EqualTo(1f).Within(0.001f));
            Assert.That(stats.AvgVal, Is.EqualTo(1f).Within(0.001f));
            Assert.That(stats.HueVariance, Is.LessThan(0.001f));
        }

        [Test]
        public void Sample_ClampsRegionToFrame()
        {
            CameraImageFrame frame = new(1, 1, new byte[] { 0, 255, 0, 255 });

            PixelStats stats = new PixelSampler().Sample(frame, new RectInt(-5, -5, 10, 10), 32);

            Assert.That(stats.AvgSat, Is.EqualTo(1f).Within(0.001f));
            Assert.That(stats.AvgVal, Is.EqualTo(1f).Within(0.001f));
        }
    }
}
