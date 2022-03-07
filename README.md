# PrinterSetting
[![Nuget downloads](https://img.shields.io/nuget/v/PrinterSetting.svg)](https://www.nuget.org/packages/PrinterSetting/)
[![NuGet](https://img.shields.io/nuget/dt/PrinterSetting.svg)](https://github.com/zerodev1200/PrinterSetting)
[![GitHub license](https://img.shields.io/github/license/mashape/apistatus.svg)](https://github.com/zerodev1200/PrinterSetting/blob/master/LICENSE)  
This is a library for changing the output size and orientation of printer.

Ver2.1.0.0  
-Changed Win32API from ANSI to UNICODE.  
-Added Extension for enumerating printers and setting/getting default printers.  
### Install
```
PM> Install-Package PrinterSetting
```
### Getting Started



```
var printer = new Printer("PrinterName");
printer.SetPageInfo(PageOrientation.Portrait, PaperSize.A5);

foreach (var p in Extensions.EnumeratePrinterName())
{
    Console.WriteLine(p);
}
Console.WriteLine(Extensions.GetDefaultPrinterName());
var result = Extensions.SetDefaultPrinter("PrinterName");
Console.WriteLine(Extensions.GetDefaultPrinterName());
```

#### .NET5 Or Greater
```
[assembly: SupportedOSPlatform("windows")]  //Put it above the namespace.
or
[SupportedOSPlatform("windows")]  //Put it above the class or method.
```
