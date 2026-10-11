using UnityEngine;

namespace TopDown
{
    /// <summary>Smoothly keeps the camera centred on a target, preserving the camera's Z.</summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] float smoothTime = 0.12f;

        Vector3 velocity;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        void LateUpdate()
        {
            if (target == null)
                return;

            Vector3 goal = new Vector3(target.position.x, target.position.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
        }
    }
}
