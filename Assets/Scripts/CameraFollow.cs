using UnityEngine;

namespace TopDown
{
    /// <summary>
    /// Smoothly keeps the camera centred on a target, preserving the camera's Z.
    /// Optionally clamps the view so it never shows past the given world bounds.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] float smoothTime = 0.12f;
        [SerializeField] bool clampToBounds;
        [Tooltip("World-space area the camera view should stay inside (e.g. the room including its walls).")]
        [SerializeField] Rect bounds;

        Camera cam;
        Vector3 velocity;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        public void SetBounds(Rect worldBounds)
        {
            bounds = worldBounds;
            clampToBounds = true;
        }

        void Awake()
        {
            cam = GetComponent<Camera>();
        }

        void LateUpdate()
        {
            if (target == null)
                return;

            Vector3 goal = ClampToBounds(new Vector3(target.position.x, target.position.y, transform.position.z));
            transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
        }

        Vector3 ClampToBounds(Vector3 position)
        {
            if (!clampToBounds || !cam.orthographic)
                return position;

            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;
            position.x = ClampAxis(position.x, bounds.xMin, bounds.xMax, halfWidth);
            position.y = ClampAxis(position.y, bounds.yMin, bounds.yMax, halfHeight);
            return position;
        }

        // If the view is larger than the bounds on an axis, centre it instead of clamping.
        static float ClampAxis(float value, float min, float max, float halfExtent) =>
            max - min <= halfExtent * 2f ? (min + max) * 0.5f : Mathf.Clamp(value, min + halfExtent, max - halfExtent);
    }
}
