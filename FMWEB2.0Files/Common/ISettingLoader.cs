using AssetCare.Model.ViewModels;

namespace AssetCare.Common
{
    public interface ISettingLoader
    {
        ExportTableSettingsModel ReadSettings();
    }
}
