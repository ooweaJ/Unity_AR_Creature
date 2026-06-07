using Prism.Data;
using UnityEngine;

namespace Prism.Gameplay
{
    /// <summary>
    /// 3~5 타입 상성을 사용하는 경량 자동 턴 계산기입니다.
    /// </summary>
    public sealed class TurnResolver : ITurnResolver
    {
        private const float AdvantageMultiplier = 1.5f;
        private const float CriticalMultiplier = 1.25f;

        /// <inheritdoc />
        public TurnOutcome Resolve(in CombatState attacker, in CombatState defender)
        {
            CreatureStats attackStats = attacker.Source.Stats;
            CreatureStats defenseStats = defender.Source.Stats;
            int baseDamage = Mathf.Max(1, attackStats.Attack - Mathf.FloorToInt(defenseStats.Defense * 0.5f));

            bool hasAdvantage = HasAdvantage(attacker.Element, defender.Element);
            bool deterministicCritical = IsDeterministicCritical(attacker.Source.Seed, defender.Source.Seed, defender.CurrentHp);

            float multiplier = hasAdvantage ? AdvantageMultiplier : 1f;
            if (deterministicCritical)
            {
                multiplier *= CriticalMultiplier;
            }

            int damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * multiplier));
            return new TurnOutcome(damage, hasAdvantage || deterministicCritical, defender.CurrentHp - damage <= 0);
        }

        /// <summary>
        /// 공격 원소가 방어 원소에 우위인지 확인합니다.
        /// </summary>
        public static bool HasAdvantage(ElementType attacker, ElementType defender)
        {
            return attacker switch
            {
                ElementType.Fire => defender == ElementType.Grass,
                ElementType.Grass => defender == ElementType.Water,
                ElementType.Water => defender == ElementType.Fire,
                ElementType.Electric => defender == ElementType.Water,
                _ => false
            };
        }

        private static bool IsDeterministicCritical(int attackerSeed, int defenderSeed, int defenderHp)
        {
            unchecked
            {
                uint hash = (uint)attackerSeed;
                hash ^= (uint)(defenderSeed * 397);
                hash ^= (uint)(defenderHp * 17);
                hash ^= hash >> 13;
                hash *= 0x85EBCA6B;
                hash ^= hash >> 16;
                return hash % 10 == 0;
            }
        }
    }
}
