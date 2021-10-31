using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Byster.Models.RestModels;

namespace Byster.Models.BysterWOWModels
{
    public class RotationWOW
    {
        public int Id { get; set; }
        public string ExpiringTime { get; set; }
        public WOWClass RotationClass { get; set; }
        public string Name { get; set; }
        public RotationRole RoleOfRotation { get; set; }
        public RotationSpecialization SpecOfRotation { get; set; }
        public string Type { get; set; }

        public RotationWOW() { }

        public RotationWOW(RotationResponse response)
        {
            Id = 1;
            ExpiringTime = DateTime.Parse(response.expired_date).ToString("dd.MM.yyyy HH:mm");
            RotationClass = new WOWClass(WOWClass.GetClassByName(response.rotation.klass));
            RoleOfRotation = new RotationRole();
            SpecOfRotation = new RotationSpecialization(RotationSpecialization.GetSpecByName(response.rotation.specialization));
            Type = response.rotation.type;
            Name = response.rotation.name;
        }

    }

    public class RotationRole
    {
        
        public string Name { get; set; }
        public string ImageUri { get; set; }

        public RotationRole() { }

        public static RotationRoles GetRoleByName(string name)
        {
            List<string> names = new List<string>()
            {
                "ANY",
                "DPS",
                "Tank",
                "Heal",
            };
            if(string.IsNullOrEmpty(name))
            {
                return RotationRoles.ANY;
            }
            else
            {
                return (RotationRoles)names.IndexOf(name);
            }
        }
    }
    public enum RotationRoles
    {
        ANY = 0,
        DPS = 1,
        Tank = 2,
        Heal = 3,
    }

    public class RotationSpecialization
    {
        
        public string Name { get; set; }
        public string ImageUri { get; set; }

        public static RotationSpecializations GetSpecByName(string name)
        {
            List<string> names = new List<string>
            {
                "ANY",
                "ARMS",
                "FURY",
                "PROTOWAR",
                "RETRIBUTION",
                "PROTOPAL",
                "HOLYPAL",
                "MM",
                "BM",
                "SURVIVABILITY",
                "COMBAT",
                "MUTILATION",
                "SUBTLETY",
                "DISCIPLINE",
                "HOLYPRIEST",
                "SHADOW",
                "BLOOD",
                "FROST",
                "UNHOLY",
                "ELEMENTAL",
                "ENHANCEMENT",
                "RESTORSHAMAN",
                "ARCANE",
                "FIRE",
                "FROSTMAGE",
                "AFFLICTION",
                "DEMONOLOGY",
                "DESTRUCTION",
                "FERAL",
                "MOONKIN",
                "RESTORDRUID",
            };
            if(string.IsNullOrEmpty(name))
            {
                return RotationSpecializations.ANY;
            }
            else
            {
                return (RotationSpecializations)names.IndexOf(name.ToUpper());
            }
        }

        public RotationSpecialization() { }

        public RotationSpecialization(RotationSpecializations spec)
        {
            List<string> names = new List<string>
            {
                "ANY",
                "ARMS",
                "FURY",
                "PROTOWAR",
                "RETRIBUTION",
                "PROTOPAL",
                "HOLYPAL",
                "MM",
                "BM",
                "SURVIVABILITY",
                "COMBAT",
                "MUTILATION",
                "SUBTLETY",
                "DISCIPLINE",
                "HOLYPRIEST",
                "SHADOW",
                "BLOOD",
                "FROST",
                "UNHOLY",
                "ELEMENTAL",
                "ENHANCEMENT",
                "RESTORSHAMAN",
                "ARCANE",
                "FIRE",
                "FROSTMAGE",
                "AFFLICTION",
                "DEMONOLOGY",
                "DESTRUCTION",
                "FERAL",
                "MOONKIN",
                "RESTORDRUID",
            };
            string rootUri = "/Resources/Images/Specializations/";

            Name = names[(int)spec];
            ImageUri = rootUri + Name + ".png";
            
        }
    }
    public enum RotationSpecializations
    {
        ANY = 0,
        //WARRIOR
        ARMS = 1,
        FURY = 2,
        PROTOWAR = 3,
        //PALADIN
        RETRIBUTION = 4,
        PROTOPAL = 5,
        HOLYPAL = 6,
        //HUNTER
        MM = 7,
        BM = 8,
        SURVIVABILITY = 9,
        //ROGUE
        COMBAT = 10,
        MUTILATION = 11,
        SUBTLETY = 12,
        //PRIEST
        DISCIPLINE = 13,
        HOLYPRIEST = 14,
        SHADOW = 15,
        //DEATHKNIGHT
        BLOOD = 16,
        FROST = 17,
        UNHOLY = 18,
        //SHAMAN
        ELEMENTAL = 19,
        ENHANCEMENT = 20,
        RESTORSHAMAN = 21,
        //MAGE
        ARCANE = 22,
        FIRE = 23,
        FROSTMAGE = 24,
        //WARLOCK
        AFFLICTION = 25,
        DEMONOLOGY = 26,
        DESTRUCTION = 27,
        //DRUID
        FERAL = 28,
        MOONKIN = 29,
        RESTORDRUID = 30,
    }
}