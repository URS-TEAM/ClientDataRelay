using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryUtility.ViewModels;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InventoryUtility.Models
{

    public enum TableCheckResult
    {
        TableDoesNotExist,
        ColumnsMismatch,
        TableExists,
        Error
    }

    public enum ButtonState
    {
        Default,
        Waiting,
        Done
    }

    public interface IFunctionality
    {
        string Description { get; set; }
        string ButtonContent { get; set; }
        ButtonState BtnState { 
            get; set; }
        ICommand ActivateFuncCommand { get; set; }

        Task UpdateStateButton();
    }

    public  class BaseFunctionality : IFunctionality, INotifyPropertyChanged
    {

        private string _description = "";
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _buttonContent = "";
        public string ButtonContent
        {
            get { return _buttonContent; }
            set
            {
                if (_buttonContent != value)
                {
                    _buttonContent = value;
                    OnPropertyChanged();
                }
            }
        }

        private ButtonState _btnState;
        public ButtonState BtnState
        {
            get { return _btnState; }
            set
            {
                if (_btnState != value)
                {
                    _btnState = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand ActivateFuncCommand { get; set; }

        public BaseFunctionality(string description, string buttonContent)
        {
            this.Description = description;
            this.ButtonContent = buttonContent;
        }

        protected async Task ExecuteTaskAsync(Func<Task> task)
        {
            await task.Invoke();
        }

        #region PropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Task UpdateStateButton() { return Task.CompletedTask;  }
        #endregion
    }

}
