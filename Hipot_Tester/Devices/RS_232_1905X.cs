using NationalInstruments.Visa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hipot_Tester.Devices
{
    public class RS_232_1905X : VisaSerialDevice
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
            SendMessage("SAFE:START ");
        }
        
        public async Task StartAsync()
        {
            await SendMessageAsync("SAFE:START ");
        }
        
        public override void STOP()
        {
            SendMessage("SAFE:STOP ");
        }
        
        public async Task StopAsync()
        {
            await SendMessageAsync("SAFE:STOP ");
        }
        public string GetMode()
        {
            return QueryMessage("SAFEty:RESult:ALL:MODE? ");
        }
        public override string GetJudgement()
        {
            return QueryMessage("SAFE:RES:LAST? \n");
        }
        public string GetMMET()
        {
            return QueryMessage("SOUR:SAFE:RES:ALL:MMET? ");
        }
        public override string GetData()
        {
            string r = "OMET, MMET, TLEF ";
            return QueryMessage("SAFE:FETC? " + r);
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

        public void SetIRLevel(double v)
        {
            SendMessage("SAFE:STEP1:IR " + v);
        }
        public void SetIRHigh(double v)
        {
            double convertedValue = v * 1000000;
            SendMessage("SAFE:STEP1:IR:LIM:HIGH " + convertedValue);
        }
        public void SetIRLow(double r)
        {
            double convertedValue = r * 1000000;
            SendMessage("SAFE:STEP1:IR:LIM:LOW " + convertedValue);
        }

        public void SetIRTime(double v)
        {
            SendMessage("SAFE:STEP1:IR:TIME " + v);
        }
        public void SetIRRamp(double v)
        {
            SendMessage("SAFE:STEP1:IR:TIME:RAMP " + v);
        }
        public void SetIRFall(double v)
        {
            SendMessage("SAFE:STEP1:IR:TIME:FALL " + v);
        }
        public void SetIRRange(double v)
        {
            SendMessage("SAFE:STEP1:IR:RANG " + v);
        }
    }
}
