using Ivi.Visa;
using NationalInstruments.Visa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hipot_Tester.Devices
{
    public abstract class VisaSerialDevice
    {

        public abstract void START();
        public abstract void STOP();
        public abstract string GetData();
        public abstract string GetJudgement();

        public Exception LastException;
        public int BaudRate { get => Session.BaudRate; set => Session.BaudRate = value; }
        public short DataBits { get => Session.DataBits; set => Session.DataBits = value; }
        public SerialParity Parity { get => Session.Parity; set => Session.Parity = value; }
        public SerialStopBitsMode StopBits { get => Session.StopBits; set => Session.StopBits = value; }
        public SerialFlowControlModes FlowControl { get => Session.FlowControl; set => Session.FlowControl = value; }
        public bool IsDisposed => Session != null && Session.IsDisposed;
        public abstract bool IsConnected { get; }
        protected SerialSession Session;
        public virtual void SetSession(SerialSession session)
        {
            Session = session;

            // 0x0A가 종료 문자로 인식되지 않도록 설정
            session.TerminationCharacterEnabled = false; // 종료 문자 기능 비활성화
            session.SendEndEnabled = false; // 메시지 끝 자동 전송 비활성화

            // 혹시나 0x0A가 잘리는 문제가 여전히 발생한다면,
            // TerminationCharacter를 다른 값으로 설정
            //session.TerminationCharacter = (byte)'\r'; // '\r'로 변경

        }

        public virtual SerialSession GetSession() => Session;

        public void Dispose()
        {
            if (Session != null && IsDisposed == false)
            {
                Session.Dispose();
            }
        }
        public bool GetConnected()
        {
            return true; //Session.Connected; => INVALID가 IO Trace에서 나오는 이유를 모르겠음. 
        }

        public virtual void SendMessage(string msg)
        {
            Session.FormattedIO.WriteLine(msg);
        }
        
        protected virtual async Task SendMessageAsync(string msg)
        {
            await Task.Run(() => Session.FormattedIO.WriteLine(msg));
        }
        
        protected virtual string ReceiveMessage()
        {
            return Session.FormattedIO.ReadLine();
        }
        
        protected virtual async Task<string> ReceiveMessageAsync()
        {
            return await Task.Run(() => Session.FormattedIO.ReadLine());
        }
        
        public virtual string QueryMessage(string msg)
        {
            SendMessage(msg);
            return ReceiveMessage();
        }
        
        protected virtual async Task<string> QueryMessageAsync(string msg)
        {
            await SendMessageAsync(msg);
            await Task.Delay(100); // Thread.Sleep 대신 비동기 지연
            return await ReceiveMessageAsync();
        }

        protected virtual void SendByte(byte[] bytes)
        {
            Session.RawIO.Write(bytes);
        }
        
        protected virtual async Task SendByteAsync(byte[] bytes)
        {
            await Task.Run(() => Session.RawIO.Write(bytes));
        }
        
        protected virtual byte[] ReceiveByte()
        {
            return Session.RawIO.Read();
        }
        
        protected virtual async Task<byte[]> ReceiveByteAsync()
        {
            return await Task.Run(() => Session.RawIO.Read());
        }
        
        protected virtual byte[] QueryByte(byte[] bytes)
        {
            SendByte(bytes);
            return ReceiveByte();
        }
        
        protected virtual async Task<byte[]> QueryByteAsync(byte[] bytes)
        {
            await SendByteAsync(bytes);
            return await ReceiveByteAsync();
        }

        protected virtual byte[] ReceiveByte(int length)
        {
            return Session.RawIO.Read(length);
        }
        
        protected virtual async Task<byte[]> ReceiveByteAsync(int length)
        {
            return await Task.Run(() => Session.RawIO.Read(length));
        }

        protected virtual byte[] QueryByte(byte[] bytes, int length)
        {
            SendByte(bytes);
            Thread.Sleep(70);
            return ReceiveByte(length);
        }
        
        protected virtual async Task<byte[]> QueryByteAsync(byte[] bytes, int length)
        {
            await SendByteAsync(bytes);
            await Task.Delay(70); // Thread.Sleep 대신 비동기 지연
            return await ReceiveByteAsync(length);
        }

    }
}
