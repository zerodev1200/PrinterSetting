using System.ComponentModel;

namespace PrinterSetting;

public interface ISetting
{
    [Description("プリンターの用紙設定をします")]
    void SetPage(PageOrientation pageOrientation, PaperSize paperSize);
}
