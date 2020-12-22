using System.IO.Ports;
using ToySerialController.UI;
using UnityEngine;

namespace ToySerialController
{
    public class TCodeDevice : AbstractGenericDevice
    {
        private JSONStorableFloat TCodeIntervalSlider;

        public override void CreateCustomUI(UIGroup group)
        {
            TCodeIntervalSlider = group.CreateSlider("Device:TCodeInterval", "TCode Interval (ms)", 3, 0, 16, true, true, true, "F0");
        }

        public override void Write(SerialPort serial, float[] xCmd, float[] rCmd, float[] eCmd)
        {
            var ms = Mathf.RoundToInt(TCodeIntervalSlider.val);
            var interval = ms > 0 ? $"I{ms}" : string.Empty;

            var l0 = $"L0{TCodeNumber(xCmd[0])}{interval}";
            var l1 = $"L1{TCodeNumber(xCmd[1])}{interval}";
            var l2 = $"L2{TCodeNumber(xCmd[2])}{interval}";
            var r0 = $"R0{TCodeNumber(rCmd[0])}{interval}";
            var r1 = $"R1{TCodeNumber(rCmd[1])}{interval}";
            var r2 = $"R2{TCodeNumber(rCmd[2])}{interval}";
            var v0 = $"V0{TCodeNumber(eCmd[0])}{interval}";
            var v1 = $"V1{TCodeNumber(eCmd[1])}{interval}";
            var l3 = $"L3{TCodeNumber(eCmd[2])}{interval}";

            var data = $"{l0} {l1} {l2} {r0} {r1} {r2} {v0} {v1} {l3}\n";
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
