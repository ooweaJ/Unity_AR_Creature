using System;
using Prism.Data;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Prism.AR
{
    /// <summary>
    /// ARAnchor 컴포넌트를 IAnchorHandle로 감싼 구현체입니다.
    /// </summary>
    public sealed class AnchorHandle : IAnchorHandle
    {
        private readonly ARAnchor anchor;
        private AnchorTrackingState lastTrackingState;
        private bool disposed;

        /// <summary>
        /// 앵커 핸들을 생성합니다.
        /// </summary>
        public AnchorHandle(ARAnchor anchor)
        {
            this.anchor = anchor != null ? anchor : throw new ArgumentNullException(nameof(anchor));
            lastTrackingState = TrackingState;
        }

        /// <inheritdoc />
        public Transform Transform => anchor != null ? anchor.transform : null;

        /// <inheritdoc />
        public AnchorTrackingState TrackingState => anchor != null
            ? AnchorTrackingStateMapper.Map(anchor.trackingState)
            : AnchorTrackingState.None;

        /// <inheritdoc />
        public event Action<AnchorTrackingState> TrackingStateChanged;

        /// <summary>
        /// 현재 상태를 확인하고 변경 이벤트를 발행합니다.
        /// </summary>
        public void PollTrackingState()
        {
            AnchorTrackingState current = TrackingState;
            if (current == lastTrackingState)
            {
                return;
            }

            lastTrackingState = current;
            TrackingStateChanged?.Invoke(current);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            if (anchor != null)
            {
                UnityEngine.Object.Destroy(anchor.gameObject);
            }
        }
    }
}
