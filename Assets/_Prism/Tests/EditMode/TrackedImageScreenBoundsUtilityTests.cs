using NUnit.Framework;
using Prism.AR;
using UnityEngine;

namespace Prism.Tests.EditMode
{
    public sealed class TrackedImageScreenBoundsUtilityTests
    {
        [Test]
        public void Calculate_ReturnsEmptyRectWhenCameraIsMissing()
        {
            RectInt result = TrackedImageScreenBoundsUtility.Calculate(null, Pose.identity, Vector2.one);

            Assert.AreEqual(new RectInt(), result);
        }

        [Test]
        public void Calculate_ReturnsPositiveRectForVisiblePose()
        {
            GameObject cameraObject = new("Bounds Test Camera");
            Camera camera = cameraObject.AddComponent<Camera>();

            try
            {
                camera.transform.position = Vector3.zero;
                camera.transform.rotation = Quaternion.identity;

                Pose pose = new(new Vector3(0f, 0f, 2f), Quaternion.identity);
                RectInt result = TrackedImageScreenBoundsUtility.Calculate(camera, pose, Vector2.one);

                Assert.Greater(result.width, 0);
                Assert.Greater(result.height, 0);
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
            }
        }
    }
}
