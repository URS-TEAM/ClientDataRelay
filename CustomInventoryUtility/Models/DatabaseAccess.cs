using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace InventoryUtility.Models
{
    public class DatabaseAccess : IDataErrorInfo, INotifyPropertyChanged
    {

        private string _serverName = "";
        public string ServerName
        {
            get { return _serverName; }
            set
            {
                if (_serverName != value)
                {
                    _serverName = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsFormCompleted));
                }
            }
        }

        private string _databaseName = "";
        public string DatabaseName
        {
            get { return _databaseName; }
            set
            {
                if (_databaseName != value)
                {
                    _databaseName = value;
                    OnPropertyChanged(nameof(DatabaseName));
                    OnPropertyChanged(nameof(IsFormCompleted));
                }
            }
        }

        private string _username = "";
        public string Username
        {
            get { return _username; }
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged(nameof(Username));
                    OnPropertyChanged(nameof(IsFormCompleted));
                }
            }
        }

        private string _password = "";
        public string Password
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
                    OnPropertyChanged(nameof(IsFormCompleted));
                }
            }
        }

        public bool IsFormCompleted {
            get { return !string.IsNullOrWhiteSpace(ServerName) && !string.IsNullOrWhiteSpace(Username) &&
                    !string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(DatabaseName); }
        }

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get
            {
                if (columnName == "ServerName")
                {
                    if (string.IsNullOrEmpty(ServerName))
                        return "Server is required";
                } else if (columnName == "DatabaseName")
                {
                    if (string.IsNullOrEmpty(Username))
                        return "Database is required";
                } else if (columnName == "Username")
                {
                    if (string.IsNullOrEmpty(Username))
                        return "Username is required";
                } else if (columnName == "Password")
                {
                    if (string.IsNullOrEmpty(Username))
                        return "Password is required";
                }
                return null;
            }
        }

        #endregion

        #region PropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

    }
}
