using Prism.Data;
using UnityEngine.XR.ARFoundation;

namespace Prism.AR
{
    /// <summary>
    /// ARSessionState를 Prism 세션 추적 상태로 변환합니다.
    /// </summary>
    public static class SessionTrackingStateMapper
    {
        /// <summary>
        /// ARSessionState 값을 SessionTracking으로 변환합니다.
        /// </summary>
        public static SessionTracking Map(ARSessionState state)
        {
            return state switch
            {
                ARSessionState.SessionTracking => SessionTracking.Tracking,
                ARSessionState.Ready or ARSessionState.SessionInitializing => SessionTracking.Limited,
                _ => SessionTracking.None
            };
        }
    }
}
