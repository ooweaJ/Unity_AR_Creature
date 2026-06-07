using System;
using Prism.Data;

namespace Prism.AR
{
    /// <summary>
    /// 등록된 레퍼런스 이미지 인식 결과를 AR Foundation 타입 없이 발행합니다.
    /// </summary>
    public interface IImageTrackingService
    {
        /// <summary>이미지가 새로 인식되어 추적 가능한 상태가 되었을 때 발생합니다.</summary>
        event Action<TrackedImageInfo> ImageRecognized;

        /// <summary>이미지 추적이 상실되었거나 제거되었을 때 발생합니다.</summary>
        event Action<string> ImageTrackingLost;

        /// <summary>
        /// 이미지 트래킹 활성 상태를 설정합니다.
        /// </summary>
        void SetEnabled(bool on);
    }
}
