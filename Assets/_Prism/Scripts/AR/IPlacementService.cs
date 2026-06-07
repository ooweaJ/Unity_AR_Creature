using System;
using UnityEngine;

namespace Prism.AR
{
    /// <summary>
    /// 화면 좌표 기반 평면 배치와 앵커 생성을 제공합니다.
    /// </summary>
    public interface IPlacementService
    {
        /// <summary>
        /// 화면 좌표에서 감지된 평면을 레이캐스트해 월드 포즈를 반환합니다.
        /// </summary>
        bool TryRaycastPlane(Vector2 screenPos, out Pose pose);

        /// <summary>
        /// 지정 포즈에 앵커 핸들을 생성합니다.
        /// </summary>
        IAnchorHandle CreateAnchor(Pose pose);

        /// <summary>배치 가능한 평면 존재 여부가 바뀔 때 발생합니다.</summary>
        event Action<bool> PlaneAvailabilityChanged;
    }
}
