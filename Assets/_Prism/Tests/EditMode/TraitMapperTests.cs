using System.Reflection;
using NUnit.Framework;
using Prism.Data;
using Prism.Gameplay;
using UnityEngine;

namespace Prism.Tests.EditMode
{
    public sealed class TraitMapperTests
    {
        [Test]
        public void ResolveElement_MapsHueBoundaries()
        {
            Assert.AreEqual(ElementType.Fire, TraitMapper.ResolveElement(ElementType.Neutral, Stats(0.00f)));
            Assert.AreEqual(ElementType.Electric, TraitMapper.ResolveElement(ElementType.Neutral, Stats(0.12f)));
            Assert.AreEqual(ElementType.Grass, TraitMapper.ResolveElement(ElementType.Neutral, Stats(0.30f)));
            Assert.AreEqual(ElementType.Water, TraitMapper.ResolveElement(ElementType.Neutral, Stats(0.55f)));
            Assert.AreEqual(ElementType.Neutral, TraitMapper.ResolveElement(ElementType.Neutral, Stats(0.80f)));
        }

        [Test]
        public void Map_IsDeterministicForSameInput()
        {
            ReferenceImageBinding binding = ScriptableObject.CreateInstance<ReferenceImageBinding>();
            CreatureArchetype archetype = ScriptableObject.CreateInstance<CreatureArchetype>();

            try
            {
                SetField(archetype, "archetypeId", "sample");
                SetField(binding, "referenceImageName", "poster");
                SetField(binding, "archetype", archetype);
                SetField(binding, "baseElement", ElementType.Neutral);

                PixelStats stats = new(0.55f, 0.8f, 0.6f, 0.3f, Color.cyan, Color.blue);
                TraitMapper mapper = new();

                CreatureData first = mapper.Map(binding, stats, 1234);
                CreatureData second = mapper.Map(binding, stats, 1234);

                Assert.AreEqual(first.Seed, second.Seed);
                Assert.AreEqual(first.ReferenceImageId, second.ReferenceImageId);
                Assert.AreEqual(first.ArchetypeId, second.ArchetypeId);
                Assert.AreEqual(first.Element, second.Element);
                Assert.AreEqual(first.Rarity, second.Rarity);
                Assert.AreEqual(first.Stats.Attack, second.Stats.Attack);
                Assert.AreEqual(first.Stats.Defense, second.Stats.Defense);
                Assert.AreEqual(first.Stats.MaxHp, second.Stats.MaxHp);
            }
            finally
            {
                Object.DestroyImmediate(binding);
                Object.DestroyImmediate(archetype);
            }
        }

        private static PixelStats Stats(float hue)
        {
            return new PixelStats(hue, 1f, 1f, 0f, Color.white, Color.black);
        }

        private static void SetField<TTarget, TValue>(TTarget target, string fieldName, TValue value)
        {
            FieldInfo field = typeof(TTarget).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing field {typeof(TTarget).Name}.{fieldName}");
            field.SetValue(target, value);
        }
    }
}
