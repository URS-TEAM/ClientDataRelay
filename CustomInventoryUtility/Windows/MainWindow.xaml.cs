using InventoryUtility.ViewModels;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace InventoryUtility.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        public MainViewModel ViewModel { get; }

        public MainWindow(MainViewModel viewModel,
                          IContentDialogService contentDialogService)
        {
            ViewModel = viewModel;
            this.DataContext = this;
            InitializeComponent();

            TitleBar.MinimizeActionOverride = (TitleBar sender, Window wind) => {
                this.Hide();
            };

            contentDialogService.SetContentPresenter(RootContentDialog);
            Wpf.Ui.Appearance.ApplicationAccentColorManager.Apply((Color)ColorConverter.ConvertFromString("#0f6be9"));
        }

        private void StackPanel_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var focusedElement = e.Source;

                if (focusedElement != null && focusedElement == PwdBox)
                    ViewModel.SaveDBAccessCommand.Execute(null);
                else if (focusedElement is TextBox textBox)
                {
                    textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                }
                e.Handled = true;
            }
        }


    }
}
