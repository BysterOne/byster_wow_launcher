using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Byster.Models.RestModels;
using Byster.Models.BysterModels;
using Byster.Localizations.Tools;

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
        public RotationType Type { get; set; }
        public string Description { get; set; }
        public List<Media> Medias { get; set; }
    }

    public class ActiveRotation : RotationBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool isVisibleInList = true;
        public bool IsVisibleInList
        {
            get { return isVisibleInList; }
            set
            {
                isVisibleInList = value;
                OnPropertyChanged("IsVisibleInList");
            }
        }

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        public string ExpiringTime { get; set; }
        public ActiveRotation() { }

        public ActiveRotation(RestRotationWOW response)
        {
            Id = response.rotation.id;
            ExpiringTime = DateTime.Parse(response.expired_date).Year <= 2050 ? DateTime.Parse(response.expired_date).ToString("dd.MM.yyyy HH:mm") : Localizations.Tools.Localizator.GetLocalizationResourceByKey("Forever").Value;
            RotationClass = ClassWOW.GetClassByEnumClass(ClassWOW.GetClassByName(response.rotation.klass));
            RoleOfRotation = RotationRole.GetRotationRoleByEnumRotationRole(RotationRole.GetRoleByName(response.rotation.role_type));
            SpecOfRotation = RotationSpecialization.GetRotationSpecializationByEnumRotationSpecialization(RotationSpecialization.GetSpecByName(response.rotation.specialization));
            Type = RotationType.GetRotationTypeByRotationTypes(RotationType.GetTypeByName(response.rotation.type));
            Name = response.rotation.name;
            Medias = null;
            ImageUri =
                SpecOfRotation.EnumRotationSpecialization != RotationSpecializations.NULL ? SpecOfRotation.ImageUri :
                Type.Name.ToLower() == "bot" ? "/Resources/Images/bot-icon-default.png" :
                Type.Name.ToLower() == "utility" ? "/Resources/Images/utility-icon-default.png" :
                "/Resources/Images/utility-icon-default.png";
        }
    }

    public class ShopRotation : RotationBase
    {
        public ShopRotation() { }

        public ShopRotation(RestRotationShop rotation)
        {
            Id = rotation.id;
            RotationClass = ClassWOW.GetClassByEnumClass(ClassWOW.GetClassByName(rotation.klass));
            RoleOfRotation = RotationRole.GetRotationRoleByEnumRotationRole(RotationRole.GetRoleByName(rotation.role_type));
            SpecOfRotation = RotationSpecialization.GetRotationSpecializationByEnumRotationSpecialization(RotationSpecialization.GetSpecByName(rotation.specialization));
            Type = RotationType.GetRotationTypeByRotationTypes(RotationType.GetTypeByName(rotation.type));
            Name = rotation.name;

            Description = Localizator.LoadedLocalizationInfo.Language == "Русский" ? rotation.description : rotation.description_en;
            Media prevMedia = null;
            Medias = new List<Media>();
            foreach (var restMedia in rotation.media)
            {
                Media currentMedia = new Media(restMedia.url, Media.GetMediaTypeByName(restMedia.type), prevMedia);
                Medias.Add(currentMedia);
                prevMedia = currentMedia;
            }

            ImageUri =
                SpecOfRotation.EnumRotationSpecialization != RotationSpecializations.NULL ? SpecOfRotation.ImageUri :
                Medias.Count > 0 ? Medias[0].Uri :
                                                                                            "/Resources/Images/image-placeholder.png";
        }
    }

    public class RotationType
    {
        public static List<RotationType> AllTypes { get; set; }
        static RotationType()
        {
            AllTypes = new List<RotationType>();
            for (int i = 0; i < 4; i++)
            {
                AllTypes.Add(new RotationType((RotationTypes)i));
            }
        }
        public static RotationType GetRotationTypeByRotationTypes(RotationTypes enumType)
        {
            return AllTypes.Where(_type => _type.EnumType == enumType).FirstOrDefault();
        }

        public RotationTypes EnumType { get; set; }
        public string Name { get; set; }
        public string ImageUri { get; set; }
        public RotationType(RotationTypes enumType)
        {
            EnumType = enumType;
            Name = enumType.ToString();
            ImageUri = "/Resources/Images/Types/" + Name + ".png";
        }

        public static RotationTypes GetTypeByName(string name)
        {
            for (int i = 0; i < 4; i++)
            {
                if (((RotationTypes)i).ToString().ToLower() == name.ToLower())
                {
                    return (RotationTypes)i;
                }
            }
            return RotationTypes.UNKNOWN;
        }
    }

    public enum RotationTypes
    {
        UNKNOWN = -1,
        PvE = 0,
        PvP = 1,
        Bot = 2,
        Utility = 3,
    }

    public class RotationRole
    {
        public static List<RotationRole> AllRotationRoles { get; set; }
        static RotationRole()
        {
            AllRotationRoles = new List<RotationRole>();
            for (int i = 0; i < 5; i++)
            {
                AllRotationRoles.Add(new RotationRole((RotationRoles)i));
            }
        }

        public static RotationRole GetRotationRoleByEnumRotationRole(RotationRoles enumRole)
        {
            return AllRotationRoles.Where(_role => _role.EnumRotationRole == enumRole).FirstOrDefault();
        }


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
                "placeholder",
            };
            if (string.IsNullOrEmpty(name))
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
                "placeholder",
            };
            string rootUri = "/Resources/Images/Roles/";
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
        public static List<RotationSpecialization> AllSpecializations { get; set; }
        static RotationSpecialization()
        {
            AllSpecializations = new List<RotationSpecialization>();
            for (int i = 0; i < 32; i++)
            {
                AllSpecializations.Add(new RotationSpecialization((RotationSpecializations)i));
            }
        }

        public static RotationSpecialization GetRotationSpecializationByEnumRotationSpecialization(RotationSpecializations enumSpec)
        {
            return AllSpecializations.Where(_spec => _spec.EnumRotationSpecialization == enumSpec).FirstOrDefault();
        }

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
            if (string.IsNullOrEmpty(name))
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
            if (Name != null)
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