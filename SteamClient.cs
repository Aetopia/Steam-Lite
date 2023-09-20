using System;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;


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
    /// Obtains a dictionary of installed Steam applications with their App ID and names.
    /// </summary>
    /// <returns>Any installed Steam applications.</returns>
    public static Dictionary<string, string> GetApps()
    {
        Dictionary<string, string> apps = [];
        using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Valve\\Steam\\Apps");
        string[] subKeyNames = registryKey.GetSubKeyNames();
        for (int i = 0; i < subKeyNames.Length; i++)
        {
            using RegistryKey subKey = registryKey.OpenSubKey(subKeyNames[i]);
            if ((int)subKey.GetValue("Installed", 0) == 1 &&
                subKey.GetValueNames().Contains("Name"))
                apps[subKey.GetValue("Name").ToString()] = subKeyNames[i];
        }
        return apps;
    }

    /// <summary>
    /// Initializes a new Steam Client instance for the class.
    /// </summary>
    /// <returns>
    /// An instance of the Process class of the launched Steam Client instance or null if a launched instance already exists.
    /// </returns>
    public static Process Launch()
    {
        using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Valve\\Steam");
        string steamExe = registryKey.GetValue("SteamExe").ToString();

        if (FindWindow("vguiPopupWindow", "SteamClient") != IntPtr.Zero)
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

    public static void WebHelper(bool enable)
    {
        IntPtr
        hWnd = FindWindow("vguiPopupWindow", "SteamClient"),
        hThread = OpenThread(0x0002, false, GetWindowThreadProcessId(hWnd, out uint _));
        if (enable)
            ResumeThread(hThread);
        else
            SuspendThread(hThread);
        Process[] processes = Process.GetProcessesByName("steamwebhelper");
        for (int i = 0; i < processes.Length; i++)
        {
            processes[i].Kill();
            processes[i].Dispose();
        }
        CloseHandle(hThread);
    }


    public static void StartGameId(string gameId)
    {
        WebHelper(true);
        using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey($"SOFTWARE\\Valve\\Steam\\Apps\\{gameId}");
        IntPtr hEvent = CreateEvent(IntPtr.Zero, true, false, IntPtr.Zero);
        Process.Start("explorer.exe", $"steam://rungameid/{gameId}").Close();
        RegNotifyChangeKeyValue(registryKey.Handle, true, 0x00000004, hEvent, true);
        WaitForSingleObject(hEvent, 0xffffffff);
        WebHelper(false);
        RegNotifyChangeKeyValue(registryKey.Handle, true, 0x00000004, hEvent, true);
        WaitForSingleObject(hEvent, 0xffffffff);
        CloseHandle(hEvent);
    }
}