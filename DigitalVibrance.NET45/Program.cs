using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NvAPIWrapper.Display;
using WindowsDisplayAPI;
using WindowsDisplayAPI.DisplayConfig;

namespace DigitalVibrance.NET45
{
    internal class Program
    {

        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 3)
                {
                    Console.WriteLine("Usage: <DisplayIdNumber> <DigitalVibrancePercentage> <Resolution>");
                    Console.WriteLine("Example: 1 100 1024x768@165");
                    Environment.Exit(0);
                }
                int displayId;
                int digitalVibrance;
                int resolutionHorizontal;
                int resolutionvertical;
                int resolutionRefreshRate;
                try
                {
                    displayId = int.Parse(args[0]);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Failed to parse DisplayIdNumber: {ex.Message}");
                }
                try
                {
                    digitalVibrance = int.Parse(args[1]);
                    if (digitalVibrance < 0 || digitalVibrance > 100)
                    {
                        throw new Exception("Value must be between 0-100");
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Failed to parse DigitalVibrancePercentage: {ex.Message}");
                }
                try
                {
                    var resolution = args[2];
                    var split1 = resolution.Split('@');
                    if (split1.Length != 2)
                    {
                        throw new Exception("Too many @ characters");
                    }
                    var dimensions = split1[0];
                    resolutionRefreshRate = int.Parse(split1[1]);
                    var split2 = dimensions.Split('x');
                    if (split2.Length != 2)
                    {
                        throw new Exception("Too many x characters");
                    }
                    resolutionHorizontal = int.Parse(split2[0]);
                    resolutionvertical = int.Parse(split2[1]);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Failed to parse Resolution: {ex.Message}");
                }
                var targetDisplayDevice = $"\\\\.\\DISPLAY{displayId}";
                var displays = NvAPIWrapper.Display.Display.GetDisplays();
                var windowsDisplays = WindowsDisplayAPI.Display.GetDisplays();
                NvAPIWrapper.Display.Display nvDisplay;
                WindowsDisplayAPI.Display windowsDisplay;
                try
                {
                    nvDisplay = displays.Single(d => d.Name == targetDisplayDevice);
                    windowsDisplay = windowsDisplays.Single(d => d.DisplayName == targetDisplayDevice);
                }
                catch
                {
                    var availableDisplays = string.Join(", ", displays.Select(d => d.Name));
                    throw new InvalidOperationException($"Could not find display {targetDisplayDevice}. Available displays are: {availableDisplays}");
                }
                DisplaySetting targetWindowsDisplaySetting;
                try
                {
                    var availableSettings = windowsDisplay.GetPossibleSettings();
                    var targetPossibleSettings = availableSettings.Where(s => s.Resolution.Width == resolutionHorizontal && s.Resolution.Height == resolutionvertical && s.Frequency == resolutionRefreshRate);
                    targetWindowsDisplaySetting = new DisplaySetting(targetPossibleSettings.First());
                }
                catch
                {
                    var availableSettings = string.Join("\r\n\t", windowsDisplay.GetPossibleSettings().Select(s => $"{s.Resolution.Width}x{s.Resolution.Height}@{s.Frequency}"));
                    throw new InvalidOperationException($"Could not find display resolution specified. Available options are: \r\n\t{availableSettings}");
                }
                try
                {
                    nvDisplay.DigitalVibranceControl.CurrentLevel = digitalVibrance;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to apply digital vibrance: {ex.Message}");
                }
                try
                {
                    windowsDisplay.SetSettings(targetWindowsDisplaySetting, true);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to apply resolution: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}