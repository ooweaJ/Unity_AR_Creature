using System.Reflection;
using NUnit.Framework;
using Prism.Data;
using UnityEngine;

namespace Prism.Tests.EditMode
{
    public sealed class CreatureDatabaseTests
    {
        [Test]
        public void FindBinding_ReturnsRegisteredBinding()
        {
            CreatureDatabase database = ScriptableObject.CreateInstance<CreatureDatabase>();
            ReferenceImageBinding binding = ScriptableObject.CreateInstance<ReferenceImageBinding>();

            try
            {
                SetField(binding, "referenceImageName", "poster_fire");
                SetField(database, "bindings", new[] { binding });

                Assert.AreSame(binding, database.FindBinding("poster_fire"));
            }
            finally
            {
                Object.DestroyImmediate(binding);
                Object.DestroyImmediate(database);
            }
        }

        [Test]
        public void FindBinding_ReturnsNullForMissingOrInvalidName()
        {
            CreatureDatabase database = ScriptableObject.CreateInstance<CreatureDatabase>();
            ReferenceImageBinding binding = ScriptableObject.CreateInstance<ReferenceImageBinding>();

            try
            {
                SetField(binding, "referenceImageName", "poster_water");
                SetField(database, "bindings", new[] { binding });

                Assert.IsNull(database.FindBinding("poster_fire"));
                Assert.IsNull(database.FindBinding(null));
                Assert.IsNull(database.FindBinding(string.Empty));
            }
            finally
            {
                Object.DestroyImmediate(binding);
                Object.DestroyImmediate(database);
            }
        }

        [Test]
        public void FindArchetype_ReturnsRegisteredArchetype()
        {
            CreatureDatabase database = ScriptableObject.CreateInstance<CreatureDatabase>();
            CreatureArchetype archetype = ScriptableObject.CreateInstance<CreatureArchetype>();

            try
            {
                SetField(archetype, "archetypeId", "sprout");
                SetField(database, "archetypes", new[] { archetype });

                Assert.AreSame(archetype, database.FindArchetype("sprout"));
            }
            finally
            {
                Object.DestroyImmediate(archetype);
                Object.DestroyImmediate(database);
            }
        }

        [Test]
        public void FindArchetype_ReturnsNullForMissingOrInvalidId()
        {
            CreatureDatabase database = ScriptableObject.CreateInstance<CreatureDatabase>();
            CreatureArchetype archetype = ScriptableObject.CreateInstance<CreatureArchetype>();

            try
            {
                SetField(archetype, "archetypeId", "spark");
                SetField(database, "archetypes", new[] { archetype });

                Assert.IsNull(database.FindArchetype("sprout"));
                Assert.IsNull(database.FindArchetype(null));
                Assert.IsNull(database.FindArchetype(string.Empty));
            }
            finally
            {
                Object.DestroyImmediate(archetype);
                Object.DestroyImmediate(database);
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
