using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace Hipot_Tester.Services
{
    public class DialogService : IDialogService
    {
        public void ShowMessage(string message, string title = "알림")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowError(string message, string title = "오류")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowWarning(string message, string title = "경고")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public bool ShowConfirm(string message, string title = "확인")
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        public async Task ShowMessageAsync(string message, string title = "알림")
        {
            await Task.Run(() => ShowMessage(message, title));
        }

        public async Task ShowErrorAsync(string message, string title = "오류")
        {
            await Task.Run(() => ShowError(message, title));
        }

        public async Task ShowWarningAsync(string message, string title = "경고")
        {
            await Task.Run(() => ShowWarning(message, title));
        }

        public async Task<bool> ShowConfirmAsync(string message, string title = "확인")
        {
            return await Task.Run(() => ShowConfirm(message, title));
        }

        public string ShowOpenFileDialog(string filter = "All files (*.*)|*.*", string title = "파일 열기")
        {
            var dialog = new OpenFileDialog
            {
                Filter = filter,
                Title = title
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string ShowSaveFileDialog(string filter = "All files (*.*)|*.*", string title = "파일 저장")
        {
            var dialog = new SaveFileDialog
            {
                Filter = filter,
                Title = title
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}