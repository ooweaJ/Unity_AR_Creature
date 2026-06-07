using Prism.Data;
using UnityEngine;

namespace Prism.Gameplay
{
    /// <summary>
    /// 픽셀 통계를 크리처 원소, 색상, 수치, 희귀도로 변환합니다.
    /// </summary>
    public sealed class TraitMapper : ITraitMapper
    {
        private const int MinAttack = 8;
        private const int MinDefense = 8;
        private const int MinHp = 30;
        private const int AttackRange = 18;
        private const int DefenseRange = 18;
        private const int HpRange = 45;

        /// <inheritdoc />
        public CreatureData Map(ReferenceImageBinding baseBinding, in PixelStats stats, int seed)
        {
            if (baseBinding == null)
            {
                throw new System.ArgumentNullException(nameof(baseBinding));
            }

            ElementType element = ResolveElement(baseBinding.BaseElement, stats);
            CreatureStats creatureStats = new(
                MinAttack + Mathf.RoundToInt(stats.AvgSat * AttackRange) + Roll(seed, 0, 3),
                MinDefense + Mathf.RoundToInt(stats.AvgVal * DefenseRange) + Roll(seed, 5, 3),
                MinHp + Mathf.RoundToInt(((stats.AvgSat + stats.AvgVal) * 0.5f) * HpRange) + Roll(seed, 11, 6));

            string archetypeId = baseBinding.Archetype != null ? baseBinding.Archetype.ArchetypeId : string.Empty;

            return new CreatureData(
                seed,
                baseBinding.ReferenceImageName,
                archetypeId,
                element,
                stats.PaletteA,
                stats.PaletteB,
                creatureStats,
                ResolveRarity(stats.HueVariance),
                string.Empty);
        }

        /// <summary>
        /// Hue와 기본 원소를 조합해 최종 원소를 계산합니다.
        /// </summary>
        public static ElementType ResolveElement(ElementType baseElement, in PixelStats stats)
        {
            if (baseElement != ElementType.Neutral && stats.AvgSat < 0.75f)
            {
                return baseElement;
            }

            float hue = Mathf.Repeat(stats.AvgHue, 1f);
            if (hue < 0.08f || hue >= 0.92f)
            {
                return ElementType.Fire;
            }

            if (hue < 0.20f)
            {
                return ElementType.Electric;
            }

            if (hue < 0.45f)
            {
                return ElementType.Grass;
            }

            if (hue < 0.70f)
            {
                return ElementType.Water;
            }

            return ElementType.Neutral;
        }

        /// <summary>
        /// Hue 분산으로 희귀도를 계산합니다.
        /// </summary>
        public static Rarity ResolveRarity(float hueVariance)
        {
            if (hueVariance >= 0.75f)
            {
                return Rarity.Legendary;
            }

            if (hueVariance >= 0.5f)
            {
                return Rarity.Epic;
            }

            if (hueVariance >= 0.25f)
            {
                return Rarity.Rare;
            }

            return Rarity.Common;
        }

        private static int Roll(int seed, int salt, int exclusiveMax)
        {
            unchecked
            {
                uint value = (uint)(seed + salt * 0x6D2B79F5);
                value ^= value >> 15;
                value *= 0x2C1B3C6D;
                value ^= value >> 12;
                value *= 0x297A2D39;
                value ^= value >> 15;
                return (int)(value % exclusiveMax);
            }
        }
    }
}
