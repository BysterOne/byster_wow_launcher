namespace ControlCenter.PanelChanger.Enums
{
    #region EPanelState
    public enum EPanelState
    {
        Hidden,
        Showen
    }
    #endregion
    #region ECP_Login
    public enum ECP_Login
    {
        InputCode,
        WaitConfirm,
        LoginAccepted,
        LoginFail
    }
    #endregion
    #region EGlobalPanels
    public enum EGlobalPanels
    {
        Login,
        Updating,
        WorkStation,
        None,
        AccountsSettings,
    }
    #endregion
    #region ECP_MainWindows
    public enum ECP_MainWindows
    {
        Main,
        Settings,
        Script,
    }
    #endregion
    #region ECP_LAPErrorPanel
    public enum ECP_LAPErrorPanel
    {
        TimeOut,
        Rejected,
    }
    #endregion

}
