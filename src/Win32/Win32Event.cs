namespace DataBreakpoints.Win32 {
    using System;
    using System.Runtime.InteropServices;
    using static PInvoke.Kernel32;

    static class Win32Event
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetEvent(SafeObjectHandle handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeObjectHandle CreateEvent(
            IntPtr attributes, bool manualReset, bool initialState, string name);
    }
}
