using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Byster.Localizations.Tools;


namespace Byster.Models.BysterModels
{
    public class ClassWOW
    {
        public static List<ClassWOW> AllClasses { get; set; }
        static ClassWOW()
        {
            AllClasses = new List<ClassWOW>();
            for(int i = 0; i < 11; i++)
            {
                AllClasses.Add(new ClassWOW((WOWClasses)i));
            }
        }
        public static ClassWOW GetClassByEnumClass(WOWClasses enumClass)
        {
            return AllClasses.Where(_class => _class.EnumWOWClass == enumClass).FirstOrDefault();
        }

        public string ImageUri { get; private set; }
        public string NameOfClass { get; private set; }
        public WOWClasses EnumWOWClass { get; private set; }
        

        public ClassWOW() { }

        public ClassWOW(WOWClasses WOWClassID)
        {
            List<string> names = new List<string>()
            {
                Localizator.GetLocalizationResourceByKey("ANYClass"),
                Localizator.GetLocalizationResourceByKey("WarriorClass"),
                Localizator.GetLocalizationResourceByKey("DruidClass"),
                Localizator.GetLocalizationResourceByKey("PriestClass"),
                Localizator.GetLocalizationResourceByKey("WizardClass"),
                Localizator.GetLocalizationResourceByKey("HunterClass"),
                Localizator.GetLocalizationResourceByKey("PaladinClass"),
                Localizator.GetLocalizationResourceByKey("RobberClass"),
                Localizator.GetLocalizationResourceByKey("DeathKnightClass"),
                Localizator.GetLocalizationResourceByKey("WarlockClass"),
                Localizator.GetLocalizationResourceByKey("ShamanClass")
            };
            List<string> uris = new List<string>()
            {
                "Wow.png",
                "Warrior.png",
                "Droid.png",
                "Priest.png",
                "Wizard.png",
                "Hunter.png",
                "Paladin.png",
                "Robber.png",
                "DeathKnight.png",
                "Warlock.png",
                "Shaman.png",
            };
            string rootUri = "/Resources/Images/ClassesWOW/";

            EnumWOWClass = WOWClassID;
            NameOfClass = names[(int)WOWClassID];
            ImageUri = rootUri + uris[(int)WOWClassID];
        }
        public static WOWClasses GetClassByName(string className)
        {
            List<string> names = new List<string>()
            {
                "ANY",
                "WARRIOR",
                "DRUID",
                "PRIEST",
                "MAGE",
                "HUNTER",
                "PALADIN",
                "ROGUE",
                "DEATHKNIGHT",
                "WARLOCK",
                "SHAMAN"
            };

            if(string.IsNullOrEmpty(className))
            {
                return WOWClasses.ANY;
            }
            else
            {
                return (WOWClasses)names.IndexOf(className);
            }
        }
    }
    public enum WOWClasses
    {
        ANY = 0,
        Warrior = 1,
        Druid = 2,
        Priest = 3,
        Wizard = 4,
        Hunter = 5,
        Paladin = 6,
        Robber = 7,
        DeathKnight = 8,
        Warlock = 9,
        Shaman = 10,
    }
}
