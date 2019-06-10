using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Ookii.Dialogs.Wpf;

namespace PlayDown_Cloner
{
    public partial class MainWindow : Window
    {
        private bool close;
        NotifyIcon nIcon = new NotifyIcon();
        ContextMenu context = new ContextMenu();

        public MainWindow()
        {
            InitializeComponent();

            DeleteCurrentFiles.IsChecked = Properties.Settings.Default.DeleteCurrentFiles;
            LaunchAtStartup.IsChecked = Properties.Settings.Default.LaunchAtStartup;

            LocalFolderButton.ToolTip = Properties.Settings.Default.LocalPath;
            ServerFolderButton.ToolTip = Properties.Settings.Default.ServerPath;

            context.MenuItems.Add(new MenuItem("Send", SendEvent));
            context.MenuItems.Add(new MenuItem("Clone", CloneEvent));
            context.MenuItems.Add("-");
            context.MenuItems.Add(new MenuItem("Settings", OpenSettings));
            context.MenuItems.Add(new MenuItem("Exit", CloseWindow));

            nIcon.Icon = Properties.Resources.Icon;
            nIcon.ContextMenu = context;
            nIcon.DoubleClick += OpenSettings;
            nIcon.Visible = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            Hide();
            nIcon.Visible = true;
        }
                
        private void LocalFolderButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.RootFolder = Environment.SpecialFolder.Desktop;
            if (folder.ShowDialog().ToString() == "OK")
            {
                Properties.Settings.Default.LocalPath = folder.SelectedPath;
                Properties.Settings.Default.Save();
                LocalFolderButton.ToolTip = Properties.Settings.Default.LocalPath;
            }
        }

        private void ServerFolderButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.RootFolder = Environment.SpecialFolder.Desktop;
            if (folder.ShowDialog().ToString() == "OK")
            {
                Properties.Settings.Default.ServerPath = folder.SelectedPath;
                Properties.Settings.Default.Save();
                ServerFolderButton.ToolTip = Properties.Settings.Default.ServerPath;
            }
        }

        private void SendEvent(object sender, EventArgs e)
        {
            Send();
        }

        private void CloneEvent(object sender, EventArgs e)
        {
            Clone();
        }

        private void Send()
        {
            if (Properties.Settings.Default.LocalPath != string.Empty && Properties.Settings.Default.ServerPath != string.Empty)
            {
                DirectoryCopy(Properties.Settings.Default.LocalPath, Properties.Settings.Default.ServerPath, true);
            }
        }

        private void Clone()
        {
            if (Properties.Settings.Default.LocalPath != string.Empty && Properties.Settings.Default.ServerPath != string.Empty)
            {
                DirectoryCopy(Properties.Settings.Default.ServerPath, Properties.Settings.Default.LocalPath, true);
            }
        }

        private void OpenSettings(object sender, EventArgs e)
        {
            Show();
            this.WindowState = WindowState.Normal;
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            close = true;
            Close();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState != WindowState.Normal)
            {
                Hide();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(!close)
            {
                e.Cancel = true;
                this.WindowState = WindowState.Minimized;
                Hide();                         
            }
        }

        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            if (DeleteCurrentFiles.IsChecked == true && Directory.GetDirectories(destDirName).Length > 0)
            {
                DirectoryInfo di = new DirectoryInfo(destDirName);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo inf in di.GetDirectories())
                {
                    inf.Delete(true);
                }
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private void DeleteCurrentFiles_Checked(object sender, RoutedEventArgs e)
        {
            if(DeleteCurrentFiles.IsChecked == true)
                Properties.Settings.Default.DeleteCurrentFiles = true;
            else
                Properties.Settings.Default.DeleteCurrentFiles = false;
            Properties.Settings.Default.Save();
        }

        private void LaunchAtStart_Checked(object sender, RoutedEventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (LaunchAtStartup.IsChecked == true)
            {
                Properties.Settings.Default.LaunchAtStartup = true;
                rk.SetValue("PlayDown Cloner", System.Windows.Forms.Application.ExecutablePath.ToString());
            }
            else
            {
                Properties.Settings.Default.LaunchAtStartup = false;
                rk.DeleteValue("PlayDown Cloner", false);
            }
            Properties.Settings.Default.Save();
        }
    }
}