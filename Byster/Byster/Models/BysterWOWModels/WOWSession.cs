using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Byster.Utilities.WOWModels;

namespace Byster.Models.BysterWOWModels
{
    public class WOWSession
    {
        public WoW wowApp { get; set; }
        public string UserName { get; set; }
        public string ServerName { get; set; }
        public WOWClass SessionClass { get; set; }
        
        public WOWSession() { }

        public static WOWClasses ConverterOfClasses(Classes sessionClass)
        {
            Dictionary<Classes, WOWClasses> classDict = new Dictionary<Classes, WOWClasses>()
            {
                {Classes.WARRIOR, WOWClasses.Warrior },
                {Classes.WARLOCK, WOWClasses.Warlock },
                {Classes.SHAMAN, WOWClasses.Shaman },
                {Classes.ROGUE, WOWClasses.Robber },
                {Classes.PRIEST, WOWClasses.Priest },
                {Classes.PALADIN, WOWClasses.Paladin },
                {Classes.MAGE, WOWClasses.Wizard },
                {Classes.HUNTER, WOWClasses.Hunter },
                {Classes.DRUID, WOWClasses.Druid },
                {Classes.DEATHKNIGHT, WOWClasses.DeathKnight }
            };
            if(!classDict.ContainsKey(sessionClass))
            {
                return WOWClasses.ANY;
            }
            else
            {
                return classDict[sessionClass];
            }
        }

    }
}
