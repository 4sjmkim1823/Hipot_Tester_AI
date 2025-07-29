// 모든 Chroma 장치의 공통 인터페이스
using Hipot_Tester.Model;
using System.Threading.Tasks;

namespace Hipot_Tester.Test.Interfaces
{
    public interface IDeviceCommunication
    {
        Task<bool> ConnectAsync(string connectionString);
        Task<bool> DisconnectAsync();
        Task<bool> IsConnectedAsync();
        Task<string> ExecuteCommandAsync(string command);
    }

    // 테스트 모드를 위한 인터페이스
    

    // 장치별 고유 기능을 위한 확장 인터페이스
    public interface IChroma1903xService : IDeviceCommunication, ITestModeService
    {
        // 1903x 시리즈 특화 기능
    }

    public interface IChroma1905xService : IDeviceCommunication, ITestModeService
    {
        // 1905x 시리즈 특화 기능
    }

    public interface IChroma11210Service : IDeviceCommunication, ITestModeService
    {
        // 11210 특화 기능
    }

    public interface IChroma11210KService : IChroma11210Service
    {
        // 11210-K 특화 기능
    }
    // 테스트 장치 모델
    

    // 테스트 설정 모델
    

    // 테스트 결과 모델
    

    // 열거형 정의
    public enum DeviceType
    {
        Chroma1903x,
        Chroma1905x,
        Chroma11210,
        Chroma11210K
    }

    public enum ConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected,
        Error
    }

    public enum TestMode
    {
        AC,
        DC,
        IR
    }

    public enum RangeType
    {
        Auto,
        Fixed
    }

    public enum CurrentRange
    {
        Range10mA,
        Range3mA,
        Range300uA,
        Range30uA,
        Range3uA,
        Range300nA
    }
}