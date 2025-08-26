using System.Runtime.InteropServices;
using GfxMan.Services.Interfaces;
using GfxMan.Services.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace GfxMan.Services;

public class SystemMonitorInfoService: HasOnStatusChangedEventServiceBase, ISystemDeviceInformationService<MonitorInfo>, IDisposable
{
    private readonly ILogger<SystemMonitorInfoService> _logger;

    public SystemMonitorInfoService(ILogger<SystemMonitorInfoService> logger)
    {
        _logger = logger;
        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
        EnumerateDevices();
    }
    
    public IEnumerable<MonitorInfo> Devices { get; private set; } = new List<MonitorInfo>();
    
    public void EnumerateDevices()
    {
        var devices = new List<MonitorInfo>();
        
        const uint displayDevicePrimaryDevice = 0x00000004;
        
        var gpu = new DisplayDevice { Cb = Marshal.SizeOf<DisplayDevice>() };
        var monitor = new DisplayDevice { Cb = Marshal.SizeOf<DisplayDevice>() };

        for (uint i = 0; EnumDisplayDevices(null, i, ref gpu, 0); i++)
        {
            for (uint j = 0; EnumDisplayDevices(null, i, ref monitor, 0); j++) 
            {
                if (!EnumDisplayDevices(gpu.DeviceName, j, ref monitor, 0))
                {
                    break;
                }

                var device = new MonitorInfo
                {
                    Name = GetDeviceManagerNameForMonitor(monitor.DeviceId),
                    IsPrimaryDisplay = (gpu.StateFlags & displayDevicePrimaryDevice) != 0
                };
                var resolution = GetPrimaryMonitorResolution(gpu.DeviceName);
                device.MaximumPixelWidth = resolution.width;
                device.MaximumPixelHeight = resolution.height;
                devices.Add(device);
            }
        }
        
        Devices = devices;
        InvokeStatusChangedEvent(EventArgs.Empty);
    }
    
    private string? GetDeviceManagerNameForMonitor(string monitorDeviceId)
    {
        if (string.IsNullOrWhiteSpace(monitorDeviceId))
            return null;

        var modelCode = monitorDeviceId.Split('\\').Skip(1).FirstOrDefault()?.ToLower();
        if (string.IsNullOrWhiteSpace(modelCode))
            return null;

        const string baseKeyPath = @"SYSTEM\CurrentControlSet\Enum\DISPLAY";

        try
        {
            using var displayKey = Registry.LocalMachine.OpenSubKey(baseKeyPath);
            if (displayKey == null)
                return null;

            foreach (var subKeyName in displayKey.GetSubKeyNames())
            {
                if (!subKeyName.ToLower().Contains(modelCode))
                    continue;

                using var modelKey = displayKey.OpenSubKey(subKeyName);
                if (modelKey == null)
                    continue;

                foreach (var instanceKeyName in modelKey.GetSubKeyNames())
                {
                    using var instanceKey = modelKey.OpenSubKey(instanceKeyName);
                    if (instanceKey == null)
                        continue;

                    var deviceDesc = instanceKey.GetValue("FriendlyName") as string;
                    if (string.IsNullOrWhiteSpace(deviceDesc)) continue;
                    
                    deviceDesc = deviceDesc?.Replace("(%1);", string.Empty);
                    return deviceDesc.Contains(';') ? deviceDesc.Split(';').Last().Trim() : deviceDesc.Trim();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Registry lookup failed: {ex.Message}");
        }

        return null;
    }
    
    private static (uint width, uint height) GetPrimaryMonitorResolution(string deviceName)
    {
        var devMode = new DevMode
        {
            dmSize = (ushort)Marshal.SizeOf(typeof(DevMode))
        };

        uint maxWidth = 0;
        uint maxHeight = 0;

        var modeNum = 0;
        while (EnumDisplaySettings(deviceName, modeNum, ref devMode))
        {
            if (devMode.dmPelsWidth * devMode.dmPelsHeight > maxWidth * maxHeight)
            {
                maxWidth = devMode.dmPelsWidth;
                maxHeight = devMode.dmPelsHeight;
            }
            modeNum++;
        }
        return (maxWidth, maxHeight);
    }
    
    public void Dispose()
    {
        SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
    }

    private void OnDisplaySettingsChanged(object sender, EventArgs e)
    {
        EnumerateDevices();
    }
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct DisplayDevice
    {
        public int Cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        public uint StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct DevMode
    {
        private const int Cchdevicename = 32;
        private const int Cchformname = 32;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Cchdevicename)]
        public string dmDeviceName;

        public ushort dmSpecVersion;
        public ushort dmDriverVersion;
        public ushort dmSize;
        public ushort dmDriverExtra;
        public uint dmFields;

        public int dmPositionX;
        public int dmPositionY;
        public uint dmDisplayOrientation;
        public uint dmDisplayFixedOutput;

        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Cchformname)]
        public string dmFormName;

        public ushort dmLogPixels;
        public uint dmBitsPerPel;
        public uint dmPelsWidth;
        public uint dmPelsHeight;

        public uint dmDisplayFlags;
        public uint dmDisplayFrequency;

        public uint dmICMMethod;
        public uint dmICMIntent;
        public uint dmMediaType;
        public uint dmDitherType;
        public uint dmReserved1;
        public uint dmReserved2;

        public uint dmPanningWidth;
        public uint dmPanningHeight;
    }

    [DllImport("user32.dll", CharSet = CharSet.Ansi)]
    private static extern bool EnumDisplayDevices(string? lpDevice, uint deviceNum, ref DisplayDevice displayDevice, uint flags);
    
    [DllImport("user32.dll")]
    private static extern bool EnumDisplaySettings(string? deviceName, int modeNum, ref DevMode devMode);
}