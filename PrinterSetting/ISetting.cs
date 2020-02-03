using System;
using System.ComponentModel;

namespace PrinterSetting
{
    public interface ISetting
    {
        [Description("プリンターの用紙設定をします")]
        bool SettingPage(PageOrientation pageOrientation, PaperSize paperSize);
    }
}
