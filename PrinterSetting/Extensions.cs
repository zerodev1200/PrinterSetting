using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Management;
using System.Runtime.Versioning;

#if NET5_0_OR_GREATER
[assembly: SupportedOSPlatform("windows")]
#endif
namespace PrinterSetting;

public static class Extensions
{
    /// <summary>
    /// 通常使うプリンター名を取得します。
    /// </summary>
    /// <returns></returns>
    public static string GetDefaultPrinterName()
    {
        using var printDocument = new PrintDocument();
        return printDocument.PrinterSettings.PrinterName;
    }

    /// <summary>
    /// インストールされているプリンター名を取得します。
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<string> EnumeratePrinterName()
    {
        foreach (var p in PrinterSettings.InstalledPrinters)
        {
            yield return (string)p;
        }
    }

    /// <summary>
    /// 通常使うプリンター名を設定します。
    /// </summary>
    /// <param name="printerName"></param>
    /// <returns></returns>
    public static bool SetDefaultPrinter(string printerName)
    {
        //System.Managementの参照を追加する
        using var mos = new ManagementObjectSearcher("Select * from Win32_Printer");
        using var moc = mos.Get();

        foreach (ManagementObject mo in moc.Cast<ManagementObject>())
        {
            if (((string)mo["Name"]) == printerName)
            {
                ManagementBaseObject mbo = mo.InvokeMethod("SetDefaultPrinter", null, null);
                if (((uint)mbo["returnValue"]) != 0)
                    return false;
                return true;
            }
        }
        return false;
    }
}
