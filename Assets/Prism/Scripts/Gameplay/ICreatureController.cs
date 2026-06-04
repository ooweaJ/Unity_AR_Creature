using System;
using Prism.Data;

namespace Prism.Gameplay
{
    /// <summary>
    /// 배치된 크리처 1개의 행동과 연출 이벤트를 제어합니다.
    /// </summary>
    public interface ICreatureController
    {
        /// <summary>이 컨트롤러가 표현하는 크리처 데이터입니다.</summary>
        CreatureData Data { get; }

        /// <summary>
        /// 지정 행동을 재생합니다.
        /// </summary>
        void Play(ActionType action);

        /// <summary>행동이 시작될 때 발생합니다.</summary>
        event Action<ActionType> ActionStarted;

        /// <summary>행동이 끝났을 때 발생합니다.</summary>
        event Action<ActionType> ActionFinished;
    }
}
