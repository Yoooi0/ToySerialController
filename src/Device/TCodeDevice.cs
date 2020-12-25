using System.IO.Ports;
using System.Text;
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

        public override void Write(SerialPort serial)
        {
            var ms = Mathf.RoundToInt(TCodeIntervalSlider.val);
            var interval = ms > 0 ? $"I{ms}" : string.Empty;

            var l0 = $"L0{TCodeNumber(XCmd[0])}{interval}";
            var l1 = $"L1{TCodeNumber(XCmd[1])}{interval}";
            var l2 = $"L2{TCodeNumber(XCmd[2])}{interval}";
            var r0 = $"R0{TCodeNumber(RCmd[0])}{interval}";
            var r1 = $"R1{TCodeNumber(RCmd[1])}{interval}";
            var r2 = $"R2{TCodeNumber(RCmd[2])}{interval}";
            var v0 = $"V0{TCodeNumber(ECmd[0])}{interval}";
            var v1 = $"V1{TCodeNumber(ECmd[1])}{interval}";
            var l3 = $"L3{TCodeNumber(ECmd[2])}{interval}";

            var data = $"{l0} {l1} {l2} {r0} {r1} {r2} {v0} {v1} {l3}\n";
            if(serial?.IsOpen == true)
                serial.Write(data);

            var sb = new StringBuilder();
            sb.Append("        Target   Cmd      Serial\n");
            sb.Append("L0\t").AppendFormat("{0,5:0.00}", XTarget[0]).Append(",\t").AppendFormat("{0,5:0.00}", XCmd[0]).Append(",\t").Append(l0).AppendLine();
            sb.Append("L1\t").AppendFormat("{0,5:0.00}", XTarget[1]).Append(",\t").AppendFormat("{0,5:0.00}", XCmd[1]).Append(",\t").Append(l1).AppendLine();
            sb.Append("L2\t").AppendFormat("{0,5:0.00}", XTarget[2]).Append(",\t").AppendFormat("{0,5:0.00}", XCmd[2]).Append(",\t").Append(l2).AppendLine();
            sb.Append("R0\t").AppendFormat("{0,5:0.00}", RTarget[0]).Append(",\t").AppendFormat("{0,5:0.00}", RCmd[0]).Append(",\t").Append(r0).AppendLine();
            sb.Append("R1\t").AppendFormat("{0,5:0.00}", RTarget[1]).Append(",\t").AppendFormat("{0,5:0.00}", RCmd[1]).Append(",\t").Append(r1).AppendLine();
            sb.Append("R2\t").AppendFormat("{0,5:0.00}", RTarget[2]).Append(",\t").AppendFormat("{0,5:0.00}", RCmd[2]).Append(",\t").Append(r2).AppendLine();
            sb.Append("V0\t").AppendFormat("{0,5:0.00}", ETarget[0]).Append(",\t").AppendFormat("{0,5:0.00}", ECmd[0]).Append(",\t").Append(v0).AppendLine();
            sb.Append("V1\t").AppendFormat("{0,5:0.00}", ETarget[1]).Append(",\t").AppendFormat("{0,5:0.00}", ECmd[1]).Append(",\t").Append(v1).AppendLine();
            sb.Append("L3\t").AppendFormat("{0,5:0.00}", ETarget[2]).Append(",\t").AppendFormat("{0,5:0.00}", ECmd[2]).Append(",\t").Append(l3);
            DeviceReport = sb.ToString();
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
