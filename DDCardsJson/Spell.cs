using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCardsJson
{
    public class Spell
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Level { get; set; }
        public SpellSchool School { get; set; }
        public string Text { get; set; }
        public string Components
        {
            get
            {
                string temp = "";
                if (components.Contains('V'))
                {
                    temp += (temp.Length == 0 ? "" : ", ") + "V";
                }
                if (components.Contains('S'))
                {
                    temp += (temp.Length == 0 ? "" : ", ") + "S";
                }
                if (components.Contains('M'))
                {
                    temp += (temp.Length == 0 ? "" : ", ") + "M";
                }
                return temp;
            }
            set
            {
                components = value;
            }
        }
        public string Range { get; set; }
        public string CastingTime
        {
            get
            {
                string temp = castingTime;
                if (temp.ToUpper().Contains("RITUAL"))
                {
                    temp = temp.Replace("Ritual", "");
                    temp = temp.Trim() + " Ritual";
                }
                return temp;
            }
            set
            {
                castingTime = value;
            }
        }
        public string Duration
        {
            get
            {
                string temp = duration;
                if (temp.ToUpper().Contains("CONCENTRATION"))
                {
                    temp = temp.Replace("Concentration", "");
                    temp = temp.Trim() + " Concentration";
                }
                return temp;
            }
            set
            {
                duration = value;
            }
        }
        public string DamageEffect { get; set; }
        public bool IsCantrip { get { return Level.ToUpper().Contains("CAN"); } }
        public string Color
        {
            get
            {
                if (IsCantrip)
                {
                    return "dimgray";
                }
                else if (Level.Contains("1"))
                {
                    return "royalblue";
                }
                else if (Level.Contains("2"))
                {
                    return "Green";
                }
                else if (Level.Contains("3"))
                {
                    return "maroon";
                }
                else if (Level.Contains("4"))
                {
                    return "MidnightBlue";
                }
                else if (Level.Contains("5"))
                {
                    return "darkgoldenrod";
                }
                else if (Level.Contains("6"))
                {
                    return "OrangeRed";
                }
                else if (Level.Contains("7"))
                {
                    return "SteelBlue";
                }
                else if (Level.Contains("8"))
                {
                    return "Plum";
                }
                else if (Level.Contains("9"))
                {
                    return "Crimson";
                }
                return "black";
            }
            set
            {
                if (IsCantrip)
                {
                    color = "dimgray";
                }
                else if (Level.Contains("1"))
                {
                    color = "royalblue";
                }
                else if (Level.Contains("2"))
                {
                    color = "Green";
                }
                else if (Level.Contains("3"))
                {
                    color = "maroon";
                }
                else if (Level.Contains("4"))
                {
                    color = "MidnightBlue";
                }
                else if (Level.Contains("5"))
                {
                    color = "darkgoldenrod";
                }
            }
        }
        private string color;
        private string components;
        private string duration;
        private string castingTime;
    }
    public enum SpellSchool
    {
        Conjuration,
        Necromancy,
        Abjuration,
        Transmutation,
        Divination,
        Enchantment,
        Illusion,
    }
}

