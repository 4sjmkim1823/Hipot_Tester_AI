using Hipot_Tester.ViewModel.Control;
using Hipot_Tester.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace Hipot_Tester.ViewModel
{
    public class DataManagementViewModel : ObservableObject
    {
        // 저장된 세션 목록
        private ObservableCollection<TestSession> _storedSessions;
        public ObservableCollection<TestSession> StoredSessions
        {
            get => _storedSessions;
            set => SetProperty(ref _storedSessions, value);
        }

        // 선택된 세션
        private TestSession _selectedSession;
        public TestSession SelectedSession
        {
            get => _selectedSession;
            set => SetProperty(ref _selectedSession, value);
        }

        // 세션 번호 표시
        private int sessionCount;
        public int SessionCount
        {
            get { return sessionCount; }
            set { SetProperty(ref sessionCount, value); }
        }

        // 명령
        public ICommand DeleteSessionCommand { get; }
        public ICommand ExportToExcelCommand { get; }
        public ICommand CloseCommand { get; }

        // 생성자
        public DataManagementViewModel()
        {
            // 세션 목록 초기화
            StoredSessions = DataStorage.Instance.StoredSessions;
            SessionCount = StoredSessions.Count;

            // 첫 번째 세션 선택 (있는 경우)
            if (StoredSessions.Count > 0)
            {
                SelectedSession = StoredSessions[0];
            }

            // 명령 초기화
            DeleteSessionCommand = new RelayCommand(DeleteSession, CanDeleteSession);
            ExportToExcelCommand = new RelayCommand(ExportToExcel, CanExportToExcel);
            CloseCommand = new RelayCommand(Close);

            DataStorage.Instance.SessionSaved += OnSessionSaved;
        }

        private void OnSessionSaved(object sender, SessionEventArgs e)
        {
            // 세션 카운트 업데이트
            SessionCount = StoredSessions.Count;
        }

        // 세션 삭제
        private void DeleteSession(object parameter)
        {
            if (SelectedSession != null)
            {
                var result = MessageBox.Show(
                    "선택한 테스트 세션을 정말 삭제하시겠습니까?",
                    "세션 삭제 확인",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);


                if (result == MessageBoxResult.Yes)
                {
                    // 선택된 세션 삭제
                    StoredSessions.Remove(SelectedSession);
                    SessionCount = StoredSessions.Count;

                    // 다음 세션 선택
                    if (StoredSessions.Count > 0)
                    {
                        SelectedSession = StoredSessions[0];
                    }
                    else
                    {
                        SelectedSession = null;
                    }
                }
            }
        }

        // 세션 삭제 가능 여부
        private bool CanDeleteSession(object parameter)
        {
            return SelectedSession != null;
        }

        // 엑셀로 내보내기
        private void ExportToExcel(object parameter)
        {
            try
            {
                if (StoredSessions.Count == 0)
                {
                    MessageBox.Show("내보낼 테스트 데이터가 없습니다.", "알림",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"HipotTest_Export_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx");

                bool success = DataStorage.Instance.ExportToExcel(filePath);

                if (success)
                {
                    MessageBox.Show($"테스트 데이터가 성공적으로 내보내기 되었습니다.\n저장 위치: {filePath}",
                        "내보내기 완료", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 세션 목록 갱신 (모든 세션이 엑셀로 내보내져서 DataStorage에서 삭제됨)
                    StoredSessions = DataStorage.Instance.StoredSessions;
                    SessionCount = StoredSessions.Count;
                    SelectedSession = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"엑셀 내보내기 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 내보내기 가능 여부
        private bool CanExportToExcel(object parameter)
        {
            return StoredSessions.Count > 0;
        }

        // 대화상자 닫기
        private void Close(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
        }
    }
}
