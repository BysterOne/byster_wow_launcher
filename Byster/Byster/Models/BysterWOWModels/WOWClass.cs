using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.BysterWOWModels
{
    public class WOWClass
    {
        public string ImageUri { get; private set; }
        public string NameOfClass { get; private set; }
        public enum WOWClasses
        {
            Default = 0,
            Warrior = 1,
            Droid = 2,
            Priest = 3,
            Wizard = 4,
            Hunter = 5,
            Paladin = 6,
            Robber = 7,
            DeathKnight = 8,
            Warlock = 9,
            Shaman = 10,
        }

        public WOWClass() { }

        public WOWClass(WOWClasses WOWClassID)
        {
            List<string> names = new List<string>()
            {
                "Не определено",
                "Воин",
                "Друид",
                "Жрец",
                "Маг",
                "Охотник",
                "Паладин",
                "Разбойник",
                "Рыцарь смерти",
                "Чернокнижник",
                "Шаман"
            };
            List<string> uris = new List<string>()
            {
                "None.png",
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
            NameOfClass = names[(int)WOWClassID];
            ImageUri = rootUri + uris[(int)WOWClassID];
        }
    }
}
