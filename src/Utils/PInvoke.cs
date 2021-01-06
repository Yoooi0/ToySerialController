using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace ToySerialController.Utils
{
    public static class PInvoke
    {
        [DllImport("dbgHelp", SetLastError = true)]
        public static extern bool MakeSureDirectoryPathExists(string dirPath);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern SafeFileHandle CreateFile(string fileName, uint fileAccess, uint fileShare,
            IntPtr securityAttributes, uint creationDisposition, uint flags, IntPtr template);
        
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteFile(IntPtr hFile, byte[] lpBuffer,uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);
    }
}
