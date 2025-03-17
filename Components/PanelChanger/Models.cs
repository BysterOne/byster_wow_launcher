using System.Windows;

namespace ControlCenter.PanelChanger.Models
{
    #region CChangerItem
    public class CChangerItem<T> where T : Enum
    {
        public T Panel { get; set; }
        public UIElement Element { get; set; }

        public CChangerItem(T panelName, UIElement element)
        {
            Panel = panelName;
            Element = element;
        }
    }
    #endregion
}
