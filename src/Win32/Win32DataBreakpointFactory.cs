namespace DataBreakpoints.Win32 {
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;

    class Win32DataBreakpointFactory: IDataBreakpointFactory {
        public IDisposable Set(int threadID, IntPtr address, UIntPtr size, DataBreakpointTrigger trigger) {
            if (address == IntPtr.Zero)
                throw new ArgumentNullException(nameof(address));
            if (size == UIntPtr.Zero)
                throw new ArgumentException("Region size must be non-zero", paramName: nameof(size));
            var type = trigger switch {
                DataBreakpointTrigger.OnWrite => HardwareBreakpointType.Write,
                DataBreakpointTrigger.OnReadOrWrite => HardwareBreakpointType.ReadWrite,
                _ => throw new ArgumentException("Invalid value", paramName: nameof(trigger))
            };
            var sizeType = checked((ulong)size) switch {
                1 => HardwareBreakpointSize.Size1,
                2 => HardwareBreakpointSize.Size2,
                4 => HardwareBreakpointSize.Size4,
                8 => HardwareBreakpointSize.Size8,
                _ => throw new NotSupportedException(
                    "Memory region size must be 1, 2, 4, or 8 bytes"),
            };
            var threadHandle = Win32Thread.OpenThread(ThreadAccess.All, inheritHandle: false, threadID);
            if (threadHandle.IsInvalid)
                throw new Win32Exception();
            var breakpoint = Win32HardwareBreakpoint.TrySet(threadHandle, type, sizeType, address);
            if (breakpoint == null)
                throw new InvalidOperationException("Data breakpoint limit reached");
            return breakpoint;
        }

        public IDisposable Set(IntPtr address, UIntPtr size, DataBreakpointTrigger trigger) {
            var breakpoints = new DisposableCollection();
            var threads = Process.GetCurrentProcess().Threads
                .Cast<ProcessThread>().ToArray();
            foreach (var thread in threads) {
                breakpoints.Add(this.Set(thread.Id, address, size, trigger));
            }

            return breakpoints;
        }
    }
}
