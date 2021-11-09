using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Byster.Models.RestModels;
using Byster.Models.BysterModels;

namespace Byster.Models.BysterModels
{
    public class RotationBase
    {
        public int Id { get; set; }
        public ClassWOW RotationClass { get; set; }
        public string Name { get; set; }
        public string ImageUri { get; set; }
        public RotationRole RoleOfRotation { get; set; }
        public RotationSpecialization SpecOfRotation { get; set; }
        public string Type { get; set; }
        public List<Media> Medias { get; set; }
    }

    public class RotationWOW : RotationBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string property = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        public string ExpiringTime { get; set; }
        public RotationWOW() { }

        public RotationWOW(RestRotationWOW response)
        {
            Id = response.rotation.id;
            ExpiringTime = DateTime.Parse(response.expired_date).Year <= 2050 ? DateTime.Parse(response.expired_date).ToString("dd.MM.yyyy HH:mm") : "Навсегда";
            RotationClass = new ClassWOW(ClassWOW.GetClassByName(response.rotation.klass));
            RoleOfRotation = new RotationRole(RotationRole.GetRoleByName(response.rotation.role_type));
            SpecOfRotation = new RotationSpecialization(RotationSpecialization.GetSpecByName(response.rotation.specialization));
            Type = response.rotation.type;
            Name = response.rotation.name;

            Medias = new List<Media>();
            foreach(var restMedia in response.rotation.media)
            {
                Medias.Add(new Media(restMedia.url, Media.GetMediaTypeByName(restMedia.type)));
            }

            ImageUri = 
                SpecOfRotation.EnumRotationSpecialization != RotationSpecializations.NULL ? SpecOfRotation.ImageUri :
                Medias.Count > 0 ?                                                          Medias[0].Uri :
                Type.ToLower() == "bot" ?                                                   "/Resources/Images/bot-icon-default.png" :
                                                                                            "/Resources/Images/image-placeholder.jpg";
        }
    }

    public class ShopRotation : RotationBase
    {
        public ShopRotation() { }

        public ShopRotation(RestRotationShop rotation)
        {
            Id = rotation.id;
            RotationClass = new ClassWOW(ClassWOW.GetClassByName(rotation.klass));
            RoleOfRotation = new RotationRole(RotationRole.GetRoleByName(rotation.role_type));
            SpecOfRotation = new RotationSpecialization(RotationSpecialization.GetSpecByName(rotation.specialization));
            Type = rotation.type;
            Name = rotation.name;

            Medias = new List<Media>();
            foreach (var restMedia in rotation.media)
            {
                Medias.Add(new Media(restMedia.url, Media.GetMediaTypeByName(restMedia.type)));
            }

            ImageUri =
                SpecOfRotation.EnumRotationSpecialization != RotationSpecializations.NULL ? SpecOfRotation.ImageUri :
                Medias.Count > 0 ?                                                          Medias[0].Uri :
                                                                                            "/Resources/Images/image-placeholder.jpg";
        }
    }

    public class RotationRole
    {
        
        public string Name { get; set; }
        public string ImageUri { get; set; }
        public RotationRoles EnumRotationRole { get; set; }

        public RotationRole() { }

        public static RotationRoles GetRoleByName(string name)
        {
            List<string> names = new List<string>()
            {
                "ANY",
                "DPS",
                "TANK",
                "HEAL",
            };
            if(string.IsNullOrEmpty(name))
            {
                return RotationRoles.ANY;
            }
            else
            {
                return (RotationRoles)names.IndexOf(name.ToUpper());
            }
        }

        public RotationRole(RotationRoles role)
        {
            List<string> names = new List<string>()
            {
                "ANY",
                "DPS",
                "Tank",
                "Heal",
            };
            string rootUri = "/Resources/Images/Types/";
            Name = names[(int)role];
            ImageUri = rootUri + Name + ".png";
            EnumRotationRole = role;
        }
    }
    public enum RotationRoles
    {
        ANY = 0,
        DPS = 1,
        Tank = 2,
        Heal = 3,
        NULL = 4,
    }

    public class RotationSpecialization
    {

        public string Name { get; set; }
        public string ImageUri { get; set; }
        public RotationSpecializations EnumRotationSpecialization { get; set; }

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
                null,
            };
            if(string.IsNullOrEmpty(name))
            {
                return RotationSpecializations.NULL;
            }
            else
            {
                return (RotationSpecializations)names.IndexOf(name.ToUpper());
            }
        }

        public RotationSpecialization() { }

        public RotationSpecialization(RotationSpecializations spec)
        {
            EnumRotationSpecialization = spec;
            List<string> names = new List<string>
            {
                "ANY",
                "ARMS",
                "FURY",
                "PROTO",
                "RETRIBUTION",
                "PROTO",
                "HOLY",
                "MM",
                "BM",
                "SURVIVABILITY",
                "COMBAT",
                "MUTILATION",
                "SUBTLETY",
                "DISCIPLINE",
                "HOLY",
                "SHADOW",
                "BLOOD",
                "FROST",
                "UNHOLY",
                "ELEMENTAL",
                "ENHANCEMENT",
                "RESTOR",
                "ARCANE",
                "FIRE",
                "FROST",
                "AFFLICTION",
                "DEMONOLOGY",
                "DESTRUCTION",
                "FERAL",
                "MOONKIN",
                "RESTOR",
                null,
            };
            string rootUri = "/Resources/Images/Specializations/";

            Name = names[(int)spec];
            List<string> imageNames = new List<string>()
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
                null,
            };
            if(Name != null)
            {
                ImageUri = rootUri + imageNames[(int)spec] + ".png";

            }
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
        //NULL SPEC
        NULL = 31,
    }
}