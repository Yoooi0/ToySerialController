using System.IO.Ports;
using UnityEngine;

namespace ToySerialController
{
    public class TCodeDevice : AbstractGenericDevice
    {
        public override void Write(SerialPort serial, Vector3 xCmd, Vector3 rCmd, float[] vCmd)
        {
            var l0 = TCodeNumber(xCmd.x);
            var l1 = TCodeNumber(xCmd.y);
            var l2 = TCodeNumber(xCmd.z);
            var r0 = TCodeNumber(rCmd.x);
            var r1 = TCodeNumber(rCmd.y);
            var r2 = TCodeNumber(rCmd.z);
            var v0 = TCodeNumber(vCmd[0]);
            var v1 = TCodeNumber(vCmd[1]);

            var data = $"L0{l0} L1{l1} L2{l2} R0{r0} R1{r1} R2{r2} V0{v0} V1{v1}\n";
            serial.Write(data);
            SerialReport = data.Replace(' ', '\n');
        }

        private string TCodeNumber(float input)
        {
            input = Mathf.Clamp(input, 0, 1) * 1000;

            if (input >= 999f) return "999";
            else if (input >= 1f) return input.ToString("000");
            return "000";
        }
    }
}
