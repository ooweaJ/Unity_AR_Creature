using System;
using System.Collections.Generic;
using UnityEngine;

namespace Prism.Data
{
    /// <summary>
    /// 레퍼런스 이미지 바인딩과 크리처 아키타입을 조회하는 단일 카탈로그입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "CreatureDatabase", menuName = "Prism/Creature Database")]
    public sealed class CreatureDatabase : ScriptableObject
    {
        [SerializeField] private ReferenceImageBinding[] bindings = Array.Empty<ReferenceImageBinding>();
        [SerializeField] private CreatureArchetype[] archetypes = Array.Empty<CreatureArchetype>();

        /// <summary>등록된 레퍼런스 이미지 바인딩 목록입니다.</summary>
        public IReadOnlyList<ReferenceImageBinding> Bindings => bindings;

        /// <summary>등록된 크리처 아키타입 목록입니다.</summary>
        public IReadOnlyList<CreatureArchetype> Archetypes => archetypes;

        /// <summary>
        /// 레퍼런스 이미지 이름으로 바인딩을 찾습니다.
        /// </summary>
        public ReferenceImageBinding FindBinding(string referenceImageName)
        {
            if (string.IsNullOrEmpty(referenceImageName) || bindings == null)
            {
                return null;
            }

            for (int i = 0; i < bindings.Length; i++)
            {
                ReferenceImageBinding binding = bindings[i];
                if (binding != null && string.Equals(binding.ReferenceImageName, referenceImageName, StringComparison.Ordinal))
                {
                    return binding;
                }
            }

            return null;
        }

        /// <summary>
        /// 아키타입 ID로 크리처 아키타입을 찾습니다.
        /// </summary>
        public CreatureArchetype FindArchetype(string archetypeId)
        {
            if (string.IsNullOrEmpty(archetypeId) || archetypes == null)
            {
                return null;
            }

            for (int i = 0; i < archetypes.Length; i++)
            {
                CreatureArchetype archetype = archetypes[i];
                if (archetype != null && string.Equals(archetype.ArchetypeId, archetypeId, StringComparison.Ordinal))
                {
                    return archetype;
                }
            }

            return null;
        }
    }
}
