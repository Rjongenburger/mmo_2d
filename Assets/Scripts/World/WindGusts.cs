using UnityEngine;

namespace TopDown
{
    /// <summary>
    /// Strong mountain wind: gusts that push the player, wind streaks and blowing snow around the camera,
    /// and a howling ambience that swells with each gust.
    /// </summary>
    public class WindGusts : MonoBehaviour
    {
        [SerializeField] Vector2 direction = Vector2.right;
        [SerializeField] float basePush = 0.15f;
        [SerializeField] float gustPush = 0.75f;
        [SerializeField] float gustPeriod = 5f;
        [SerializeField] Sprite streakSprite;
        [SerializeField] Sprite snowBig;
        [SerializeField] Sprite snowSmall;
        [SerializeField] int streakCount = 22;
        [SerializeField] int snowCount = 60;
        [SerializeField] int sortingOrder = 400;
        [SerializeField] AudioSource windAudio;

        struct Mote
        {
            public Transform transform;
            public SpriteRenderer renderer;
            public float speed, wobble, phase;
            public bool streak;
        }

        Mote[] motes;
        Camera cam;
        float seed;

        /// <summary>Current gust strength 0..1, for things like flags that should react to the wind.</summary>
        public static float Gust { get; private set; }

        void Start()
        {
            cam = Camera.main;
            seed = Random.value * 100f;
            motes = new Mote[streakCount + snowCount];
            for (int i = 0; i < motes.Length; i++)
            {
                bool streak = i < streakCount;
                var go = new GameObject(streak ? "Wind Streak" : "Snow");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = streak ? streakSprite : (Random.value < 0.35f ? snowBig : snowSmall);
                sr.sortingOrder = sortingOrder;
                motes[i] = new Mote
                {
                    transform = go.transform,
                    renderer = sr,
                    streak = streak,
                    speed = streak ? Random.Range(9f, 14f) : Random.Range(2.5f, 5f),
                    wobble = streak ? Random.Range(0.05f, 0.2f) : Random.Range(0.2f, 0.6f),
                    phase = Random.value * 10f,
                };
                go.transform.position = RandomPointInView(0f);
            }
        }

        void Update()
        {
            if (cam == null)
                cam = Camera.main;
            float t = Time.time;
            float noise = Mathf.PerlinNoise(seed, t / gustPeriod);
            Gust = Mathf.Clamp01((noise - 0.3f) / 0.5f);

            if (Player.Instance != null)
                Player.Instance.Controller.ExternalVelocity = direction.normalized * (basePush + gustPush * Gust * Gust);

            if (windAudio != null)
            {
                windAudio.volume = 0.35f + 0.35f * Gust;
                windAudio.pitch = 0.92f + 0.16f * Gust;
            }

            if (cam == null || motes == null)
                return;
            float halfH = cam.orthographicSize + 1f;
            float halfW = cam.orthographicSize * cam.aspect + 1.5f;
            Vector2 center = cam.transform.position;
            Vector2 dir = direction.normalized;
            float speedScale = 0.6f + 0.8f * Gust;

            for (int i = 0; i < motes.Length; i++)
            {
                ref var m = ref motes[i];
                Vector3 p = m.transform.position;
                p += (Vector3)(dir * (m.speed * speedScale * Time.deltaTime));
                p.y += Mathf.Sin(t * 2f + m.phase) * m.wobble * Time.deltaTime * (m.streak ? 1f : 3f) - (m.streak ? 0f : 0.6f * Time.deltaTime);

                if (p.x > center.x + halfW || p.x < center.x - halfW || p.y < center.y - halfH || p.y > center.y + halfH)
                    p = new Vector3(center.x - halfW + Random.value * 0.5f, center.y + Random.Range(-halfH, halfH), 0f);

                m.transform.position = p;
                if (m.streak)
                    m.renderer.color = new Color(1f, 1f, 1f, 0.12f + 0.45f * Gust);
            }
        }

        Vector3 RandomPointInView(float z)
        {
            if (cam == null)
                cam = Camera.main;
            float halfH = cam != null ? cam.orthographicSize + 1f : 6f;
            float halfW = cam != null ? halfH * cam.aspect : 10f;
            Vector2 c = cam != null ? (Vector2)cam.transform.position : Vector2.zero;
            return new Vector3(c.x + Random.Range(-halfW, halfW), c.y + Random.Range(-halfH, halfH), z);
        }
    }
}
