using System.Collections.Generic;

namespace NonsensicalKit.Setting
{
    public interface ISystemSettingProvider
    {
        string CategoryId { get; }
        string CategoryLabel { get; }
        int Order { get; }
        SettingTabKind TabKind { get; }

        IEnumerable<SettingItemDef> GetItems();
        bool IsSupported(string key);
        SettingValue ReadCurrent(string key);
        void Apply(string key, SettingValue value);
    }
}
