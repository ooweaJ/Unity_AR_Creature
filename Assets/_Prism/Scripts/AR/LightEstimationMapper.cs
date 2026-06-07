using Prism.Data;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Prism.AR
{
    /// <summary>
    /// AR Foundation 광원 추정 데이터를 Prism 데이터로 변환합니다.
    /// </summary>
    public static class LightEstimationMapper
    {
        /// <summary>
        /// ARLightEstimationData를 LightReading으로 변환합니다.
        /// </summary>
        public static LightReading Map(in ARLightEstimationData data)
        {
            float brightness = data.averageBrightness
                ?? data.averageMainLightBrightness
                ?? 1f;

            Color colorCorrection = data.colorCorrection ?? Color.white;
            Vector3? mainLightDirection = data.mainLightDirection;
            return new LightReading(brightness, colorCorrection, mainLightDirection);
        }
    }
}
