using Hipot_Tester.Model;
using Hipot_Tester.ViewModel.Control;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipot_Tester.ViewModel
{
    public class DataManager : ObservableObject
    {
        private static DataManager instance;
        public static DataManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataManager();
                }
                return instance;
            }
        }

        private ObservableCollection<DataModel> currentTestData;
        public ObservableCollection<DataModel> CurrentTestData
        {
            get => currentTestData;
            set => SetProperty(ref currentTestData, value);
        }

        private ObservableCollection<TestSession> _testHistory;
        public ObservableCollection<TestSession> TestHistory
        {
            get => _testHistory;
            set => SetProperty(ref _testHistory, value);
        }

        private TestSession _selectedSession;
        public TestSession SelectedSession
        {
            get => _selectedSession;
            set
            {
                if (SetProperty(ref _selectedSession, value) && value != null)
                {
                    // 선택된 세션의 데이터를 현재 테스트 데이터로 설정
                    CurrentTestData = new ObservableCollection<DataModel>(value.Data);
                }
            }
        }

        public DataManager()
        {
            currentTestData = new ObservableCollection<DataModel>();
            _testHistory = new ObservableCollection<TestSession>();

            DataStorage.Instance.SessionSaved += OnSessionSaved;
        }

        private void OnSessionSaved(object sender, SessionEventArgs e)
        {
            if (e.Session != null)
            {
                // TestHistory에 새 세션 추가
                TestHistory.Add(e.Session);
                SelectedSession = e.Session;

                // 이벤트 발생
                TestDataUpdated?.Invoke(this, new TestDataEventArgs { Session = e.Session });
            }
        }

        public void UpdateTestData(ObservableCollection<DataModel> newData, string testMode, string device)
        {
            // 현재 테스트 데이터 업데이트
            CurrentTestData = new ObservableCollection<DataModel>(newData);

            // 테스트 세션 저장
            if (newData.Count > 0)
            {
                var session = new TestSession
                {
                    SessionId = Guid.NewGuid(),
                    TestTime = DateTime.Now,
                    TestMode = testMode,
                    DeviceType = device,
                    Data = new ObservableCollection<DataModel>(newData),
                    Result = GetTestResult(newData)
                };

                TestHistory.Add(session);
                SelectedSession = session;

                // 이벤트 발생
                TestDataUpdated?.Invoke(this, new TestDataEventArgs { Session = session });
            }
        }

        private string GetTestResult(ObservableCollection<DataModel> data)
        {
            // 테스트 결과 판정 로직
            // 여기서는 간단한 예시만 구현
            if (data.Count == 0)
                return "N/A";

            var lastItem = data.Last();

            // 여기에 판정 로직 구현
            if (lastItem.Resistance > 1E+6)
                return "PASS";
            else
                return "FAIL";
        }

        public Dictionary<DataClassification, int> CalculateStatistics(TestSession session)
        {
            var result = new Dictionary<DataClassification, int>
            {
                { DataClassification.Valid, 0 },
                { DataClassification.Error, 0 },
                { DataClassification.OutOfRange, 0 },
                { DataClassification.Critical, 0 },
                { DataClassification.Dead, 0 }
            };

            if (session == null || session.Data.Count == 0)
            {
                return result;
            }

            // 데이터 리스트 변환
            var dataList = session.Data.ToList();

            // DEC 값 계산
            var decValues = ChartHelper.CalculateDECValues(dataList);

            // 필터링 수행
            var filteredData = DataFilterExtensions.FilterInvalidData(dataList, decValues);

            // 데이터 분류 수행
            var classifications = DataFilterExtensions.ClassifyData(dataList, filteredData, decValues);

            // 분류별 개수 계산
            foreach (var classification in classifications.Values)
            {
                result[classification]++;
            }

            return result;
        }

        public event EventHandler<TestDataEventArgs> TestDataUpdated;
    }

    public class TestSession : ObservableObject
    {
        public Guid SessionId { get; set; }
        public DateTime TestTime { get; set; }
        public string TestMode { get; set; }
        public string DeviceType { get; set; }
        public ObservableCollection<DataModel> Data { get; set; }
        public string Result { get; set; }

        // 표시용 프로퍼티
        public string DisplayName => $"{TestTime:yyyy-MM-dd HH:mm:ss} - {TestMode} ({Result})";
    }

    public class TestDataEventArgs : EventArgs
    {
        public TestSession Session { get; set; }
    }
}
