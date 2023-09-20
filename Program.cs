using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

static class Program
{
  [STAThread]
  static void Main(string[] args)
  {
    Process process = SteamClient.Launch();
    bool silent = false;
    for (int i = 0; i < args.Length; i++)
    {
      if (args[i] == "-silent" && !silent)
        silent = true;
    }

    if (process != null)
    {
      new Thread(() =>
      {
        process.WaitForExit();
        process.Dispose();
      //  Environment.Exit(0);
      });
      Application.EnableVisualStyles();
      Application.Run(new MainForm());
    }
    else
    {
      MessageBox.Show("Steam is either already running or isn't installed.", "Steam Lite", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
  }
}