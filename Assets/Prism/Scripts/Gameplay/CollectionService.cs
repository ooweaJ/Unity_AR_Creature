using System;
using System.Collections.Generic;
using System.IO;
using Prism.Data;
using UnityEngine;

namespace Prism.Gameplay
{
    /// <summary>
    /// CreatureData 컬렉션을 JsonUtility 형식으로 저장하고 로드합니다.
    /// </summary>
    public sealed class CollectionService : ICollectionService
    {
        private const string DefaultFileName = "prism_collection.json";

        private readonly string filePath;
        private readonly List<CreatureData> creatures = new();

        /// <summary>
        /// 기본 persistentDataPath 기반 컬렉션 서비스를 생성합니다.
        /// </summary>
        public CollectionService()
            : this(Path.Combine(Application.persistentDataPath, DefaultFileName))
        {
        }

        /// <summary>
        /// 지정 저장 경로를 사용하는 컬렉션 서비스를 생성합니다.
        /// </summary>
        public CollectionService(string filePath)
        {
            this.filePath = !string.IsNullOrEmpty(filePath)
                ? filePath
                : throw new ArgumentException("Collection file path is required.", nameof(filePath));
        }

        /// <inheritdoc />
        public IReadOnlyList<CreatureData> All => creatures;

        /// <inheritdoc />
        public event Action<CreatureData> CreatureAdded;

        /// <inheritdoc />
        public void Add(CreatureData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            creatures.Add(data);
            Save();
            CreatureAdded?.Invoke(data);
        }

        /// <inheritdoc />
        public bool Contains(string referenceImageId)
        {
            if (string.IsNullOrEmpty(referenceImageId))
            {
                return false;
            }

            for (int i = 0; i < creatures.Count; i++)
            {
                CreatureData data = creatures[i];
                if (data != null && string.Equals(data.ReferenceImageId, referenceImageId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public void Load()
        {
            creatures.Clear();

            if (!File.Exists(filePath))
            {
                return;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                CollectionSaveData saveData = JsonUtility.FromJson<CollectionSaveData>(json);
                if (saveData?.creatures == null)
                {
                    return;
                }

                creatures.AddRange(saveData.creatures);
            }
            catch (Exception)
            {
                creatures.Clear();
            }
        }

        private void Save()
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            CollectionSaveData saveData = new() { creatures = creatures };
            File.WriteAllText(filePath, JsonUtility.ToJson(saveData, true));
        }

        [Serializable]
        private sealed class CollectionSaveData
        {
            public List<CreatureData> creatures = new();
        }
    }
}
