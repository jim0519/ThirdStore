
using ThirdStoreCommon.Models.Setting;

namespace ThirdStoreBusiness.Setting
{
    public interface ISettingService
    {
        void InsertSetting(T_Setting setting);

        void UpdateSetting(T_Setting setting);

        void DeleteSetting(T_Setting setting);

        T LoadSetting<T>() where T : ISettings, new();

        T GetSettingByKey<T>(string key, T defaultValue = default(T));
    }
    public interface ISettings
    {

    }
}
