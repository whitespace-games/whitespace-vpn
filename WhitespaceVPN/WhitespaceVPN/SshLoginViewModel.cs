using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

using System.Windows.Controls;
using System.Windows.Input;

using Renci.SshNet;
using Renci.SshNet.Common;
using System.Windows;
using System;
using System.Net.Sockets;

namespace WhitespaceVPN
{
    public class SshLoginViewModel : ObservableObject
    {
        private string _login;
        private bool _loggedIn;
        private string _connectionMessage;

        private SshClient sshClient;
        private ForwardedPort forwardedPort;

        public ICommand LoginCommand { get; }
        public ICommand LogoutCommand { get; }

        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
        }

        public string ConnectionMessage
        {
            get => _connectionMessage;
            set { _connectionMessage = value; OnPropertyChanged(); }
        }

        public bool LoggedIn
        {
            get => _loggedIn;
            set { _loggedIn = value; OnPropertyChanged(); }
        }

        public SshLoginViewModel()
        {
            _login = string.Empty;
            _loggedIn = false;

            this.LoginCommand = new RelayCommand<object>(DoLogin);
            this.LogoutCommand = new RelayCommand(DoLogout);
        }

        private void DoLogin(object passwordBox)
        {
            this.sshClient = new SshClient("ssh.mini.pw.edu.pl", Login, (passwordBox as PasswordBox).Password);
            this.sshClient.KeepAliveInterval = TimeSpan.FromMinutes(1);

            try
            {
                this.sshClient.Connect();
            }
            catch (SshAuthenticationException e)
            {
                MessageBox.Show($"Authentication failed: {e.Message}");
                return;
            }
            catch (SshConnectionException)
            {
                MessageBox.Show($"Connection error.");
                return;
            }

            if (!this.BindPort())
            {
                this.sshClient.Disconnect();
                this.sshClient.Dispose();

                return;
            }

            LoggedIn = true;
        }

        private void DoLogout()
        {
            this.sshClient.Disconnect();
            this.sshClient.Dispose();

            LoggedIn = false;
        }

        private bool BindPort()
        {
            var localAddress = "127.0.0.1";
            uint port = 4242;
            var random = new Random();
            
            while (true)
            {
                try
                {
                    this.forwardedPort = new ForwardedPortLocal(localAddress, port, "perforce.knur.mini.pw.edu.pl", 1666);
                    this.sshClient.AddForwardedPort(forwardedPort);

                    break;
                }
                catch (SshConnectionException)
                {
                    port = (uint)random.Next(1024, 49152);
                }
            }

            try
            {
                this.forwardedPort.Start();
            }
            catch (SocketException)
            {
                MessageBox.Show("Another instance of app already running and connected");
                return false;
            }

            this.ConnectionMessage = $"Use address ssl:{localAddress}:{port} to access Perforce.";

            return true;
        }
    }
}
