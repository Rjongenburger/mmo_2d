using UnityEngine;
using UnityEngine.Tilemaps;

namespace TopDown
{
    /// <summary>
    /// The tiles you can stand on. Anywhere else (the cloud sea between the peaks) the player falls.
    /// Scenes without one treat everything as solid ground.
    /// </summary>
    public class WalkableGround : MonoBehaviour
    {
        static WalkableGround instance;

        [SerializeField] Tilemap ground;

        void Awake() => instance = this;

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public static bool IsGround(Vector2 worldPoint) =>
            instance == null || instance.ground == null || instance.ground.HasTile(instance.ground.WorldToCell(worldPoint));

        /// <summary>True when the point and its surroundings within <paramref name="margin"/> are all ground.</summary>
        public static bool IsSafe(Vector2 worldPoint, float margin) =>
            IsGround(worldPoint) &&
            IsGround(worldPoint + new Vector2(margin, 0f)) && IsGround(worldPoint - new Vector2(margin, 0f)) &&
            IsGround(worldPoint + new Vector2(0f, margin)) && IsGround(worldPoint - new Vector2(0f, margin));
    }
}
