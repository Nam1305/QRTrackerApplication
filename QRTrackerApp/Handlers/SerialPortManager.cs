using System;
using System.IO.Ports;

namespace QRTrackerApp.Handlers
{
    public class SerialPortManager : IDisposable
    {
        private SerialPort com4;
        private SerialPort com5;

        public event Action<string> DataReceived;

        public bool InitializePorts()
        {
            try
            {
                if (!SerialPort.GetPortNames().Contains("COM5", StringComparer.OrdinalIgnoreCase))
                    return false;

                com4 = new SerialPort("COM4", 9600, Parity.None, 8, StopBits.One);
                com4.Open();

                com5 = new SerialPort("COM5", 9600, Parity.None, 8, StopBits.One);
                com5.DataReceived += Com5_DataReceived;
                com5.Open();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Com5_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string raw = com5.ReadExisting();
            DataReceived?.Invoke(raw);
        }

        public void WriteToCOM4(string data)
        {
            com4?.Write(data);
        }

        public void ClosePorts()
        {
            if (com5 != null)
            {
                com5.DataReceived -= Com5_DataReceived;
                if (com5.IsOpen) com5.Close();
            }
            if (com4 != null && com4.IsOpen) com4.Close();
        }

        public void Dispose()
        {
            ClosePorts();
            com4?.Dispose();
            com5?.Dispose();
        }
    }
}
