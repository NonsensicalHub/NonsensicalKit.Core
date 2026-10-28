namespace NonsensicalKit.Setting
{
    public enum SettingItemType
    {
        Bool,
        Int,
        Float,
        String,
        Enum,
        Button,
        Info
    }

    public enum SettingApplyMode
    {
        Immediate,
        Restart
    }

    public enum SettingRiskLevel
    {
        Normal,
        Advanced,
        Danger
    }

    public enum SettingTabKind
    {
        System,
        Project,
        Advanced,
        Diagnostics
    }
}
