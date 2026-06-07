using UnityEngine;

namespace Prism.Data
{
    /// <summary>
    /// AR 이미지 트래킹 결과를 도메인 레이어가 사용할 수 있는 값으로 변환한 정보입니다.
    /// </summary>
    public readonly struct TrackedImageInfo
    {
        /// <summary>XRReferenceImage 이름과 일치하는 바인딩 키입니다.</summary>
        public readonly string ReferenceImageName;

        /// <summary>트래킹된 이미지의 월드 포즈입니다.</summary>
        public readonly Pose WorldPose;

        /// <summary>화면 픽셀 샘플링에 사용할 스크린 영역입니다.</summary>
        public readonly RectInt ScreenBounds;

        /// <summary>AR Foundation 타입을 숨긴 트래킹 상태입니다.</summary>
        public readonly ImageTrackingState TrackingState;

        /// <summary>
        /// 트래킹 이미지 정보를 생성합니다.
        /// </summary>
        public TrackedImageInfo(
            string referenceImageName,
            Pose worldPose,
            RectInt screenBounds,
            ImageTrackingState trackingState)
        {
            ReferenceImageName = referenceImageName;
            WorldPose = worldPose;
            ScreenBounds = screenBounds;
            TrackingState = trackingState;
        }
    }
}
