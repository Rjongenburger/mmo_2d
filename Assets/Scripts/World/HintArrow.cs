using UnityEngine;

namespace TopDown
{
    /// <summary>OSRS tutorial-style yellow arrow bobbing above the next objective.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class HintArrow : MonoBehaviour
    {
        [SerializeField] float bobHeight = 0.18f;
        [SerializeField] float bobSpeed = 5f;

        SpriteRenderer arrow;
        Transform target;
        float height;

        void Awake()
        {
            arrow = GetComponent<SpriteRenderer>();
            arrow.enabled = false;
        }

        /// <summary>Points at a target, hovering just above its tallest sprite. Null hides the arrow.</summary>
        public void PointAt(Component targetComponent)
        {
            target = targetComponent != null ? targetComponent.transform : null;
            height = 0.6f;
            if (target != null)
            {
                float top = target.position.y;
                foreach (var r in target.GetComponentsInChildren<SpriteRenderer>())
                    top = Mathf.Max(top, r.bounds.max.y);
                height = top - target.position.y + 0.15f;
            }
        }

        void LateUpdate()
        {
            bool show = target != null && target.gameObject.activeInHierarchy;
            arrow.enabled = show;
            if (!show)
                return;
            float bob = Mathf.Abs(Mathf.Sin(Time.time * bobSpeed)) * bobHeight;
            transform.position = target.position + new Vector3(0f, height + bob, 0f);
        }
    }
}
