using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Localizations.Tools
{
    public class LocalizationDataForViewModels
    {
        public LocalizationResource Main { get; set; }
        public LocalizationResource Shop { get; set; }
        public LocalizationResource ShopCapitalized { get; set; }
        public LocalizationResource NoRotations { get; set; }
        public LocalizationResource AllAvailableRotations { get; set; }
        public LocalizationResource LoginToCharacter { get; set; }
        public LocalizationResource Start { get; set; }
        public LocalizationResource WaitLoadingEnd { get; set; }
        public LocalizationResource Classes { get; set; }
        public LocalizationResource Types { get; set; }
        public LocalizationResource ToBuy { get; set; }
        public LocalizationResource Settings { get; set; }
        public LocalizationResource Success { get; set; }
        public LocalizationResource ChangePassword { get; set; }
        public LocalizationResource LinkEmail { get; set; }
        public LocalizationResource ActivateCoupon { get; set; }
        public LocalizationResource ShowConsole { get; set; }
        public LocalizationResource Encryption { get; set; }
        public LocalizationResource SelectRotationBranch { get; set; }
        public LocalizationResource SelectLoadtype { get; set; }
        public LocalizationResource MenuOfSelectingRotations { get; set; }
        public LocalizationResource Confirmation { get; set; }
        public LocalizationResource Error { get; set; }
        public LocalizationResource Continue { get; set; }
        public LocalizationResource Cancel { get; set; }
        public LocalizationResource EndSum { get; set; }
        public LocalizationResource ToPaySum { get; set; }
        public LocalizationResource BonusPay { get; set; }
        public LocalizationResource SelectPayment { get; set; }
        public LocalizationResource SelectPaymentNotRequired { get; set; }
        public LocalizationResource ClearCart { get; set; }
        public LocalizationResource Raschet { get; set; }
        public LocalizationResource Sync { get; set; }
        public LocalizationResource Save { get; set; }
        public LocalizationResource Add { get; set; }
        public LocalizationResource LocalRotationSearch { get; set; }
        public LocalizationResource RotationSelect { get; set; }
        public LocalizationResource NameOfRotation { get; set; }
        public LocalizationResource Description { get; set; }
        public LocalizationResource RotationCreate { get; set; }
        public LocalizationResource OK { get; set; }
        public LocalizationResource ToCart { get; set; }
        public LocalizationResource Test { get; set; }
        public LocalizationResource Localization { get; set; }
        public LocalizationResource ReferalCode { get; set; }
        public LocalizationDataForViewModels()
        {
            Main = "Main";
            Shop = "Shop";
            ShopCapitalized = "ShopCapitalized";
            NoRotations = "NoRotations";
            AllAvailableRotations = "AllAvailableRotations";
            LoginToCharacter = "LoginToCharacter";
            Start = "Start";
            WaitLoadingEnd = "WaitLoadingEnd";
            Classes = "Classes";
            Types = "Types";
            ToBuy = "ToBuy";
            Settings = "Settings";
            Success = "Success";
            ChangePassword = "ChangePassword";
            LinkEmail = "LinkEmail";
            ActivateCoupon = "ActivateCoupon";
            ShowConsole = "ShowConsole";
            Encryption = "Encryption";
            SelectRotationBranch = "SelectRotationBranch";
            SelectLoadtype = "SelectLoadtype";
            MenuOfSelectingRotations = "MenuOfSelectingRotations";
            Confirmation = "Confirmarion";
            Error = "Error";
            Continue = "Continue";
            Cancel = "Cancel";
            EndSum = "EndSum";
            ToPaySum = "ToPaySum";
            BonusPay = "BonusPay";
            SelectPayment = "SelectPayment";
            SelectPaymentNotRequired = "SelectPaymentNotRequired";
            ClearCart = "ClearCart";
            Raschet = "Raschet";
            Sync = "Sync";
            Save = "Save";
            Add = "Add";
            LocalRotationSearch = "LocalRotationSearch";
            RotationSelect = "RotationSelect";
            NameOfRotation = "NameOfRotation";
            Description = "Description";
            RotationCreate = "RotationCreate";
            OK = "OK";
            ToCart = "ToCart";
            Test = "Test";
            Localization = "Localization";
            ReferalCode = "ReferalCode";
        }

        public LocalizationInfo LoadedLocalizationInfo
        {
            get
            {
                return Localizator.LoadedLocalizationInfo;
            }
            set
            {
                if(value.Id != Localizator.LoadedLocalizationInfo.Id)
                    Localizator.ReloadLocalization(value.LanguageCode);
            }
        }
        public LocalizationInfo[] Localizations
        {
            get
            {
                return LocalizationInfo.AllLocalizationInfos.ToArray(); 
            }
        }
    }
}
