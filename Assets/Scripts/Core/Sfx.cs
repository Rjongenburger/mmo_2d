using UnityEngine;

namespace TopDown
{
    /// <summary>Fire-and-forget 2D sound effects.</summary>
    public static class Sfx
    {
        public static void Play(AudioClip clip, float volume = 1f, float pitchJitter = 0f)
        {
            if (clip == null)
                return;

            var go = new GameObject($"Sfx {clip.name}");
            var source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume;
            source.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            source.spatialBlend = 0f;
            source.Play();
            Object.Destroy(go, clip.length / source.pitch + 0.1f);
        }
    }
}
