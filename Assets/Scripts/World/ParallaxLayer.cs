using UnityEngine;

namespace TopDown
{
    /// <summary>
    /// Moves a background layer relative to the camera to fake depth (0 = fixed in the world, 1 = glued
    /// to the camera) and optionally scrolls a seamlessly tiling sprite, wrapping it to always cover the view.
    /// </summary>
    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] float parallax = 0.6f;
        [SerializeField] Vector2 scrollSpeed;
        [Tooltip("Size of one repetition of the tiled sprite in world units; zero disables wrapping.")]
        [SerializeField] Vector2 wrapSize;

        Transform cam;
        Vector3 origin;
        Vector2 scroll;

        public void Configure(float parallaxFactor, Vector2 speed, Vector2 tileSize)
        {
            parallax = parallaxFactor;
            scrollSpeed = speed;
            wrapSize = tileSize;
        }

        void Start()
        {
            origin = transform.position;
            if (Camera.main != null)
                cam = Camera.main.transform;
        }

        void LateUpdate()
        {
            if (cam == null)
                return;
            scroll += scrollSpeed * Time.deltaTime;
            Vector2 camPos = cam.position;
            Vector2 desired = (Vector2)origin + camPos * parallax + scroll;

            if (wrapSize.x > 0f && wrapSize.y > 0f)
            {
                // Keep the tiled sprite centred on the camera while preserving its pattern phase.
                Vector2 offset = desired - camPos;
                offset.x = Mathf.Repeat(offset.x, wrapSize.x) - wrapSize.x * 0.5f;
                offset.y = Mathf.Repeat(offset.y, wrapSize.y) - wrapSize.y * 0.5f;
                desired = camPos + offset;
            }

            transform.position = new Vector3(desired.x, desired.y, origin.z);
        }
    }
}
