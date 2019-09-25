namespace DataBreakpoints
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using DataBreakpoints.Win32;

    static class Win32DataBreakpoints {
        static unsafe void Main() {
            long* address = (long*)Marshal.AllocHGlobal(8);
            *address = 0x0EADBEEF_BEEFDEAD;
            var breakpoint = Win32HardwareBreakpoint.Set(
                Win32Thread.GetCurrentThread(), HardwareBreakpointType.Write,
                HardwareBreakpointSize.Size8, (IntPtr)address);

            if (breakpoint == null) throw new InvalidOperationException();

            *address = 0x42424242_42424242;

            Win32HardwareBreakpoint.Remove(breakpoint.Value);
        }
    }
}