using System;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Linq.Expressions;

/// <summary>
/// Provides methods for interacting with a Steam Client instance.
/// </summary>
public class SteamClient
{
    [DllImport("User32.dll")]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint dwProcessId);

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    static extern bool SetWindowText(IntPtr hWnd, string lpString);

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("Kernel32.dll")]
    static extern uint SuspendThread(IntPtr hThread);

    [DllImport("Kernel32.dll")]
    static extern uint ResumeThread(IntPtr hThread);

    [DllImport("Kernel32.dll")]
    static extern IntPtr OpenThread(uint dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

    [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
    static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, IntPtr lpName);

    [DllImport("Kernel32.dll")]
    static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

    [DllImport("Kernel32.dll")]
    static extern bool CloseHandle(IntPtr hObject);

    [DllImport("Advapi32.dll")]
    static extern long RegNotifyChangeKeyValue(SafeRegistryHandle hKey, bool bWatchSubtree, uint dwNotifyFilter, IntPtr hEvent, bool fAsynchronous);

    /// <summary>
    /// Obtain a running Steam client instance.
    /// </summary>
    /// <returns>Any currently running Steam Client instance, null if no instance is running.</returns>
    public static Process GetInstance()
    {
        if (GetWindowThreadProcessId(FindWindow("vguiPopupWindow", "SteamClient"), out uint dwProcessId) != 0)
            return Process.GetProcessById((int)dwProcessId);
            
        return null;
    }

    /// <summary>
    /// Obtains installed Steam applications with their App ID and name.
    /// </summary>
    /// <returns>
    /// A dictionary of installed Steam applications.
    /// </returns>
    public static Dictionary<string, string> GetApps()
    {
        Dictionary<string, string> apps = [];

        using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Valve\\Steam\\Apps");
        if (registryKey != null)
        {
            string[] subKeyNames = registryKey.GetSubKeyNames();
            for (int i = 0; i < subKeyNames.Length; i++)
            {
                using RegistryKey subKey = registryKey.OpenSubKey(subKeyNames[i]);
                if ((int)subKey.GetValue("Installed", 0) == 1 &&
                    subKey.GetValueNames().Contains("Name"))
                    apps[subKey.GetValue("Name").ToString()] = subKeyNames[i];
            }
        }

        return apps;
    }

    /// <summary>
    /// Initializes a new Steam Client instance for the class.
    /// </summary>
    /// <returns>
    /// An instance of the launched Steam Client instance or null if a launched instance already exists or Steam isn't installed.
    /// </returns>
    public static Process Launch()
    {
        using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Valve\\Steam");
        string steamExe = registryKey.GetValue("SteamExe").ToString();

        if (FindWindow("vguiPopupWindow", "SteamClient") != IntPtr.Zero || !File.Exists(steamExe))
            return null;
        Process process;
        if (GetWindowThreadProcessId(FindWindow("vguiPopupWindow", "Untitled"), out uint dwProcessId) != 0)
            using (process = Process.GetProcessById((int)dwProcessId))
            {
                Process.Start(steamExe, "-shutdown").Dispose();
                process.WaitForExit();
            }

        process = Process.Start(steamExe, $"-silent -cef-single-process -cef-in-process-gpu -cef-disable-d3d11 -cef-disable-breakpad");
        IntPtr hWnd;
        while ((hWnd = FindWindow("vguiPopupWindow", "Untitled")) == IntPtr.Zero)
            Thread.Sleep(1);
        SetWindowText(hWnd, "SteamClient");
        WebHelper(false);

        return process;
    }

    /// <summary>
    /// Uninitializes the current running Steam Client instance.
    /// </summary>
    /// <returns>If the operation was successful true is returned else false.</returns>
    public static bool Shutdown()
    {
        if (FindWindow("vguiPopupWindow", "SteamClient") != IntPtr.Zero)
        {
            WebHelper(true);
            using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Valve\\Steam");
            Process.Start(registryKey.GetValue("SteamExe").ToString(), "-shutdown").Dispose();
        }

        return true;
    }

    /// <summary>
    /// Disables or enables the Steam WebHelper.
    /// </summary>
    /// <param name="enable">Pass true to enable or false to disable the Steam WebHelper.</param>
    /// <returns>If the operation was successful true is returned else false.</returns>
    public static bool WebHelper(bool enable)
    {
        IntPtr
        hWnd = FindWindow("vguiPopupWindow", "SteamClient"),
        hThread = OpenThread(0x0002, false, GetWindowThreadProcessId(hWnd, out uint _));

        if (hWnd != IntPtr.Zero)
        {
            if (enable) ResumeThread(hThread);
            else SuspendThread(hThread);
            Process[] processes = Process.GetProcessesByName("steamwebhelper");
            for (int i = 0; i < processes.Length; i++)
            {
                processes[i].Kill();
                processes[i].Dispose();
            }
        }
        CloseHandle(hThread);

        return hWnd != IntPtr.Zero;
    }

    /// <summary>
    /// Runs the specified App ID.
    /// </summary>
    /// <param name="gameId">App ID of the app to run.</param>
    /// <returns>
    /// If the operation was successful true is returned else false.
    /// </returns>
    public static bool RunGameId(string gameId)
    {
        if (FindWindow("vguiPopupWindow", "SteamClient") == IntPtr.Zero)
            return false;

        using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey($"SOFTWARE\\Valve\\Steam\\Apps\\{gameId}");
        IntPtr hEvent = CreateEvent(IntPtr.Zero, true, false, IntPtr.Zero);

        WebHelper(true);
        Process.Start("explorer.exe", $"steam://rungameid/{gameId}").Close();
        RegNotifyChangeKeyValue(registryKey.Handle, true, 0x00000004, hEvent, true);
        WaitForSingleObject(hEvent, 0xffffffff);
        WebHelper(false);

        RegNotifyChangeKeyValue(registryKey.Handle, true, 0x00000004, hEvent, true);
        WaitForSingleObject(hEvent, 0xffffffff);
        CloseHandle(hEvent);

        return true;
    }

    /// <summary>
    /// Obtains installed Steam applications with their App ID and name for the currently signed in user.
    /// </summary>
    /// <returns>
    /// A dictionary of installed Steam applications for the currently signed in user.
    /// </returns>
    public static Dictionary<string, string> GetAppsForUser()
    {
        Dictionary<string, string> apps = GetApps(), userApps = []; ;
        using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Valve\\Steam");
        using RegistryKey subKey = registryKey.OpenSubKey("ActiveProcess");
        string[] lines = File.ReadAllLines($"{registryKey.GetValue("SteamPath")}/userdata/{subKey.GetValue("ActiveUser")}/config/localconfig.vdf");

        for (int i = 0; i < lines.Length; i++)
        {
            try
            {
                KeyValuePair<string, string> keyValuePair = new();
                string line = lines[i].Trim().Trim('"');
                Convert.ToUInt32(line);
                keyValuePair = apps.First(source => source.Value == line);
                userApps.Add(keyValuePair.Key, keyValuePair.Value);
            }
            catch (FormatException) { }
            catch (InvalidOperationException) { }

        }

        return userApps;
    }
}