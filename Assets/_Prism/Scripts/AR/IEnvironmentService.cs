using System;
using Prism.Data;

namespace Prism.AR
{
    /// <summary>
    /// AR 환경 정보(오클루전, 광원, 세션 추적 상태)를 제공합니다.
    /// </summary>
    public interface IEnvironmentService
    {
        /// <summary>현재 기기/세션에서 오클루전 기능을 사용할 수 있는지 여부입니다.</summary>
        bool OcclusionSupported { get; }

        /// <summary>
        /// 오클루전 기능 활성 상태를 설정합니다.
        /// </summary>
        void SetOcclusionEnabled(bool on);

        /// <summary>광원 추정값이 갱신될 때 발생합니다.</summary>
        event Action<LightReading> LightUpdated;

        /// <summary>AR 세션 추적 상태가 바뀔 때 발생합니다.</summary>
        event Action<SessionTracking> SessionTrackingChanged;
    }
}
