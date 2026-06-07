using System.IO;
using NUnit.Framework;
using Prism.Data;
using Prism.Gameplay;
using UnityEngine;

namespace Prism.Tests.EditMode
{
    public sealed class CollectionServiceTests
    {
        private string tempFilePath;

        [SetUp]
        public void SetUp()
        {
            tempFilePath = Path.Combine(Application.temporaryCachePath, $"prism_collection_test_{System.Guid.NewGuid():N}.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }

        [Test]
        public void Add_Save_Load_RoundTripsCreatureData()
        {
            CollectionService writer = new(tempFilePath);
            CreatureData creature = Creature("poster_a");

            writer.Add(creature);

            CollectionService reader = new(tempFilePath);
            reader.Load();

            Assert.AreEqual(1, reader.All.Count);
            Assert.AreEqual("poster_a", reader.All[0].ReferenceImageId);
            Assert.AreEqual(ElementType.Fire, reader.All[0].Element);
        }

        [Test]
        public void Load_MissingFile_ReturnsEmptyCollection()
        {
            CollectionService service = new(tempFilePath);

            service.Load();

            Assert.AreEqual(0, service.All.Count);
        }

        [Test]
        public void Load_CorruptFile_ReturnsEmptyCollection()
        {
            File.WriteAllText(tempFilePath, "{ not valid json");
            CollectionService service = new(tempFilePath);

            service.Load();

            Assert.AreEqual(0, service.All.Count);
        }

        [Test]
        public void Contains_ReturnsTrueForMatchingReferenceImageId()
        {
            CollectionService service = new(tempFilePath);
            service.Add(Creature("poster_b"));

            Assert.IsTrue(service.Contains("poster_b"));
            Assert.IsFalse(service.Contains("poster_c"));
        }

        private static CreatureData Creature(string referenceImageId)
        {
            return new CreatureData(
                7,
                referenceImageId,
                "sample",
                ElementType.Fire,
                Color.red,
                Color.black,
                new CreatureStats(10, 8, 30),
                Rarity.Common,
                "2026-06-04T00:00:00Z");
        }
    }
}
