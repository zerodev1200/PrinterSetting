using System.ComponentModel;

namespace PrinterSetting;

public interface IPrinter
{
    [Description("プリンターの用紙設定をします")]
    void SetPageInfo(PageOrientation pageOrientation, PaperSize paperSize);
}
