using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

#if NET5_0_OR_GREATER
[assembly: SupportedOSPlatform("windows")]
#endif
namespace PrinterSetting;

public static class Extensions
{
    private const int PADDING_IA64 = 4;
    /// <summary>
    /// インストールされているプリンター名を取得します。
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<string> EnumeratePrinterName()
    {
        int returnCode;
        int level, sizeofstruct;
        // Note: Level 5 doesn't seem to work properly on NT platforms
        // (atleast the call to get the size of the buffer reqd.),
        // and Level 4 doesn't work on Win9x.
        //
        if (Environment.OSVersion.Platform == System.PlatformID.Win32NT)
        {
            level = 4;
            // PRINTER_INFO_4 are 12 bytes in size
            if (IntPtr.Size == 8)
                sizeofstruct = (IntPtr.Size * 2) + (Marshal.SizeOf(typeof(int)) * 1) + PADDING_IA64;
            else
                sizeofstruct = (IntPtr.Size * 2) + (Marshal.SizeOf(typeof(int)) * 1);
        }
        else
        {
            level = 5;
            // PRINTER_INFO_5 are 20 bytes in size
            sizeofstruct = (IntPtr.Size * 2) + (Marshal.SizeOf(typeof(int)) * 3);
        }
        string[] array;
        _ = NativeMethods.EnumPrinters(PrinterEnumFlags.PRINTER_ENUM_LOCAL | PrinterEnumFlags.PRINTER_ENUM_CONNECTIONS, null, level, IntPtr.Zero, 0, out int bufferSize, out _);

        IntPtr buffer = Marshal.AllocCoTaskMem(bufferSize);
        returnCode = NativeMethods.EnumPrinters(PrinterEnumFlags.PRINTER_ENUM_LOCAL | PrinterEnumFlags.PRINTER_ENUM_CONNECTIONS,
                                                        null, level, buffer,
                                                        bufferSize, out _, out int count);
        array = new string[count];

        if (returnCode == 0)
        {
            Marshal.FreeCoTaskMem(buffer);
            throw new Win32Exception();
        }

        for (int i = 0; i < count; i++)
        {
            // The printer name is at offset 0
            IntPtr namePointer = Marshal.ReadIntPtr((IntPtr)(checked((long)buffer + i * sizeofstruct)));
            array[i] = Marshal.PtrToStringAuto(namePointer);
        }

        Marshal.FreeCoTaskMem(buffer);

        return array;
    }

    /// <summary>
    /// 通常使うプリンター名を取得します。
    /// </summary>
    /// <returns></returns>
    public static string GetDefaultPrinterName()
    {
        int pcchBuffer = 0;
        if (NativeMethods.GetDefaultPrinter(null, ref pcchBuffer))
            return "";

        int lastWin32Error = Marshal.GetLastWin32Error();
        if (lastWin32Error == (int)ErrorCode.ERROR_INSUFFICIENT_BUFFER)
        {
            StringBuilder pszBuffer = new(pcchBuffer);
            if (NativeMethods.GetDefaultPrinter(pszBuffer, ref pcchBuffer))
                return pszBuffer.ToString();

            lastWin32Error = Marshal.GetLastWin32Error();
        }
        if (lastWin32Error == (int)ErrorCode.ERROR_FILE_NOT_FOUND)
            return "";

        throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    /// <summary>
    /// 通常使うプリンターを設定します。
    /// </summary>
    /// <param name="printerName"></param>
    /// <returns></returns>
    public static bool SetDefaultPrinter(string printerName) => NativeMethods.SetDefaultPrinter(printerName);

    private class NativeMethods
    {
        [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int EnumPrinters(PrinterEnumFlags flags, string name, int level, IntPtr pPrinterEnum/*buffer*/,
                                              int cbBuf, out int pcbNeeded, out int pcReturned);

        [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]

        internal static extern bool SetDefaultPrinter([MarshalAs(UnmanagedType.LPWStr), In] string name);
#nullable enable
        [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetDefaultPrinter(StringBuilder? pszBuffer, ref int pcchBuffer);
#nullable disable
    }
}
