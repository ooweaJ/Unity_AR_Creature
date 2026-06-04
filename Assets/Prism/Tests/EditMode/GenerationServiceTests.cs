using System.Reflection;
using NUnit.Framework;
using Prism.Data;
using Prism.Gameplay;
using UnityEngine;

namespace Prism.Tests.EditMode
{
    public sealed class GenerationServiceTests
    {
        [Test]
        public void Generate_UsesBindingSamplerAndMapper()
        {
            CreatureDatabase database = ScriptableObject.CreateInstance<CreatureDatabase>();
            ReferenceImageBinding binding = ScriptableObject.CreateInstance<ReferenceImageBinding>();
            CreatureArchetype archetype = ScriptableObject.CreateInstance<CreatureArchetype>();

            try
            {
                SetField(archetype, "archetypeId", "sample");
                SetField(binding, "referenceImageName", "poster");
                SetField(binding, "archetype", archetype);
                SetField(binding, "baseElement", ElementType.Neutral);
                SetField(database, "bindings", new[] { binding });

                FixedSampler sampler = new(new PixelStats(0.02f, 1f, 1f, 0f, Color.red, Color.black));
                TraitMapper mapper = new();
                GenerationService service = new(database, sampler, mapper, 4);

                CreatureData result = service.Generate(new GenerationRequest(
                    "poster",
                    new CameraImageFrame(1, 1, new byte[] { 255, 0, 0, 255 }),
                    new RectInt(0, 0, 1, 1)));

                Assert.AreEqual("poster", result.ReferenceImageId);
                Assert.AreEqual("sample", result.ArchetypeId);
                Assert.AreEqual(ElementType.Fire, result.Element);
                Assert.AreEqual(4, sampler.LastDownsample);
            }
            finally
            {
                Object.DestroyImmediate(database);
                Object.DestroyImmediate(binding);
                Object.DestroyImmediate(archetype);
            }
        }

        private sealed class FixedSampler : IPixelSampler
        {
            private readonly PixelStats stats;

            public int LastDownsample { get; private set; }

            public FixedSampler(PixelStats stats)
            {
                this.stats = stats;
            }

            public PixelStats Sample(in CameraImageFrame frame, RectInt region, int downsample = 32)
            {
                LastDownsample = downsample;
                return stats;
            }
        }

        private static void SetField<TTarget, TValue>(TTarget target, string fieldName, TValue value)
        {
            FieldInfo field = typeof(TTarget).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing field {typeof(TTarget).Name}.{fieldName}");
            field.SetValue(target, value);
        }
    }
}
