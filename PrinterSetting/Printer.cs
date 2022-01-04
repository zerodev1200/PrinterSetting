using System;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace PrinterSetting;

internal class Printer
{
    [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "DocumentPropertiesA", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern int DocumentProperties(IntPtr hwnd, IntPtr hPrinter, [MarshalAs(UnmanagedType.LPStr)] string pDeviceNameg, IntPtr pDevModeOutput, ref IntPtr pDevModeInput, int fMode);

    [DllImport("winspool.Drv", EntryPoint = "GetPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetPrinter(IntPtr hPrinter, Int32 dwLevel, IntPtr pPrinter, Int32 dwBuf, out Int32 dwNeeded);

    [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, ref PrinterDefaults pd);

    [DllImport("winspool.Drv", CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern bool SetPrinter(IntPtr hPrinter, int Level, IntPtr pPrinter, int Command);

    internal static void SetPrinterSetting(string printerName, PageSetting printerSetting)
    {
        IntPtr hPrinter = IntPtr.Zero;
        IntPtr yDevModeData = IntPtr.Zero;
        IntPtr printerInfo = IntPtr.Zero;
        try
        {
            var dm = GetPrinterSettings(printerName, out yDevModeData, out printerInfo, out hPrinter, out PrinterDefaults printerValues, out PrinterInfo9 pinfo);

            dm.dmPaperSize = (short)printerSetting.Size;
            dm.dmOrientation = (short)printerSetting.Orientation;
            //構造体からポインタに
            Marshal.StructureToPtr(dm, yDevModeData, true);
            pinfo.pDevMode = yDevModeData;

            Marshal.StructureToPtr(pinfo, printerInfo, true);
            var result = SetPrinter(hPrinter, 9, printerInfo, 0);
            var nRet = Convert.ToInt16(result);
            if (nRet == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        finally
        {
            if (hPrinter != IntPtr.Zero)
                ClosePrinter(hPrinter);
            if (yDevModeData != IntPtr.Zero)
                Marshal.FreeCoTaskMem(yDevModeData);
            if (printerInfo != IntPtr.Zero)
                Marshal.FreeCoTaskMem(printerInfo);
        }
    }

    private static DevMode GetPrinterSettings(string printerName, out IntPtr yDevModeData, out IntPtr pi, out IntPtr hPrinter, out PrinterDefaults printerValues, out PrinterInfo9 pinfo)
    {
        DevMode dm;
        const int DM_OUT_BUFFER = 2;
        //const int PRINTER_ACCESS_ADMINISTER = 0x4;
        const int PRINTER_ACCESS_USE = 0x8;
        //const int PRINTER_ALL_ACCESS = 0xF000C;

        printerValues = new PrinterDefaults()
        {
            pDatatype = IntPtr.Zero,
            pDevMode = IntPtr.Zero,
            DesiredAccess = PRINTER_ACCESS_USE,
        };

        var nRet = Convert.ToInt32(OpenPrinter(printerName, out hPrinter, ref printerValues));
        if (nRet == 0)
            throw new Win32Exception(Marshal.GetLastWin32Error());

        GetPrinter(hPrinter, 2, IntPtr.Zero, 0, out int nBytesNeeded);
        if (nBytesNeeded <= 0)
            throw new Exception("Unable to allocate memory");
        else
        {
            pi = Marshal.AllocCoTaskMem(nBytesNeeded);
            var result = GetPrinter(hPrinter, 9, pi, nBytesNeeded, out _);
            nRet = Convert.ToInt32(result);
            if (nRet == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            pinfo = (PrinterInfo9)Marshal.PtrToStructure(pi, typeof(PrinterInfo9));
            var temp = new IntPtr();
            if (pinfo.pDevMode == IntPtr.Zero)
            {
                var ptrZero = IntPtr.Zero;
                var sizeOfDevMode = DocumentProperties(IntPtr.Zero, hPrinter, printerName, ptrZero, ref ptrZero, 0);
                IntPtr ptrDM = IntPtr.Zero;
                try
                {
                    ptrDM = Marshal.AllocCoTaskMem(sizeOfDevMode);
                    int i = DocumentProperties(IntPtr.Zero, hPrinter, printerName, ptrDM, ref ptrZero, DM_OUT_BUFFER);
                    if ((i < 0) || (ptrDM == IntPtr.Zero))
                        throw new ApplicationException("Cannot get DEVMODE data");
                    pinfo.pDevMode = ptrDM;
                }
                finally { Marshal.FreeCoTaskMem(ptrDM); }
            }
            var intError = DocumentProperties(IntPtr.Zero, hPrinter, printerName, IntPtr.Zero, ref temp, 0);
            yDevModeData = Marshal.AllocCoTaskMem(intError);

            _ = DocumentProperties(IntPtr.Zero, hPrinter, printerName, yDevModeData, ref temp, 2);
            dm = (DevMode)Marshal.PtrToStructure(yDevModeData, typeof(DevMode));
            if ((nRet == 0) || (hPrinter == IntPtr.Zero))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return dm;
        }
    }
}
