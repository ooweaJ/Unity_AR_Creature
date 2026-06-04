using System;
using Prism.Data;
using UnityEngine;

namespace Prism.Gameplay
{
    /// <summary>
    /// 경량 자동 대전을 진행하고 연출 순서를 제어합니다.
    /// </summary>
    public interface IBattleDirector
    {
        /// <summary>
        /// 두 크리처의 자동 대전을 시작합니다.
        /// </summary>
        void StartBattle(CreatureData a, CreatureData b, Pose arenaPose);

        /// <summary>대전이 종료될 때 발생합니다.</summary>
        event Action<BattleResult> BattleEnded;
    }
}
