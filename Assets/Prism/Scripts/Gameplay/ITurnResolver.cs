using Prism.Data;

namespace Prism.Gameplay
{
    /// <summary>
    /// 한 턴의 전투 결과를 즉시 계산합니다.
    /// </summary>
    public interface ITurnResolver
    {
        /// <summary>
        /// 공격자와 방어자 상태로 턴 결과를 계산합니다.
        /// </summary>
        TurnOutcome Resolve(in CombatState attacker, in CombatState defender);
    }
}
