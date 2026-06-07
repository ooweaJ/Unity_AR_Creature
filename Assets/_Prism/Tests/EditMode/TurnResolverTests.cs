using NUnit.Framework;
using Prism.Data;
using Prism.Gameplay;
using UnityEngine;

namespace Prism.Tests.EditMode
{
    public sealed class TurnResolverTests
    {
        [Test]
        public void HasAdvantage_UsesLightweightTypeTable()
        {
            Assert.IsTrue(TurnResolver.HasAdvantage(ElementType.Fire, ElementType.Grass));
            Assert.IsTrue(TurnResolver.HasAdvantage(ElementType.Grass, ElementType.Water));
            Assert.IsTrue(TurnResolver.HasAdvantage(ElementType.Water, ElementType.Fire));
            Assert.IsTrue(TurnResolver.HasAdvantage(ElementType.Electric, ElementType.Water));
            Assert.IsFalse(TurnResolver.HasAdvantage(ElementType.Neutral, ElementType.Fire));
        }

        [Test]
        public void Resolve_AppliesDamageAndDownFlag()
        {
            CreatureData attackerData = Creature(1, ElementType.Fire, new CreatureStats(20, 5, 40));
            CreatureData defenderData = Creature(2, ElementType.Grass, new CreatureStats(10, 4, 12));
            CombatState attacker = new(attackerData, 40, ElementType.Fire);
            CombatState defender = new(defenderData, 12, ElementType.Grass);

            TurnOutcome outcome = new TurnResolver().Resolve(attacker, defender);

            Assert.GreaterOrEqual(outcome.Damage, 1);
            Assert.IsTrue(outcome.IsCritical);
            Assert.IsTrue(outcome.DefenderDown);
        }

        [Test]
        public void CombatState_ApplyDamage_ClampsHpAtZero()
        {
            CreatureData data = Creature(1, ElementType.Neutral, new CreatureStats(1, 1, 10));
            CombatState state = new(data, 5, ElementType.Neutral);

            CombatState next = state.ApplyDamage(50);

            Assert.AreEqual(0, next.CurrentHp);
        }

        private static CreatureData Creature(int seed, ElementType element, CreatureStats stats)
        {
            return new CreatureData(
                seed,
                $"ref_{seed}",
                "sample",
                element,
                Color.white,
                Color.black,
                stats,
                Rarity.Common,
                string.Empty);
        }
    }
}
