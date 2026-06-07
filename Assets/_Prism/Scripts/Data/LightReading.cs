using UnityEngine;

namespace Prism.Data
{
    /// <summary>
    /// AR 광원 추정값을 도메인 레이어가 사용할 수 있게 정리한 데이터입니다.
    /// </summary>
    public readonly struct LightReading
    {
        /// <summary>주변 밝기입니다. 범위는 0..1로 정규화합니다.</summary>
        public readonly float Brightness;

        /// <summary>색 보정값입니다.</summary>
        public readonly Color ColorCorrection;

        /// <summary>주광 방향입니다. 제공되지 않으면 null입니다.</summary>
        public readonly Vector3? MainLightDirection;

        /// <summary>
        /// 광원 추정 데이터를 생성합니다.
        /// </summary>
        public LightReading(float brightness, Color colorCorrection, Vector3? mainLightDirection)
        {
            Brightness = Mathf.Clamp01(brightness);
            ColorCorrection = colorCorrection;
            MainLightDirection = mainLightDirection;
        }
    }
}
