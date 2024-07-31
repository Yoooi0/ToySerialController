using Leap.Unity;
using MVR.FileManagementSecure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ToySerialController.UI;
using UnityEngine;

namespace ToySerialController
{
    public class BinaryDeviceRecorder : IDeviceRecorder
    {
        private JSONStorableFloat RecordingRDPEpsilon;
        private UIHorizontalGroup RecordButtonGroup;
        private JSONStorableStringChooser RecordingTypePopup;
        private JSONStorableStringChooser TimeSourcePopup;

        private const int _bytesPerTick = 7 * 4;
        private const int _maxTicksPerChunk = 16384;
        private const int _bufferSize = _bytesPerTick * _maxTicksPerChunk;

        private bool _isRecording;
        private byte[][] _buffers;
        private int _writeBufferIndex;
        private int _bufferIndex;
        private int _chunkIndex;
        private string _recordingPrefix;
        private float _startTime;

        public void CreateUI(IUIBuilder builder)
        {
            RecordButtonGroup = builder.CreateHorizontalGroup(510, 50, new Vector2(10, 0), 2, idx => builder.CreateButtonEx());
            var startButton = RecordButtonGroup.items[0].GetComponent<UIDynamicButton>();
            startButton.buttonText.fontSize = 25;
            startButton.label = "Start Recording";
            startButton.buttonColor = new Color(0.309f, 1f, 0.039f) * 0.8f;
            startButton.textColor = Color.white;
            startButton.button.onClick.AddListener(StartRecordingCallback);

            var stopButton = RecordButtonGroup.items[1].GetComponent<UIDynamicButton>();
            stopButton.buttonText.fontSize = 25;
            stopButton.label = "Stop Recording";
            stopButton.buttonColor = new Color(1f, 0.168f, 0.039f) * 0.8f;
            stopButton.textColor = Color.white;
            stopButton.button.onClick.AddListener(StopRecordingCallback);

            RecordingTypePopup = builder.CreatePopup("Recording:RecordingType", "Recording Type", new List<string>() { "Per Frame", "Per Physics Tick" }, "Per Frame", null);
            TimeSourcePopup = builder.CreatePopup("Recording:TimeSource", "Time Source", new List<string>() { "Game Time", "Real Time" }, "Game Time", null);
            RecordingRDPEpsilon = builder.CreateSlider("Recording:RamerDouglasPeucker:Epsilon", "RDP Epsilon", 0.001f, 0, 1, true, true, valueFormat: "F6");
        }

        public void DestroyUI(IUIBuilder builder)
        {
            builder.Destroy(RecordButtonGroup);
            builder.Destroy(RecordingTypePopup);
            builder.Destroy(TimeSourcePopup);
            builder.Destroy(RecordingRDPEpsilon);
        }

        protected void StartRecordingCallback() => StartRecording();
        protected void StopRecordingCallback() => StopRecording();

        public void StartRecording()
        {
            if (_isRecording)
                return;

            if (!FileManagerSecure.DirectoryExists(Plugin.PluginDir))
            {
                FileManagerSecure.CreateDirectory(Plugin.PluginDir, StartRecording, null, null);
                return;
            }

            _isRecording = true;
            _recordingPrefix = $"recording_{DateTime.Now:yyyyMMddTHHmmssfff}";
            _startTime = CurrentTime();

            InitializeBuffer();
            SuperController.LogMessage($"Started recording: {_recordingPrefix}");
        }

        public void StopRecording()
        {
            if (!_isRecording)
                return;

            SaveChunk();
            SuperController.LogMessage($"Stopped recording {_recordingPrefix}");

            _isRecording = false;
            _buffers = null;
            _writeBufferIndex = 0;
            _bufferIndex = 0;
            _chunkIndex = 0;

            try
            {
                GenerateFunscripts();
            }
            catch (Exception e)
            {
                SuperController.LogError($"Failed to generate funscripts: {e}");
            }

            _recordingPrefix = null;
        }

        public void RecordValues(float l0, float l1, float l2, float r0, float r1, float r2)
        {
            if (!_isRecording)
                return;

            if (Time.inFixedTimeStep && RecordingTypePopup.val != "Per Physics Tick")
                return;
            if (!Time.inFixedTimeStep && RecordingTypePopup.val != "Per Frame")
                return;

            var buffer = _buffers[_writeBufferIndex];
            BitConverterNonAlloc.GetBytes(CurrentTime() - _startTime, buffer, ref _bufferIndex);
            BitConverterNonAlloc.GetBytes(l0,   buffer, ref _bufferIndex);
            BitConverterNonAlloc.GetBytes(l1,   buffer, ref _bufferIndex);
            BitConverterNonAlloc.GetBytes(l2,   buffer, ref _bufferIndex);
            BitConverterNonAlloc.GetBytes(r0,   buffer, ref _bufferIndex);
            BitConverterNonAlloc.GetBytes(r1,   buffer, ref _bufferIndex);
            BitConverterNonAlloc.GetBytes(r2,   buffer, ref _bufferIndex);

            if (_bufferIndex == buffer.Length)
                SaveChunk();
        }

        private float CurrentTime()
        {
            if (TimeSourcePopup.val == "Real Time")
                return Time.realtimeSinceStartup;
            if (TimeSourcePopup.val == "Game Time")
                return Time.time;
            return 0;
        }

        private void InitializeBuffer()
        {
            _buffers = new byte[][] { new byte[_bufferSize], new byte[_bufferSize] };
            _writeBufferIndex = 0;
            _bufferIndex = 0;
            _chunkIndex = 0;
        }

        private void SaveChunk()
        {
            var chunkPath = $"{Plugin.PluginDir}/{_recordingPrefix}.{_chunkIndex}.bin";
            var saveBuffer = _buffers[_writeBufferIndex];
            var saveLength = _bufferIndex;

            SuperController.LogMessage($"Saving chunk #{_chunkIndex} with {_bufferIndex} bytes");
            if (saveLength == saveBuffer.Length)
                ThreadPool.QueueUserWorkItem(_ => FileManagerSecure.WriteAllBytes(chunkPath, saveBuffer));
            else
                FileManagerSecure.WriteAllBytes(chunkPath, saveBuffer.Take(saveLength).ToArray());

            _writeBufferIndex = 1 - _writeBufferIndex;
            _bufferIndex = 0;
            _chunkIndex++;
        }

        private void GenerateFunscripts()
        {
            var recordingFiles = FileManagerSecure.GetFiles(Plugin.PluginDir, $"*{_recordingPrefix}.*.bin");
            if (recordingFiles.Length == 0)
            {
                SuperController.LogMessage("No recording files found?");
                return;
            }

            var recordingData = new byte[recordingFiles.Length * _bufferSize];
            var recordingSize = 0;
            foreach (var file in recordingFiles)
            {
                var bytes = FileManagerSecure.ReadAllBytes(file);
                Buffer.BlockCopy(bytes, 0, recordingData, recordingSize, bytes.Length);
                recordingSize += bytes.Length;
            }

            if (recordingSize <= 3)
                return;

            Array.Resize(ref recordingData, recordingSize);

            var tickCount = recordingSize / _bytesPerTick;
            var xs = new float[tickCount];
            for (var i = 0; i < tickCount; i++)
                xs[i] = BitConverterNonAlloc.ToSingle(recordingData, i * _bytesPerTick) - xs[0];
            xs[0] = 0;

            var axisNames = new Dictionary<string, int>() { ["L0"] = 1, ["L1"] = 2, ["L2"] = 3, ["R0"] = 4, ["R1"] = 5, ["R2"] = 6 };
            var ys = new float[tickCount];
            var sb = new StringBuilder();
            foreach (var axis in axisNames)
            {
                SuperController.LogMessage($"Generating {axis.Key} funscript");

                var byteOffset = axis.Value * 4;
                for (var i = 0; i < tickCount; i++)
                    ys[i] = BitConverterNonAlloc.ToSingle(recordingData, i * _bytesPerTick + byteOffset);

                sb.Length = 0;
                var removedPoints = 0;
                var result = RamerDouglasPeucker(xs, ys, RecordingRDPEpsilon.val);
                for (var i = 0; i < tickCount; i++)
                {
                    if (!result[i])
                    {
                        removedPoints++;
                        continue;
                    }

                    if (sb.Length != 0)
                        sb.Append(",");
                    else
                        sb.Append("{\"actions\":[");

                    sb.Append("{\"at\":")
                      .Append((int)(xs[i] * 1000))
                      .Append(",\"pos\":")
                      .Append(Mathf.Clamp01(ys[i]) * 100)
                      .Append("}");
                }

                sb.Append("]}");

                SuperController.LogMessage($"Optimized {removedPoints} points");
                FileManagerSecure.WriteAllText($"{Plugin.PluginDir}/{_recordingPrefix}.{axis.Key}.funscript", sb.ToString());
            }
        }

        private BitArray RamerDouglasPeucker(float[] xs, float[] ys, float epsilon)
        {
            var result = new BitArray(xs.Length, true);
            if (epsilon == 0)
                return result;

            var stack = new Stack<KeyValuePair<int, int>>();
            stack.Push(new KeyValuePair<int, int>(0, xs.Length - 1));

            while (stack.Count > 0)
            {
                var item = stack.Pop();
                var startIndex = item.Key;
                var endIndex = item.Value;

                var maxDistance = 0f;
                var maxIndex = startIndex;
                for (var i = maxIndex + 1; i < endIndex; i++)
                {
                    if (!result[i])
                        continue;

                    var distance = PointLineDistance(xs, ys, i, startIndex, endIndex);
                    if (distance > maxDistance)
                    {
                        maxIndex = i;
                        maxDistance = distance;
                    }
                }

                if (maxDistance > epsilon)
                {
                    stack.Push(new KeyValuePair<int, int>(startIndex, maxIndex));
                    stack.Push(new KeyValuePair<int, int>(maxIndex, endIndex));
                }
                else
                {
                    for (var i = startIndex + 1; i < endIndex; i++)
                        result[i] = false;
                }
            }

            return result;
        }

        private float PointLineDistance(float[] xs, float[] ys, int pointIndex, int lineStartIndex, int lineEndIndex)
        {
            var px = xs[pointIndex];     var py = ys[pointIndex];
            var sx = xs[lineStartIndex]; var sy = ys[lineStartIndex];
            var ex = xs[lineEndIndex];   var ey = ys[lineEndIndex];
            var desx = ex - sx; var desy = ey - sy;
            return Mathf.Abs(desx * (sy - py) - desy * (sx - px)) / Mathf.Sqrt(desx * desx + desy * desy);
        }

        public void Dispose() => Dispose(true);
        protected virtual void Dispose(bool disposing) => StopRecording();
    }
}
