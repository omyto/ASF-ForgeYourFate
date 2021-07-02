using System;
using System.Collections.Generic;
using System.Linq;

namespace ForgeYourFateASF
{
    internal class Summer2021Badge
    {
        internal int Id { get; }
        internal string Name { get;  }
        internal int[] Choices { get; }

        internal Summer2021Badge(int id, string name, int[] choices) {
            Id = id;
            Name = name;
            Choices = choices;
        }
    }

    internal class BadgeScore
    {
        internal string Name { get; }
        internal int Min { get; }
        internal int Max { get; }

        internal BadgeScore(string name, int min, int max)
        {
            Name = name;
            Min = min;
            Max = max;
        }
    }

    internal class Summer2021BadgeUtils
    {
        private const int MaxGenre = 14;
        private static Dictionary<int, BadgeScore> Badges { get; } = new()
        {
            { 51, new BadgeScore("The Masked Avenger", 14, 16) },
            { 52, new BadgeScore("The Trailblazing Explorer", 17, 19) },
            { 53, new BadgeScore("The Gorilla Scientist", 20, 22) },
            { 54, new BadgeScore("The Paranormal Professor", 23, 25) },
            { 55, new BadgeScore("The Ghost Detective", 26, 28) }
        };

        internal static Summer2021Badge GetRandomSummer2021Badge()
        {
            Random random = new();
            var badgeIds = new List<int>(Badges.Keys);

            int id = badgeIds[random.Next(0, badgeIds.Count)];
            BadgeScore badgeScore = Badges[id];
            string name = badgeScore.Name;

            int score = random.Next(badgeScore.Min, badgeScore.Max + 1);
            int[] choices = Enumerable.Repeat(1, MaxGenre).ToArray();
            int remainScore = score - MaxGenre;
            for (int i = 0; i < remainScore; i++)
            {
                choices[i]++;
            }

            var finalChoices = choices.OrderBy(x => random.Next()).ToArray();
            return new Summer2021Badge(id, name, finalChoices);
        }
    }
}
