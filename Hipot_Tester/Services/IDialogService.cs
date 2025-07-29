using System.Threading.Tasks;

namespace Hipot_Tester.Services
{
    public interface IDialogService
    {
        void ShowMessage(string message, string title = "알림");
        void ShowError(string message, string title = "오류");
        void ShowWarning(string message, string title = "경고");
        bool ShowConfirm(string message, string title = "확인");
        
        Task ShowMessageAsync(string message, string title = "알림");
        Task ShowErrorAsync(string message, string title = "오류");
        Task ShowWarningAsync(string message, string title = "경고");
        Task<bool> ShowConfirmAsync(string message, string title = "확인");
        
        string ShowOpenFileDialog(string filter = "All files (*.*)|*.*", string title = "파일 열기");
        string ShowSaveFileDialog(string filter = "All files (*.*)|*.*", string title = "파일 저장");
    }
}