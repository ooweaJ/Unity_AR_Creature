using NUnit.Framework;
using Prism.AR;
using Prism.Data;
using UnityEngine.XR.ARFoundation;

namespace Prism.Tests.EditMode
{
    public sealed class SessionTrackingStateMapperTests
    {
        [Test]
        public void Map_ConvertsArSessionStates()
        {
            Assert.AreEqual(SessionTracking.None, SessionTrackingStateMapper.Map(ARSessionState.None));
            Assert.AreEqual(SessionTracking.None, SessionTrackingStateMapper.Map(ARSessionState.Unsupported));
            Assert.AreEqual(SessionTracking.Limited, SessionTrackingStateMapper.Map(ARSessionState.Ready));
            Assert.AreEqual(SessionTracking.Limited, SessionTrackingStateMapper.Map(ARSessionState.SessionInitializing));
            Assert.AreEqual(SessionTracking.Tracking, SessionTrackingStateMapper.Map(ARSessionState.SessionTracking));
        }
    }
}
