namespace DataBreakpoints
{
    using System;
    using System.Runtime.InteropServices;

    static class Win32DataBreakpoints {
        static unsafe void Main() {
            long* address = (long*)Marshal.AllocHGlobal(8);
            *address = 0x0EADBEEF_BEEFDEAD;
            using var breakpoint = DataBreakpoint.Set(
                address: (IntPtr)address, size: new UIntPtr(8),
                DataBreakpointTrigger.OnWrite);

            if (breakpoint == null) throw new InvalidOperationException();

            *address = 0x42424242_42424242;
        }
    }
}