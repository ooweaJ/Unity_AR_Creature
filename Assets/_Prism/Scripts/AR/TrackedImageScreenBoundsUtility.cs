using UnityEngine;

namespace Prism.AR
{
    /// <summary>
    /// 트래킹 이미지의 월드 포즈와 물리 크기로 화면 영역을 계산합니다.
    /// </summary>
    public static class TrackedImageScreenBoundsUtility
    {
        /// <summary>
        /// 이미지 네 모서리를 화면 좌표로 투영해 픽셀 Rect를 계산합니다.
        /// </summary>
        public static RectInt Calculate(Camera camera, Pose worldPose, Vector2 physicalSize)
        {
            if (camera == null || physicalSize.x <= 0f || physicalSize.y <= 0f)
            {
                return new RectInt();
            }

            Vector3 halfRight = worldPose.rotation * Vector3.right * (physicalSize.x * 0.5f);
            Vector3 halfUp = worldPose.rotation * Vector3.up * (physicalSize.y * 0.5f);
            Vector3 center = worldPose.position;

            Vector3[] corners =
            {
                center - halfRight - halfUp,
                center - halfRight + halfUp,
                center + halfRight - halfUp,
                center + halfRight + halfUp
            };

            Vector3 first = camera.WorldToScreenPoint(corners[0]);
            float minX = first.x;
            float maxX = first.x;
            float minY = first.y;
            float maxY = first.y;

            for (int i = 1; i < corners.Length; i++)
            {
                Vector3 screen = camera.WorldToScreenPoint(corners[i]);
                minX = Mathf.Min(minX, screen.x);
                maxX = Mathf.Max(maxX, screen.x);
                minY = Mathf.Min(minY, screen.y);
                maxY = Mathf.Max(maxY, screen.y);
            }

            int x = Mathf.FloorToInt(minX);
            int y = Mathf.FloorToInt(minY);
            int width = Mathf.Max(0, Mathf.CeilToInt(maxX) - x);
            int height = Mathf.Max(0, Mathf.CeilToInt(maxY) - y);
            return new RectInt(x, y, width, height);
        }
    }
}
