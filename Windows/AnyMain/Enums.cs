using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Windows.AnyMain.Enums
{
    public enum EPC_MainShop
    {
        Main,
        Shop
    }

    public enum ERotationClass
    {
        Warrior,
        Druid,
        Priest,
        Mage,
        Hunter,
        Paladin,
        Rogue,
        DeathKnight,
        Warlock,  
        Shaman,
        Any
    }

    public enum ERotationType
    {
        PvP,
        PvE,
        Bot,
        Utility
    }

    public enum ERotationRole
    {
        Heal,
        Dps,
        Tank,
        Any
    }

    public enum EMediaType
    {
        Image,
        Video,
    }

    public enum EProductsType
    {        
        Bundle
    }

    public enum EServerIcon
    {
        Main,
        Devnet,
        Sernet
    }
}
