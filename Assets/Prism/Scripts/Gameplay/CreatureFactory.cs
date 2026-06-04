using System;
using Prism.Data;
using UnityEngine;

namespace Prism.Gameplay
{
    /// <summary>
    /// CreatureData를 실제 크리처 GameObject 인스턴스로 복원합니다.
    /// </summary>
    public static class CreatureFactory
    {
        private static CreatureDatabase database;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        /// <summary>
        /// 기본 데이터베이스를 설정합니다.
        /// </summary>
        public static void Configure(CreatureDatabase creatureDatabase)
        {
            database = creatureDatabase;
        }

        /// <summary>
        /// 설정된 기본 데이터베이스로 크리처를 생성합니다.
        /// </summary>
        public static ICreatureController Build(CreatureData data, Transform parent)
        {
            if (database == null)
            {
                throw new InvalidOperationException("CreatureFactory database is not configured.");
            }

            return Build(database, data, parent);
        }

        /// <summary>
        /// 지정 데이터베이스로 크리처를 생성합니다.
        /// </summary>
        public static ICreatureController Build(CreatureDatabase creatureDatabase, CreatureData data, Transform parent)
        {
            if (creatureDatabase == null)
            {
                throw new ArgumentNullException(nameof(creatureDatabase));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            CreatureArchetype archetype = creatureDatabase.FindArchetype(data.ArchetypeId);
            if (archetype == null || archetype.BasePrefab == null)
            {
                throw new InvalidOperationException($"Creature archetype or prefab not found: {data.ArchetypeId}");
            }

            GameObject instance = UnityEngine.Object.Instantiate(archetype.BasePrefab, parent);
            instance.name = $"Creature_{data.ReferenceImageId}_{data.Seed}";

            CreatureController controller = instance.GetComponent<CreatureController>();
            if (controller == null)
            {
                controller = instance.AddComponent<CreatureController>();
            }

            controller.Initialize(data, archetype.AnimationSet);
            ApplyTint(instance, data);
            return controller;
        }

        private static void ApplyTint(GameObject instance, CreatureData data)
        {
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            MaterialPropertyBlock block = new();

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                renderer.GetPropertyBlock(block);
                block.SetColor(BaseColorId, data.PrimaryTint);
                block.SetColor(ColorId, data.PrimaryTint);
                renderer.SetPropertyBlock(block);
            }
        }
    }
}
