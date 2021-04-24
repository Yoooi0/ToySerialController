using DebugUtils;
using System;
using System.IO.Ports;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace ToySerialController
{
    public class UdpSerial : SerialWrapper
    {
        private string _udpAddress;
        private string _udpPort;
        public bool _isConnected;
        private bool _isConnecting;
        private UdpClient _udpClient;
        
        public UdpSerial(string address, string port) : base("", 0)
        {
            SuperController.LogMessage("UdpSerial init " + address + ":" + port);
            _isConnected = false;
            _isConnecting = false;
            _udpAddress = address;
            _udpPort = port;
        }
        
        public override void Open()
        {
            SuperController.LogMessage("UdpSerial try open");
            try
            {
                if (!_isConnected)
                {
                    setNetworkStatus(true);
                    _isConnecting = true;
                    IPEndPoint tcodeIPEndPoint = CreateIPEndPoint(_udpAddress + ":" + _udpPort);
                    SuperController.LogMessage("UdpSerial new UDP");
					_udpClient = new UdpClient();
                    SuperController.LogMessage("UdpSerial connect");
                    _udpClient.Connect(tcodeIPEndPoint);
					var handshake = System.Text.Encoding.ASCII.GetBytes("D1\n");
					//Log.Debug(DateTime.Now + " Sending udp connection handshake");
                    SuperController.LogMessage("UdpSerial send handshake");
					_udpClient.Send(handshake, handshake.Length);
					_udpClient.BeginReceive(ReceiveCallback, new UdpState() {udp = _udpClient, ip = tcodeIPEndPoint});
                }
			}
            catch (Exception e)
            {
                SuperController.LogError("UDP Exception: " + e);
            }
        }
        
        public override void Close()
        {
 			_isConnecting = false;
			_isConnected = false;
			if(_udpClient != null)
				_udpClient.Close();
            SuperController.LogMessage("UDP connection stopped");           
        }
        
        public override void Write(string tcode)
        {
			var tcodeBytes = System.Text.Encoding.ASCII.GetBytes(tcode);
			_udpClient.Send(tcodeBytes, tcodeBytes.Length);
        }
        
        public override bool IsOpen()
        {
            return _isConnected;
        }
    
        private class UdpState {
			public UdpClient udp;
			public IPEndPoint ip;
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
            try
            {
				UdpClient u = ((UdpState)(ar.AsyncState)).udp;
				IPEndPoint e = ((UdpState)(ar.AsyncState)).ip;

				byte[] receiveBytes = u.EndReceive(ar, ref e);
				string receiveString = System.Text.Encoding.ASCII.GetString(receiveBytes);

            	SuperController.LogMessage(receiveString);
				if (receiveString.Contains("TCode"))
				{
					_isConnected = true;
				}
				_isConnecting = false;
				setNetworkStatus();
			}
            catch (Exception e)
            {
                SuperController.LogError("UDP callback Exception: " + e);
            }
		}
		private void setNetworkStatus(bool connecting = false) {
        //    networkAddress.val = _defaultIPAddress + ":" + _defaultPort + "\n" + (connecting ? "Connecting..." : (_isConnected ? "Connected" : "Not connected"));
		}
		// Handles IPv4 and IPv6 notation.
		public static IPEndPoint CreateIPEndPoint(string endPoint)
		{
			string[] ep = endPoint.Split(':');
			if (ep.Length < 2) throw new FormatException("Invalid endpoint format");
			IPAddress ip;
			if (ep.Length > 2)
			{
				if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip))
				{
					throw new FormatException("Invalid ip-adress");
				}
			}
			else
			{
				if (!IPAddress.TryParse(ep[0], out ip))
				{
					throw new FormatException("Invalid ip-adress");
				}
			}
			int port;
			if (!int.TryParse(ep[ep.Length - 1], System.Globalization.NumberStyles.None, System.Globalization.NumberFormatInfo.CurrentInfo, out port))
			{
				throw new FormatException("Invalid port");
			}
			return new IPEndPoint(ip, port);
		}
    }
}
