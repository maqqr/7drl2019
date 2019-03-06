using System.Collections.Generic;

namespace GoblinKing.Data
{
    public class PerkValue
    {
        public string Name;

        public float NumberValue;
        public bool BoolValue;
    }

    public class Perk
    {
        public string Name;
        public string Description;
        public string Requirement;

        public Dictionary<string, PerkValue> Gains;
    }
}