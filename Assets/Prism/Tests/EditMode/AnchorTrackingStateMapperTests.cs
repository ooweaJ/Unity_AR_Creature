using NUnit.Framework;
using Prism.AR;
using Prism.Data;
using UnityEngine.XR.ARSubsystems;

namespace Prism.Tests.EditMode
{
    public sealed class AnchorTrackingStateMapperTests
    {
        [Test]
        public void Map_ConvertsArFoundationTrackingState()
        {
            Assert.AreEqual(AnchorTrackingState.None, AnchorTrackingStateMapper.Map(TrackingState.None));
            Assert.AreEqual(AnchorTrackingState.Limited, AnchorTrackingStateMapper.Map(TrackingState.Limited));
            Assert.AreEqual(AnchorTrackingState.Tracking, AnchorTrackingStateMapper.Map(TrackingState.Tracking));
        }
    }
}
