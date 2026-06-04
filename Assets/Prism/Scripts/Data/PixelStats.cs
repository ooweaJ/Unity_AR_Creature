using UnityEngine;

namespace Prism.Data
{
    /// <summary>
    /// 생성 파이프라인에서 사용하는 픽셀 통계값입니다.
    /// </summary>
    public readonly struct PixelStats
    {
        /// <summary>평균 Hue 값입니다. 범위는 0..1입니다.</summary>
        public readonly float AvgHue;

        /// <summary>평균 Saturation 값입니다. 범위는 0..1입니다.</summary>
        public readonly float AvgSat;

        /// <summary>평균 Value 값입니다. 범위는 0..1입니다.</summary>
        public readonly float AvgVal;

        /// <summary>Hue 분산 근사값입니다. 범위는 0..1입니다.</summary>
        public readonly float HueVariance;

        /// <summary>대표 팔레트 색상 A입니다.</summary>
        public readonly Color PaletteA;

        /// <summary>대표 팔레트 색상 B입니다.</summary>
        public readonly Color PaletteB;

        /// <summary>
        /// 픽셀 통계를 생성합니다.
        /// </summary>
        public PixelStats(float avgHue, float avgSat, float avgVal, float hueVariance, Color paletteA, Color paletteB)
        {
            AvgHue = Mathf.Repeat(avgHue, 1f);
            AvgSat = Mathf.Clamp01(avgSat);
            AvgVal = Mathf.Clamp01(avgVal);
            HueVariance = Mathf.Clamp01(hueVariance);
            PaletteA = paletteA;
            PaletteB = paletteB;
        }
    }
}
