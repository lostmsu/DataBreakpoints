namespace DataBreakpoints {
    using System;
    using System.Runtime.InteropServices;
    using DataBreakpoints.Win32;

    public static class DataBreakpoint
    {
        public static IDisposable Set(int threadID, IntPtr address, UIntPtr size, DataBreakpointTrigger trigger) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new Win32DataBreakpointFactory().Set(threadID, address, size, trigger);
            throw new PlatformNotSupportedException("Only Windows 64 bit is supported");
        }
    }
}
