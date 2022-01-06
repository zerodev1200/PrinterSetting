namespace PrinterSetting;

public class Printer : IPrinter
{
    readonly string _printerName;

    public Printer(string printerName)
    {
        _printerName = printerName;
    }

    /// <summary>
    /// 印刷の向き、出力用紙サイズを設定します。
    /// </summary>
    /// <param name="pageOrientation">印刷の向き</param>
    /// <param name="paperSize">出力用紙サイズ</param>
    public void SetPageInfo(PageOrientation pageOrientation, PaperSize paperSize = PaperSize.A4)
    {
        var pageSetting = new PageSetting()
        {
            Size = paperSize,
            Orientation = pageOrientation
        };
        PrinterSetting.SetPrinterSetting(_printerName, pageSetting);
    }
}
