using System.IO.Ports;
using ToySerialController.UI;
using UnityEngine;

namespace ToySerialController
{
    public class TCodeDevice : AbstractGenericDevice
    {
        private JSONStorableFloat TCodeIntervalScaleSlider;

        public override void CreateCustomUI(UIGroup group)
        {
            TCodeIntervalScaleSlider = group.CreateSlider("Device:TCodeIntervalScale", "TCode Interval Scale", 0, 0, 2, true, true, true);
        }

        public override void Write(SerialPort serial, Vector3 xCmd, Vector3 rCmd, float[] vCmd)
        {
            var ms = Mathf.RoundToInt(Time.fixedDeltaTime * 1000 * TCodeIntervalScaleSlider.val);
            var interval = ms > 0 ? $"I{ms}" : string.Empty;

            var l0 = $"L0{TCodeNumber(xCmd.x)}{interval}";
            var l1 = $"L1{TCodeNumber(xCmd.y)}{interval}";
            var l2 = $"L2{TCodeNumber(xCmd.z)}{interval}";
            var r0 = $"R0{TCodeNumber(rCmd.x)}{interval}";
            var r1 = $"R1{TCodeNumber(rCmd.y)}{interval}";
            var r2 = $"R2{TCodeNumber(rCmd.z)}{interval}";
            var v0 = $"V0{TCodeNumber(vCmd[0])}{interval}";
            var v1 = $"V1{TCodeNumber(vCmd[1])}{interval}";

            var data = $"{l0} {l1} {l2} {r0} {r1} {r2} {v0} {v1}\n";
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
