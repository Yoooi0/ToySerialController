using System;
using System.IO.Ports;
using System.Linq;
using UnityEngine;

namespace ToySerialController
{
    public class RSMDevice : AbstractGenericDevice
    {
        private readonly byte[] _buffer = new byte[8];

        public override void Write(SerialPort serial, Vector3 xCmd, Vector3 rCmd, float[] vCmd)
        {
            var servo0f = Mathf.Clamp(8000 - 4000 * xCmd.x - 2000 * (rCmd.y - 0.5f), 4000, 8000);   // Left servo
            var servo1f = Mathf.Clamp(4000 + 4000 * xCmd.x - 2000 * (rCmd.y - 0.5f), 4000, 8000);   // Right servo

            var servo0i = Convert.ToUInt32(servo0f);
            var servo1i = Convert.ToUInt32(servo1f);

            _buffer[0] = 0x84;                                  // Move command identifier
            _buffer[1] = 0x00;                                  // Servo number (left servo - 0)
            _buffer[2] = Convert.ToByte(servo0i & 0x7F);        // First 7 bits of 14-bit position command
            _buffer[3] = Convert.ToByte((servo0i >> 7) & 0x7F); // Second 7 bits of 14-bit position command
            _buffer[4] = 0x84;                                  // Move command identifier
            _buffer[5] = 0x01;                                  // Servo number (right servo - 1)
            _buffer[6] = Convert.ToByte(servo1i & 0x7F);        // First 7 bits of 14-bit position command
            _buffer[7] = Convert.ToByte((servo1i >> 7) & 0x7F); // Second 7 bits of 14-bit position command

            serial.Write(_buffer, 0, _buffer.Length);

            var firstBytes = string.Join(" ", _buffer.Take(4).Select(b => b.ToString("0")).ToArray());
            var lastBytes = string.Join(" ", _buffer.Skip(4).Select(b => b.ToString("0")).ToArray());
            SerialReport = $"{firstBytes}\n{lastBytes}";
        }
    }
}
