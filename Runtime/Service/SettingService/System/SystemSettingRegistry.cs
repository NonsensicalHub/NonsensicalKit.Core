namespace NonsensicalKit.Setting
{
    public static class SystemSettingRegistry
    {
        public static void RegisterDefaults(SettingManager manager)
        {
            manager.RegisterProvider(new DisplaySettingProvider());
            manager.RegisterProvider(new QualitySettingProvider());
            manager.RegisterProvider(new AudioSettingProvider());
            manager.RegisterProvider(new PerformanceSettingProvider());
            manager.RegisterProvider(new AdvancedRuntimeProvider());
            manager.RegisterProvider(new DiagnosticsProvider());
        }
    }
}
