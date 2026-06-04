using NUnit.Framework;
using Prism.Data;
using Prism.Gameplay;

namespace Prism.Tests.EditMode
{
    public sealed class CreatureActionAnimatorParametersTests
    {
        [TestCase(ActionType.Spawn, "spawn")]
        [TestCase(ActionType.Capture, "capture")]
        [TestCase(ActionType.Tap, "tap")]
        [TestCase(ActionType.Drag, "drag")]
        [TestCase(ActionType.Curious, "curious")]
        [TestCase(ActionType.BattleReady, "battleStart")]
        [TestCase(ActionType.Attack, "attack")]
        [TestCase(ActionType.Hit, "hit")]
        [TestCase(ActionType.Win, "win")]
        [TestCase(ActionType.Faint, "faint")]
        [TestCase(ActionType.Idle, "idle")]
        public void GetTrigger_ReturnsExpectedAnimatorTrigger(ActionType action, string expected)
        {
            Assert.AreEqual(expected, CreatureActionAnimatorParameters.GetTrigger(action));
        }

        [Test]
        public void UsesMovingBool_IsTrueOnlyForDrag()
        {
            Assert.IsTrue(CreatureActionAnimatorParameters.UsesMovingBool(ActionType.Drag));
            Assert.IsFalse(CreatureActionAnimatorParameters.UsesMovingBool(ActionType.Tap));
        }
    }
}
