using Prism.Data;
using UnityEngine;

namespace Prism.Gameplay
{
    /// <summary>
    /// RGBA32 프레임을 균등 다운샘플링해 평균 HSV와 팔레트 근사값을 계산합니다.
    /// </summary>
    public sealed class PixelSampler : IPixelSampler
    {
        /// <inheritdoc />
        public PixelStats Sample(in CameraImageFrame frame, RectInt region, int downsample = 32)
        {
            if (!frame.IsValid)
            {
                return new PixelStats(0f, 0f, 0f, 0f, Color.black, Color.black);
            }

            RectInt clamped = ClampRegion(frame, region);
            if (clamped.width <= 0 || clamped.height <= 0)
            {
                return new PixelStats(0f, 0f, 0f, 0f, Color.black, Color.black);
            }

            int samplesX = Mathf.Clamp(downsample, 1, clamped.width);
            int samplesY = Mathf.Clamp(downsample, 1, clamped.height);
            int sampleCount = samplesX * samplesY;

            float satSum = 0f;
            float valSum = 0f;
            float hueSin = 0f;
            float hueCos = 0f;
            Color paletteASum = Color.black;
            Color paletteBSum = Color.black;
            int paletteACount = 0;
            int paletteBCount = 0;

            for (int y = 0; y < samplesY; y++)
            {
                int pixelY = clamped.yMin + Mathf.Min(clamped.height - 1, Mathf.FloorToInt((y + 0.5f) * clamped.height / samplesY));
                for (int x = 0; x < samplesX; x++)
                {
                    int pixelX = clamped.xMin + Mathf.Min(clamped.width - 1, Mathf.FloorToInt((x + 0.5f) * clamped.width / samplesX));
                    Color color = ReadColor(frame, pixelX, pixelY);
                    Color.RGBToHSV(color, out float hue, out float sat, out float val);

                    float angle = hue * Mathf.PI * 2f;
                    hueSin += Mathf.Sin(angle);
                    hueCos += Mathf.Cos(angle);
                    satSum += sat;
                    valSum += val;

                    if (sat >= 0.5f)
                    {
                        paletteASum += color;
                        paletteACount++;
                    }
                    else
                    {
                        paletteBSum += color;
                        paletteBCount++;
                    }
                }
            }

            float avgHue = Mathf.Repeat(Mathf.Atan2(hueSin, hueCos) / (Mathf.PI * 2f), 1f);
            float avgSat = satSum / sampleCount;
            float avgVal = valSum / sampleCount;
            float hueMagnitude = Mathf.Sqrt(hueSin * hueSin + hueCos * hueCos) / sampleCount;
            float hueVariance = 1f - Mathf.Clamp01(hueMagnitude);

            Color paletteA = paletteACount > 0 ? paletteASum / paletteACount : ReadColor(frame, clamped.center.x, clamped.center.y);
            Color paletteB = paletteBCount > 0 ? paletteBSum / paletteBCount : paletteA;

            return new PixelStats(avgHue, avgSat, avgVal, hueVariance, paletteA, paletteB);
        }

        private static RectInt ClampRegion(in CameraImageFrame frame, RectInt region)
        {
            RectInt source = region.width > 0 && region.height > 0
                ? region
                : new RectInt(0, 0, frame.Width, frame.Height);

            int xMin = Mathf.Clamp(source.xMin, 0, frame.Width);
            int yMin = Mathf.Clamp(source.yMin, 0, frame.Height);
            int xMax = Mathf.Clamp(source.xMax, xMin, frame.Width);
            int yMax = Mathf.Clamp(source.yMax, yMin, frame.Height);
            return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        private static Color ReadColor(in CameraImageFrame frame, float x, float y)
        {
            return ReadColor(frame, Mathf.FloorToInt(x), Mathf.FloorToInt(y));
        }

        private static Color ReadColor(in CameraImageFrame frame, int x, int y)
        {
            int clampedX = Mathf.Clamp(x, 0, frame.Width - 1);
            int clampedY = Mathf.Clamp(y, 0, frame.Height - 1);
            int index = (clampedY * frame.Width + clampedX) * 4;
            return new Color(
                frame.Rgba32[index] / 255f,
                frame.Rgba32[index + 1] / 255f,
                frame.Rgba32[index + 2] / 255f,
                frame.Rgba32[index + 3] / 255f);
        }
    }
}
