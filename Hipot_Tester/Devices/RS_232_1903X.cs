using NationalInstruments.Visa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hipot_Tester.Devices
{
    public class RS_232_1903X : VisaSerialDevice
    {
        public override bool IsConnected => !string.IsNullOrEmpty(GetIDN());

        public override void SetSession(SerialSession session)
        {
            //session.SendEndEnabled = true;
            session.TerminationCharacterEnabled = false;
            session.SendEndEnabled = false;
            base.SetSession(session);
        }

        public string GetIDN()
        {
            SendMessage("*IDN?");
            Thread.Sleep(100); // TODO: 비동기 버전 사용 권장
            return ReceiveMessage();
        }
        
        public async Task<string> GetIDNAsync()
        {
            return await QueryMessageAsync("*IDN?");
        }
        
        public override void START()
        {
            SendMessage("SOURce:SAFEty:START ");
        }
        
        public async Task StartAsync()
        {
            await SendMessageAsync("SOURce:SAFEty:START ");
        }
        
        public override void STOP()
        {
            SendMessage("SOURce:SAFEty:STOP ");
        }
        
        public async Task StopAsync()
        {
            await SendMessageAsync("SOURce:SAFEty:STOP ");
        }
        public string GetStepNumber()
        {
            return QueryMessage("SOURce:SAFEty:SNUMBer? ");
        }
        public string GetMode()
        {
            return QueryMessage("SAFEty:RESult:ALL:MODE? ");
        }
        public override string GetJudgement()
        {
            return QueryMessage("SOURce:SAFEty:RESult:ALL:JUDGment? ");
        }
        public string GetMMET()
        {
            return QueryMessage("SOUR:SAFE:RES:ALL:MMET? ");
        }
        public override string GetData()
        {
            string r = "OMET, MMET, TLEA ";
            return QueryMessage("SAF:FETC? " + r);
        }

        private string hipotStatus = "";

        public string GetStatus()
        {
            string response = QueryMessage("SAFE:STAT? ");
            return response.Trim(); 
        }
        public bool AutoRangeONOFF(bool isOn)
        {
            string r = isOn ? " ON" : " OFF";  // ON 또는 OFF 설정
            SendMessage("SYST:TCON:WRAN" + r);
            return isOn;
        }
        public bool SetautoRange(bool isOn)
        {
            string r = isOn ? " ON" : " OFF";  // ON 또는 OFF 설정
            SendMessage("SYST:STEP1:IR:RANG:AUTO" + r);
            return isOn;
        }

        public string Status()
        {
            return hipotStatus; // 저장된 상태 반환
        }

        // Range Settings
        public void SetFixRange(double range)
        {
            SendMessage($"SAF:STEP1:IR:RANG:FIX {range}");
        }

        public void SetAutoRange(bool enable)
        {
            string state = enable ? "ON" : "OFF";
            SendMessage($"SAF:STEP1:IR:RANG:AUTO {state}");
        }

        public string GetCurrentRange()
        {
            return QueryMessage("SAF:STEP1:IR:RANG?");
        }

        // IR Mode Settings
        public void SetIRLevel(double v)
        {
            SendMessage("SAF:STEP1:IR " + v);
        }
        public void SetIRHigh(double v)
        {
            double convertedValue = v * 1000000;
            SendMessage("SAF:STEP1:IR:LIM:HIGH " + convertedValue);
        }
        public void SetIRLow(double r)
        {
            double convertedValue = r * 1000000; 
            SendMessage("SAF:STEP1:IR:LIM:LOW " + convertedValue);
        }

        public void SetIRTime(double v)
        {
            SendMessage("SAF:STEP1:IR:TIME " + v);
        }
        public void SetIRRamp(double v)
        {
            SendMessage("SAF:STEP1:IR:TIME:RAMP " + v);
        }
        public void SetIRDwell(double v)
        {
            SendMessage("SAF:STEP1:IR:TIME:DWEL " + v);
        }
        public void SetIRFall(double v)
        {
            SendMessage("SAF:STEP1:IR:TIME:FALL " + v);
        }
        public void SetIRRange(double v)
        {
            SendMessage("SAF:STEP1:IR:RANG " + v);
        }
    }
}
