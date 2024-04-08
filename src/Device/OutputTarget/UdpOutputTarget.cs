using SimpleJSON;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;

namespace ToySerialController.Device.OutputTarget
{
    public class UdpOutputTarget : IOutputTarget
    {
        private UITextInput AddressInput;
        private UITextInput PortInput;
        private JSONStorableString IpText;
        private JSONStorableString PortText;
        private UIHorizontalGroup ButtonGroup;

        private JSONStorableAction StartUdpAction;
        private JSONStorableAction StopUdpAction;

        private UdpClient _client;

        public void CreateUI(IUIBuilder builder)
        {
            AddressInput = builder.CreateTextInput("OutputTarget:Udp:Address", "Address:", "tcode.local", 50);
            PortInput = builder.CreateTextInput("OutputTarget:Udp:Port", "Port:", "8000", 50);
            IpText = AddressInput.storable;
            PortText = PortInput.storable;

            ButtonGroup = builder.CreateHorizontalGroup(510, 50, new Vector2(10, 0), 2, idx => builder.CreateButtonEx());
            var startSerialButton = ButtonGroup.items[0].GetComponent<UIDynamicButton>();
            startSerialButton.label = "Start Udp";
            startSerialButton.button.onClick.AddListener(StartUdp);

            var stopSerialButton = ButtonGroup.items[1].GetComponent<UIDynamicButton>();
            stopSerialButton.label = "Stop Udp";
            stopSerialButton.button.onClick.AddListener(StopUdp);

            StartUdpAction = UIManager.CreateAction("Start Udp", StartUdp);
            StopUdpAction = UIManager.CreateAction("Stop Udp", StopUdp);
        }

        public void DestroyUI(IUIBuilder builder)
        {
            builder.Destroy(AddressInput);
            builder.Destroy(PortInput);
            builder.Destroy(ButtonGroup);

            UIManager.RemoveAction(StartUdpAction);
            UIManager.RemoveAction(StopUdpAction);
        }

        public void RestoreConfig(JSONNode config)
        {
            config.Restore(IpText);
            config.Restore(PortText);
        }

        public void StoreConfig(JSONNode config)
        {
            config.Store(IpText);
            config.Store(PortText);
        }

        private void StartUdp()
        {
            if (_client != null)
                return;

            try
            {
                var address = IPAddress.Parse(IpText.val);
                var port = int.Parse(PortText.val);
                var endpoint = new IPEndPoint(address, port);
                _client = new UdpClient
                {
                    ExclusiveAddressUse = false
                };

                _client.Client.Blocking = false;
                _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _client.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
                _client.Connect(endpoint);

                SuperController.LogMessage($"Upd started on port: {port}");
            }
            catch (Exception e)
            {
                SuperController.LogError(e.ToString());
                StopUdp();
            }
        }

        private void StopUdp()
        {
            if (_client == null)
                return;

            try
            {
                _client.Close();
            }
            catch(Exception e)
            {
                SuperController.LogError(e.ToString());
            }

            SuperController.LogMessage("Upd stopped");
            _client = null;
        }


        public void Write(string data)
        {
            if (_client == null)
                return;

            var bytes = Encoding.ASCII.GetBytes(data);
            var sent = _client.Send(bytes, bytes.Length);
        }

        protected virtual void Dispose(bool disposing)
        {
            StopUdp();
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
