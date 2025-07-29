using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using Hipot_Tester.ViewModel;
using Hipot_Tester.ViewModel.Control;

namespace Hipot_Tester.Model
{
    public class DataStorage : ObservableObject
    {
        private static DataStorage _instance;
        private static readonly object _lock = new object();

        public static DataStorage Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DataStorage();
                        }
                    }
                }
                return _instance;
            }
        }

        private ObservableCollection<TestSession> _storedSessions;
        public ObservableCollection<TestSession> StoredSessions
        {
            get => _storedSessions;
            private set => SetProperty(ref _storedSessions, value);
        }

        public event EventHandler<SessionEventArgs> SessionSaved;

        private DataStorage()
        {
            StoredSessions = new ObservableCollection<TestSession>();
        }

        public void SaveSession(TestSession session)
        {
            if (session == null)
                return;

            StoredSessions.Add(session);
            OnSessionSaved(new SessionEventArgs { Session = session });
        }

        public void RemoveSession(TestSession session)
        {
            if (session != null && StoredSessions.Contains(session))
            {
                StoredSessions.Remove(session);
            }
        }

        public void ClearAllSessions()
        {
            StoredSessions.Clear();
        }

        public bool ExportToExcel(string filePath)
        {
            try
            {
                if (StoredSessions.Count == 0)
                    return false;

                using (var workbook = new XLWorkbook())
                {
                    // 세션별로 워크시트 생성
                    foreach (var session in StoredSessions)
                    {
                        var worksheetName = $"Session_{session.TestTime:yyyyMMdd_HHmmss}";
                        var worksheet = workbook.Worksheets.Add(worksheetName);

                        // 헤더 정보
                        worksheet.Cell(1, 1).Value = "Session Information";
                        worksheet.Cell(2, 1).Value = "Session ID:";
                        worksheet.Cell(2, 2).Value = session.SessionId.ToString();
                        worksheet.Cell(3, 1).Value = "Test Time:";
                        worksheet.Cell(3, 2).Value = session.TestTime.ToString("yyyy-MM-dd HH:mm:ss");
                        worksheet.Cell(4, 1).Value = "Test Mode:";
                        worksheet.Cell(4, 2).Value = session.TestMode;
                        worksheet.Cell(5, 1).Value = "Device Type:";
                        worksheet.Cell(5, 2).Value = session.DeviceType;
                        worksheet.Cell(6, 1).Value = "Result:";
                        worksheet.Cell(6, 2).Value = session.Result;

                        // 데이터 헤더
                        int dataStartRow = 8;
                        worksheet.Cell(dataStartRow, 1).Value = "Index";
                        worksheet.Cell(dataStartRow, 2).Value = "Time";
                        worksheet.Cell(dataStartRow, 3).Value = "Voltage (V)";
                        worksheet.Cell(dataStartRow, 4).Value = "Current (A)";
                        worksheet.Cell(dataStartRow, 5).Value = "Resistance (Ω)";

                        // 데이터 입력
                        for (int i = 0; i < session.Data.Count; i++)
                        {
                            var data = session.Data[i];
                            int row = dataStartRow + 1 + i;

                            worksheet.Cell(row, 1).Value = i + 1;
                            worksheet.Cell(row, 2).Value = data.Time;
                            worksheet.Cell(row, 3).Value = data.Voltage;
                            worksheet.Cell(row, 4).Value = data.Current;
                            worksheet.Cell(row, 5).Value = data.Resistance;
                        }

                        // 자동 열 너비 조정
                        worksheet.Columns().AdjustToContents();
                    }

                    // 요약 시트 생성
                    var summarySheet = workbook.Worksheets.Add("Summary");
                    summarySheet.Cell(1, 1).Value = "Test Sessions Summary";
                    summarySheet.Cell(2, 1).Value = "Total Sessions:";
                    summarySheet.Cell(2, 2).Value = StoredSessions.Count;

                    summarySheet.Cell(4, 1).Value = "Session";
                    summarySheet.Cell(4, 2).Value = "Test Time";
                    summarySheet.Cell(4, 3).Value = "Mode";
                    summarySheet.Cell(4, 4).Value = "Device";
                    summarySheet.Cell(4, 5).Value = "Result";
                    summarySheet.Cell(4, 6).Value = "Data Points";

                    for (int i = 0; i < StoredSessions.Count; i++)
                    {
                        var session = StoredSessions[i];
                        int row = 5 + i;

                        summarySheet.Cell(row, 1).Value = i + 1;
                        summarySheet.Cell(row, 2).Value = session.TestTime.ToString("yyyy-MM-dd HH:mm:ss");
                        summarySheet.Cell(row, 3).Value = session.TestMode;
                        summarySheet.Cell(row, 4).Value = session.DeviceType;
                        summarySheet.Cell(row, 5).Value = session.Result;
                        summarySheet.Cell(row, 6).Value = session.Data.Count;
                    }

                    summarySheet.Columns().AdjustToContents();

                    // 파일 저장
                    workbook.SaveAs(filePath);
                }

                // 내보내기 후 세션 클리어 (선택사항)
                // ClearAllSessions();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Excel export error: {ex.Message}");
                return false;
            }
        }

        protected virtual void OnSessionSaved(SessionEventArgs e)
        {
            SessionSaved?.Invoke(this, e);
        }
    }

    public class SessionEventArgs : EventArgs
    {
        public TestSession Session { get; set; }
    }
}