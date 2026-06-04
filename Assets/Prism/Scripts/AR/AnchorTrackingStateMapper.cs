using Prism.Data;
using UnityEngine.XR.ARSubsystems;

namespace Prism.AR
{
    /// <summary>
    /// AR Foundation 추적 상태를 Prism 도메인 상태로 변환합니다.
    /// </summary>
    public static class AnchorTrackingStateMapper
    {
        /// <summary>
        /// TrackingState 값을 AnchorTrackingState로 변환합니다.
        /// </summary>
        public static AnchorTrackingState Map(TrackingState state)
        {
            return state switch
            {
                TrackingState.Tracking => AnchorTrackingState.Tracking,
                TrackingState.Limited => AnchorTrackingState.Limited,
                _ => AnchorTrackingState.None
            };
        }
    }
}
