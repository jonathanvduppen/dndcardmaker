using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DDCardsJson
{
    class Program
    {
        public static string baseUrl = "https://www.dndbeyond.com/spells/";
        public static bool completed = false;
        public static IHtmlDocument websiteData;
        static void Main(string[] args)
        {
            string[] spells = null;
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\SpellsToImport.txt"))
            {
                spells = System.IO.File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\SpellsToImport.txt");
            }
            else
            {
                Console.WriteLine("Could not find file \"SpellsToImport.txt\"");
                Console.ReadLine();
                return;
            }
            List<Spell> spellList = new List<Spell>();
            foreach (string spellName in spells)
            {
                completed = false;
                spellList.Add(createSpell(spellName.Trim()));
            }
            string json = "[";
            foreach (Spell spell in spellList)
            {
                List<string> descriptions = splitDescription(spell.Description);
                for (int i = 0; i < descriptions.Count; i++)
                {
                    json += "{" +
                    "\n\"count\": 1" +
                    "\n,\"color\": \"" + spell.Color + "\"" +
                    "\n,\"title\": \"" + (descriptions.Count > 1 ? (spell.Name + " " + (i + 1).ToString() + "/" + (descriptions.Count).ToString()) : spell.Name) + "\"" +
                    "\n,\"icon\": \"white-book-0\"" +
                    "\n,\"icon_back\":\"robe\"" +
                    "\n,\"contents\": [" +
                        "\n\"subtitle | " + (spell.IsCantrip ? spell.School.ToString() + " " + "Cantrip" : spell.Level + " level " + spell.School.ToString()) + "\"" +
                        "\n,\"rule\"" +
                         "\n,\"property | Casting Time | " + spell.CastingTime + "\"" +
                         "\n,\"property | Range | " + spell.Range + "\"" +
                         "\n,\"property | Components | " + spell.Components + "\"" +
                         "\n,\"property | Duration | " + spell.Duration + "\"" +
                        "\n,\"rule\"" +
                        "\n,\"text | " + descriptions[i] + "\"" +
                    "\n]" +
                    "\n,\"tags\": [\n\"spell\"\n,\"mage\"]" +
                    "\n},";
                    Console.WriteLine("Export spell " + spell.Name);
                }
            }
            json = json.Substring(0, json.Length - 1);
            json += "]";
            /*StreamWriter file = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\spellCards.JSON");
            file.WriteLine(json);
            file.Close();*/
            string fileName = spellList.Count() + "_spells" + DateTime.Now.ToString("yyyyMMddhhmm") + ".JSON";
            System.IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "//" + fileName, json);
            Console.WriteLine(fileName + " created");
            //System.Diagnostics.Process.Start("http://crobi.github.io/rpg-cards/generator/generate");
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }
        public static Spell createSpell(string spellName)
        {
            Spell spell = new Spell();
            ScrapeWebsite(baseUrl + spellName.Replace(" ", "-").ToLower());
            while (!completed)
            {
                Thread.Sleep(1000);
            }
            spell.Level = GetSpellProperty(PropertyClass.Level);
            spell.CastingTime = GetSpellProperty(PropertyClass.CastingTime);
            spell.Range = GetSpellProperty(PropertyClass.RangeArea);
            spell.Components = GetSpellProperty(PropertyClass.Components);
            spell.Duration = GetSpellProperty(PropertyClass.Duration);
            spell.School = SetSpellSchool(GetSpellProperty(PropertyClass.School));
            spell.DamageEffect = GetSpellProperty(PropertyClass.DamageEffect);
            spell.Name = GetSpellProperty(PropertyClass.Name);
            spell.Description = GetSpellProperty(PropertyClass.Description);
            Console.WriteLine("Created spell " + spell.Name);
            return spell;
        }
        public static async void ScrapeWebsite(string siteUrl)
        {
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage request = await httpClient.GetAsync(siteUrl);
            cancellationToken.Token.ThrowIfCancellationRequested();

            Stream response = await request.Content.ReadAsStreamAsync();
            cancellationToken.Token.ThrowIfCancellationRequested();

            HtmlParser parser = new HtmlParser();
            websiteData = parser.ParseDocument(response);
            completed = true;
        }

        public static string stripTags(string innerHtml)
        {
            string returnValue = innerHtml.ToString().Trim();
            while (returnValue.Contains('<'))
            {
                if (returnValue.Contains('>'))
                {
                    returnValue = returnValue.Substring(0, returnValue.IndexOf('<')) + returnValue.Substring(returnValue.IndexOf('>') + 1);
                }
                else
                {
                    returnValue = returnValue.Substring(0, returnValue.IndexOf('<'));
                }
            }
            while (returnValue.Contains("  "))
            {
                returnValue = returnValue.Replace("  ", "");
            }
            return returnValue.ToString().Trim().Replace("\"", "\\\"").Replace("\n", " ");
        }
        public static string GetSpellProperty(PropertyClass property)
        {
            foreach (IElement element in websiteData.All.Where(x => x.ClassName == property.Value))
            {
                if (property.Value == PropertyClass.Description.Value)
                {
                    string description = "";
                    foreach (IElement child in element.Children)
                    {
                        description += stripTags(child.InnerHtml);
                    }
                    return description;
                }
                else if (element.Children.Length >= 2)
                {
                    if (element.Children[1].FirstElementChild != null && element.Children[1].FirstElementChild.InnerHtml != null)
                    {
                        return stripTags(element.Children[1].InnerHtml).Trim() + " " + stripTags(element.Children[1].FirstElementChild.InnerHtml).Trim();
                    }
                    else
                    {
                        return stripTags(element.Children[1].InnerHtml);
                    }
                }
                else
                {
                    return stripTags(element.InnerHtml);
                }
            }
            return "";
        }

        public class PropertyClass
        {
            private PropertyClass(string value) { Value = value; }

            public string Value { get; set; }

            public static PropertyClass Level { get { return new PropertyClass("ddb-statblock-item ddb-statblock-item-level"); } }
            public static PropertyClass CastingTime { get { return new PropertyClass("ddb-statblock-item ddb-statblock-item-casting-time"); } }
            public static PropertyClass RangeArea { get { return new PropertyClass("ddb-statblock-item ddb-statblock-item-range-area"); } }
            public static PropertyClass Components { get { return new PropertyClass("ddb-statblock-item ddb-statblock-item-components"); } }
            public static PropertyClass Duration { get { return new PropertyClass("ddb-statblock-item ddb-statblock-item-duration"); } }
            public static PropertyClass School { get { return new PropertyClass("ddb-statblock-item ddb-statblock-item-school"); } }
            public static PropertyClass AttackSave { get { return new PropertyClass("ddb-statblock-item ddb-statblock-item-attack-save"); } }
            public static PropertyClass DamageEffect { get { return new PropertyClass("ddb-statblock-item ddb-statblock-item-damage-effect"); } }
            public static PropertyClass Name { get { return new PropertyClass("page-title"); } }
            public static PropertyClass Description { get { return new PropertyClass("more-info-content"); } }
        }

        public static SpellSchool SetSpellSchool(string school)
        {
            SpellSchool spellSchool = SpellSchool.Abjuration;
            switch (school.ToUpper().Trim())
            {
                case "CONJURATION":
                    spellSchool = SpellSchool.Conjuration;
                    break;
                case "NECROMANCY":
                    spellSchool = SpellSchool.Necromancy;
                    break;
                case "ABJURATION":
                    spellSchool = SpellSchool.Abjuration;
                    break;
                case "TRANSMUTATION":
                    spellSchool = SpellSchool.Transmutation;
                    break;
                case "DIVINATION":
                    spellSchool = SpellSchool.Divination;
                    break;
                case "ENCHANTMENT":
                    spellSchool = SpellSchool.Enchantment;
                    break;
                case "ILLUSION":
                    spellSchool = SpellSchool.Illusion;
                    break;
            }
            return spellSchool;
        }
        public static List<string> splitDescription(string description)
        {
            List<string> text = new List<string>();
            while (description.Length > 300)
            {
                int length = 300;
                string toAdd = "ZZ";
                bool skip = false;
                while (toAdd.Last() != ' ' && !skip)
                {
                    length++;
                    if (length >= description.Length)
                    {
                        skip = true;
                        length = description.Length;
                    }
                    toAdd = description.Substring(0, length);
                }
                text.Add(description.Substring(0, length));
                description = description.Substring(length, description.Length - length);
            }
            text.Add(description);
            return text;
        }
    }
}
