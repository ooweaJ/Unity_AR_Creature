using System;
using Prism.Data;
using UnityEngine;

namespace Prism.AR
{
    /// <summary>
    /// AR 앵커를 외부 레이어가 사용할 수 있는 핸들로 감싼 인터페이스입니다.
    /// </summary>
    public interface IAnchorHandle : IDisposable
    {
        /// <summary>크리처 인스턴스를 자식으로 붙일 Transform입니다.</summary>
        Transform Transform { get; }

        /// <summary>현재 앵커 추적 상태입니다.</summary>
        AnchorTrackingState TrackingState { get; }

        /// <summary>앵커 추적 상태가 바뀔 때 발생합니다.</summary>
        event Action<AnchorTrackingState> TrackingStateChanged;
    }
}
