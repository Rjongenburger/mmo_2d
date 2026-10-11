using System;
using UnityEngine;

namespace TopDown
{
    /// <summary>Game messages shown in the chatbox ("You get some logs.").</summary>
    public static class GameLog
    {
        public static event Action<string> MessageAdded;

        public static void Add(string message) => MessageAdded?.Invoke(message);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => MessageAdded = null;
    }
}
