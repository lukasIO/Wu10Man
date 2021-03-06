﻿using System;
using System.Configuration;
using System.Linq;
using System.Windows.Controls;
using WereDev.Utils.Wu10Man.Editors;
using WereDev.Utils.Wu10Man.UserControls.Models;

namespace WereDev.Utils.Wu10Man.UserControls
{
    /// <summary>
    /// Interaction logic for HostsFileControl.xaml
    /// </summary>
    public partial class HostsFileControl : UserControl
    {
        private readonly HostsEditor _hostsEditor;
        private readonly HostsFileModel _model;

        public HostsFileControl()
        {
            _hostsEditor = new HostsEditor();
            _model = new HostsFileModel();
            GetHostSettings();
            DataContext = _model;
            InitializeComponent();
        }

        public void SaveHostSettings()
        {
            var selectedHosts = _model.HostStatus.Where(x => x.IsBlocked).Select(x => x.Host).ToArray();
            if (selectedHosts.Any())
                _hostsEditor.SetHostsEntries(selectedHosts);
            else
                _hostsEditor.ClearHostsEntries();
        }

        private void BlockHost_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = (Button)e.Source;
            var kvp = (HostStatus)button.DataContext;
            SetHostValue(kvp.Host, true);
            Logger.LogInfo(string.Format("Host blocked: {0}", kvp.Host));
            ShowUpdateNotice();
        }

        private void UnblockHost_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = (Button)e.Source;
            var kvp = (HostStatus)button.DataContext;
            SetHostValue(kvp.Host, false);
            Logger.LogInfo(string.Format("Host unblocked: {0}", kvp.Host));
            ShowUpdateNotice();
        }

        private void SetHostValue(string hostUrl, bool block)
        {
            var hostStatus = _model.HostStatus.FirstOrDefault(x => x.Host == hostUrl);
            if (hostStatus == null) throw new ArgumentOutOfRangeException(nameof(hostUrl));
            hostStatus.IsBlocked = block;
            SaveHostSettings();
        }

        private void GetHostSettings()
        {
            var hostUrls = GetWindowsUpdateUrls();
            if (hostUrls == null) return;
            var currentHosts = _hostsEditor.GetHostsInFile();
            var hostSettings = hostUrls.ToDictionary(x => x, x => currentHosts.Contains(x));
            _model.HostStatus = hostSettings.Select(x => new HostStatus(x.Key, x.Value))
                                            .OrderBy(x => x.Host)
                                            .ToArray();
        }

        private string[] GetWindowsUpdateUrls()
        {
            var windowsUpdateUrls = ConfigurationManager.AppSettings["WindowsUpdateUrls"];
            if (windowsUpdateUrls == null) return new string[0];
            var split = windowsUpdateUrls.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var uniques = split.Distinct();
            return uniques?.ToArray();
        }

        private void UnblockAllHosts_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (var hostStatus in _model.HostStatus)
                hostStatus.IsBlocked = false;
            SaveHostSettings();
            Logger.LogInfo("All Hosts blocked.");
            ShowUpdateNotice();
        }

        private void BlockAllHosts_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (var hostStatus in _model.HostStatus)
                hostStatus.IsBlocked = true;
            SaveHostSettings();
            Logger.LogInfo("All Hosts unblocked.");
            ShowUpdateNotice();
        }

        private void ShowUpdateNotice()
        {
            System.Windows.MessageBox.Show("Hosts file udpated.", "Hosts File", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
    }
}
