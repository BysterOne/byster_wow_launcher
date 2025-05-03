using Launcher.Api.Models;
using Launcher.Windows.AnyMain.Enums;

namespace Launcher.Components.MainWindow.Any.PageShop.Models
{
    #region CFilters
    public class CFilters
	{
		public bool IsBundle { get; set; } = false;
		public List<ERotationType> Types { get; set; } = [];
		public List<ERotationClass> Classes { get; set; } = [];
    }
	#endregion
	#region CCartItem
	public class CCartItem
	{
		public Product Product { get; set; } = null!;
		public int Count { get; set; } = 1;
    }
    #endregion
    #region CServer
    public class CServer
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PathToExe { get; set; } = string.Empty;
        public EServerIcon Icon { get; set; }
    }
    #endregion
}
