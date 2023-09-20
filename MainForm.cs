using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

public class MainForm : Form
{
    public MainForm(bool silent = false)
    {
        Text = "Steam Lite";
        Font = SystemFonts.MessageBoxFont;
        ListView listView = new()
        {
            BorderStyle = BorderStyle.Fixed3D,
            View = View.Details,
            Dock = DockStyle.Fill,
            HeaderStyle = ColumnHeaderStyle.None,
            MultiSelect = false
        };
        NotifyIcon notifyIcon = new() { Text = "Steam Lite", Icon = Icon, Visible = true };
        Dictionary<string, string> apps = SteamClient.GetAppsForUser();
        TableLayoutPanel tableLayoutPanel = new() { Dock = DockStyle.Fill };
        Button button = new()
        {
            Text = "Play",
            Dock = DockStyle.Fill,
            Anchor = AnchorStyles.Bottom |
           AnchorStyles.Left |
           AnchorStyles.Right,
            Enabled = false
        };


        tableLayoutPanel.Controls.Add(listView, 0, 0);
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        tableLayoutPanel.Controls.Add(button, -1, 0);
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        Controls.Add(tableLayoutPanel);
        listView.Columns.Add("Games");

        for (int i = 0; i < apps.Keys.Count; i++)
        {
            listView.Items.Add(apps.Keys.ElementAt(i));
        }

        Resize += (sender, e) =>
        {
            if (WindowState == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Maximized;
                ShowInTaskbar = true;
            }
            else if (WindowState == FormWindowState.Minimized)
                ShowInTaskbar = false;
            listView.Width = listView.ClientSize.Width;
            listView.Columns[0].Width = listView.ClientSize.Width;
        };

        FormClosing += (sender, e) =>
        {
            notifyIcon.Visible = false;
            ShowInTaskbar = false;
            SteamClient.Shutdown();
        };

        notifyIcon.DoubleClick += (sender, e) =>
        {
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
            Activate();
        };

        listView.SelectedIndexChanged += (sender, e) => button.Enabled = listView.Enabled && listView.SelectedItems.Count != 0;
        listView.ItemActivate += (sender, e) => button.PerformClick();
        button.Click += (sender, e) => new Thread(() =>
        {
            if (listView.SelectedItems.Count != 0)
            {
                WindowState = FormWindowState.Minimized;

                listView.Enabled = false;
                button.Enabled = false;
                Text = notifyIcon.Text = $"Steam Lite - {listView.SelectedItems[0].Text}";
                button.Text = "Running";
                SteamClient.RunGameId(apps[listView.SelectedItems[0].Text]);
                button.Text = "Play";
                Text = notifyIcon.Text = "Steam Lite";
                listView.Enabled = true;
                button.Enabled = true;
                listView.SelectedItems[0].Selected = false;
            }
        }).Start();

        if (silent)
            WindowState = FormWindowState.Minimized;
        OnResize(null);
    }


}
