using System.Runtime.InteropServices;

namespace PrinterSetting;

[ProgId("PrinterSetting.Setting")]
public class Setting : ISetting
{
    readonly string _printerName;

    public Setting(string printerName)
    {
        _printerName = printerName;
    }

    /// <summary>
    /// 印刷の向き、出力用紙サイズを設定します。
    /// </summary>
    /// <param name="pageOrientation">印刷の向き</param>
    /// <param name="paperSize">出力用紙サイズ</param>
    public bool SettingPage(PageOrientation pageOrientation, PaperSize paperSize = PaperSize.A4)
    {
        var printerSetting = new PrinterSetting()
        {
            Size = paperSize,
            Orientation = pageOrientation
        };
        Printer.SetPrinterSetting(_printerName, printerSetting);
        return true;
    }
}
