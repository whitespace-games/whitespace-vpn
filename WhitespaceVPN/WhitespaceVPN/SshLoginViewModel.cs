using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

using System.Windows.Controls;
using System.Windows.Input;

using Renci.SshNet;

namespace WhitespaceVPN
{
    public class SshLoginViewModel : ObservableObject
    {
        private string _login;
        private bool _loggedIn;

        private SshClient sshClient;
        private ForwardedPort forwardedPort;

        public ICommand LoginCommand { get; }
        public ICommand LogoutCommand { get; }

        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
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
            var obj = passwordBox as PasswordBox;

            this.sshClient = new SshClient("ssh.mini.pw.edu.pl", Login, obj.Password);
            this.sshClient.Connect();

            this.forwardedPort = new ForwardedPortLocal("127.0.0.1", 4242, "perforce.knur.mini.pw.edu.pl", 1666);
            this.sshClient.AddForwardedPort(forwardedPort);
            
            this.forwardedPort.Start();

            LoggedIn = true;
        }

        private void DoLogout()
        {
            this.sshClient.Disconnect();
            this.forwardedPort.Stop();
            this.sshClient.Dispose();

            LoggedIn = false;
        }
    }
}
