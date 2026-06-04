using Prism.Data;
using UnityEngine;

namespace Prism.Gameplay
{
    /// <summary>
    /// 생성 결과를 재현하기 위한 seed 계산 유틸리티입니다.
    /// </summary>
    public static class GenerationSeedUtility
    {
        /// <summary>
        /// 레퍼런스 이미지 이름과 양자화된 픽셀 통계로 결정적 seed를 계산합니다.
        /// </summary>
        public static int Calculate(string referenceImageName, in PixelStats stats)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + HashString(referenceImageName);
                hash = hash * 31 + Quantize(stats.AvgHue);
                hash = hash * 31 + Quantize(stats.AvgSat);
                hash = hash * 31 + Quantize(stats.AvgVal);
                hash = hash * 31 + Quantize(stats.HueVariance);
                hash = hash * 31 + QuantizeColor(stats.PaletteA);
                hash = hash * 31 + QuantizeColor(stats.PaletteB);
                return hash;
            }
        }

        private static int Quantize(float value)
        {
            return Mathf.RoundToInt(Mathf.Clamp01(value) * 1000f);
        }

        private static int QuantizeColor(Color color)
        {
            unchecked
            {
                int hash = 23;
                hash = hash * 31 + Quantize(color.r);
                hash = hash * 31 + Quantize(color.g);
                hash = hash * 31 + Quantize(color.b);
                return hash;
            }
        }

        private static int HashString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            unchecked
            {
                int hash = 216613626;
                for (int i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= 16777619;
                }

                return hash;
            }
        }
    }
}
