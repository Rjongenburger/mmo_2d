using System;
using UnityEngine;

namespace TopDown
{
    public enum Skill { Magic }

    /// <summary>Per-player skill experience, using the OSRS experience curve.</summary>
    public class Skills : MonoBehaviour
    {
        static readonly int SkillCount = Enum.GetValues(typeof(Skill)).Length;

        [SerializeField] int[] experience = new int[SkillCount];

        public event Action<Skill, int> XpGained;
        public event Action<Skill, int> LevelledUp;

        public int GetXp(Skill skill) => experience[(int)skill];
        public int GetLevel(Skill skill) => XpTable.LevelForXp(GetXp(skill));

        public int TotalLevel
        {
            get
            {
                int total = 0;
                foreach (Skill s in Enum.GetValues(typeof(Skill)))
                    total += GetLevel(s);
                return total;
            }
        }

        public void AddXp(Skill skill, int amount)
        {
            int before = GetLevel(skill);
            experience[(int)skill] = Mathf.Min(XpTable.MaxXp, experience[(int)skill] + amount);
            XpGained?.Invoke(skill, amount);

            int after = GetLevel(skill);
            if (after > before)
            {
                GameLog.Add($"<#0000a0>Congratulations, you've just advanced your {skill} level. You are now level {after}.</>");
                LevelledUp?.Invoke(skill, after);
            }
        }

        void Awake()
        {
            if (experience == null || experience.Length != SkillCount)
                Array.Resize(ref experience, SkillCount);
        }
    }

    /// <summary>OSRS experience table: level L needs floor(1/4 * Σ_{l=1}^{L-1} floor(l + 300·2^(l/7))) XP.</summary>
    public static class XpTable
    {
        public const int MaxLevel = 99;
        public const int MaxXp = 200_000_000;

        static readonly int[] xpForLevel = Build();

        static int[] Build()
        {
            var table = new int[MaxLevel + 1];
            double points = 0;
            for (int level = 1; level <= MaxLevel; level++)
            {
                table[level] = (int)Math.Floor(points / 4);
                points += Math.Floor(level + 300 * Math.Pow(2, level / 7.0));
            }
            return table;
        }

        public static int XpForLevel(int level) => xpForLevel[Mathf.Clamp(level, 1, MaxLevel)];

        public static int LevelForXp(int xp)
        {
            int level = 1;
            while (level < MaxLevel && xp >= xpForLevel[level + 1])
                level++;
            return level;
        }
    }
}
