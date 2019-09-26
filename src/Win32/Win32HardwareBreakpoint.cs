namespace DataBreakpoints.Win32 {
    using System;
    using System.Runtime.InteropServices;
    using PInvoke;
    using static PInvoke.Kernel32;
    using Win32Exception = System.ComponentModel.Win32Exception;

    class Win32HardwareBreakpoint: IDisposable {
        IntPtr Address;
        SafeObjectHandle ThreadHandle;
        HardwareBreakpointType Type;
        HardwareBreakpointSize Size;
        SafeObjectHandle CompletionEvent;
        int iReg;
        Operation Op;
        bool SUCC;
        GCHandle gcHandle;

        enum Operation
        {
            Set,
            Remove,
        }

        static void SetBits(ref IntPtr dw, int lowBit, int bits, int newValue) {
            if (IntPtr.Size == 4) {
                uint mask = (1u << bits) - 1u;
                uint value = unchecked((uint)dw);
                value = (value & ~(mask << lowBit)) | (unchecked((uint)newValue) << lowBit);
                dw = unchecked((IntPtr)value);
            } else if (IntPtr.Size == 8) {
                ulong mask = (1ul << bits) - 1ul;
                ulong value = unchecked((ulong)dw);
                value = (value & ~(mask << lowBit)) | (unchecked((ulong)newValue) << lowBit);
                dw = unchecked((IntPtr)value);
            } else {
                throw new PlatformNotSupportedException();
            }
        }

        static int th(IntPtr lpParameter) {
            if (lpParameter == IntPtr.Zero) throw new ArgumentNullException(nameof(lpParameter));

            var breakpoint = (Win32HardwareBreakpoint)GCHandle.FromIntPtr(lpParameter).Target;

            int threadOpResult = SuspendThread(breakpoint.ThreadHandle);
            GetLastError().ThrowOnError();

            var ct = new Win32ThreadContext {
                ContextFlags = ContextFlags.DebugRegisters,
            };

            Win32ThreadContext.Get(breakpoint.ThreadHandle, ref ct);
            GetLastError().ThrowOnError();

            int FlagBit = 0;

            bool Dr0Busy = false;
            bool Dr1Busy = false;
            bool Dr2Busy = false;
            bool Dr3Busy = false;
            if (((ulong)ct.Dr7 & 1) != 0)
                Dr0Busy = true;
            if (((ulong)ct.Dr7 & 4) != 0)
                Dr1Busy = true;
            if (((ulong)ct.Dr7 & 16) != 0)
                Dr2Busy = true;
            if (((ulong)ct.Dr7 & 64) != 0)
                Dr3Busy = true;

            if (breakpoint.Op == Operation.Remove) {
                // Remove
                if (breakpoint.iReg == 0) {
                    FlagBit = 0;
                    ct.Dr0 = IntPtr.Zero;
                    Dr0Busy = false;
                }
                if (breakpoint.iReg == 1) {
                    FlagBit = 2;
                    ct.Dr1 = IntPtr.Zero; ;
                    Dr1Busy = false;
                }
                if (breakpoint.iReg == 2) {
                    FlagBit = 4;
                    ct.Dr2 = IntPtr.Zero; ;
                    Dr2Busy = false;
                }
                if (breakpoint.iReg == 3) {
                    FlagBit = 6;
                    ct.Dr3 = IntPtr.Zero; ;
                    Dr3Busy = false;
                }

                if (IntPtr.Size == 8) {
                    ulong v = (ulong)ct.Dr7;
                    v &= ~(1ul << FlagBit);
                    ct.Dr7 = (IntPtr)v;
                }
            } else {
                if (!Dr0Busy) {
                    breakpoint.iReg = 0;
                    ct.Dr0 = breakpoint.Address;
                    Dr0Busy = true;
                } else
                if (!Dr1Busy) {
                    breakpoint.iReg = 1;
                    ct.Dr1 = breakpoint.Address;
                    Dr1Busy = true;
                } else
                if (!Dr2Busy) {
                    breakpoint.iReg = 2;
                    ct.Dr2 = breakpoint.Address;
                    Dr2Busy = true;
                } else
                if (!Dr3Busy) {
                    breakpoint.iReg = 3;
                    ct.Dr3 = breakpoint.Address;
                    Dr3Busy = true;
                } else {
                    breakpoint.SUCC = false;
                    threadOpResult = ResumeThread(breakpoint.ThreadHandle);
                    GetLastError().ThrowOnError();
                    Win32Event.SetEvent(breakpoint.CompletionEvent);
                    return 0;
                }
                ct.Dr6 = IntPtr.Zero;
                int st = 0;
                if (breakpoint.Type == HardwareBreakpointType.Code)
                    st = 0;
                if (breakpoint.Type == HardwareBreakpointType.ReadWrite)
                    st = 3;
                if (breakpoint.Type == HardwareBreakpointType.Write)
                    st = 1;
                int le = 0;
                if (breakpoint.Size == HardwareBreakpointSize.Size1)
                    le = 0;
                if (breakpoint.Size == HardwareBreakpointSize.Size2)
                    le = 1;
                if (breakpoint.Size == HardwareBreakpointSize.Size4)
                    le = 3;
                if (breakpoint.Size == HardwareBreakpointSize.Size8)
                    le = 2;

                SetBits(ref ct.Dr7, 16 + breakpoint.iReg * 4, 2, st);
                SetBits(ref ct.Dr7, 18 + breakpoint.iReg * 4, 2, le);
                SetBits(ref ct.Dr7, breakpoint.iReg * 2, 1, 1);
            }

            ct.ContextFlags = ContextFlags.DebugRegisters;
            ct.Set(breakpoint.ThreadHandle);

            ct = new Win32ThreadContext {ContextFlags = ContextFlags.DebugRegisters};
            Win32ThreadContext.Get(breakpoint.ThreadHandle, ref ct);

            threadOpResult = ResumeThread(breakpoint.ThreadHandle);
            GetLastError().ThrowOnError();

            breakpoint.SUCC = true;

            Win32Event.SetEvent(breakpoint.CompletionEvent);
            return 0;
        }

        public static Win32HardwareBreakpoint TrySet(SafeObjectHandle threadHandle,
            HardwareBreakpointType type, HardwareBreakpointSize size, IntPtr address) {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || IntPtr.Size != 8)
                throw new PlatformNotSupportedException("Only 64 bit Windows is supported");

            var breakpoint = new Win32HardwareBreakpoint {
                Address = address,
                Size = size,
                Type = type,
                ThreadHandle = threadHandle
            };

            bool self = threadHandle.DangerousGetHandle() == Win32Thread.GetCurrentThread().DangerousGetHandle();
            if (self) {
                int threadID = GetCurrentThreadId();
                breakpoint.ThreadHandle = Win32Thread.OpenThread(ThreadAccess.All, inheritHandle: false, threadID: threadID);
                if (breakpoint.ThreadHandle.IsInvalid)
                    throw new Win32Exception();
            }

            breakpoint.CompletionEvent = Win32Event.CreateEvent(IntPtr.Zero, manualReset: false, initialState: false, name: null);
            breakpoint.Op = Operation.Set;

            Win32Thread.ThreadProc threadProc = Win32HardwareBreakpoint.th;
            var th = Marshal.GetFunctionPointerForDelegate(threadProc);
            var h = GCHandle.Alloc(breakpoint);
            if (Win32Thread.CreateThread(IntPtr.Zero, UIntPtr.Zero, th,
                parameter: (IntPtr)h, creationFlags: 0, out int _).IsInvalid)
                throw new Win32Exception();

            WaitForSingleObject(breakpoint.CompletionEvent, dwMilliseconds: Constants.INFINITE);
            breakpoint.CompletionEvent.Close();

            if (self) {
                breakpoint.ThreadHandle.Close();
            }

            breakpoint.ThreadHandle = threadHandle;
            if (!breakpoint.SUCC) {
                h.Free();
                return null;
            }

            breakpoint.gcHandle = h;

            GC.KeepAlive(threadProc);
            return breakpoint;
        }

        static void Remove(GCHandle handle) {
            if (!handle.IsAllocated) throw new ArgumentNullException(nameof(handle));

            var breakpoint = (Win32HardwareBreakpoint)handle.Target;
            bool isSelf = false;
            if (breakpoint.ThreadHandle.DangerousGetHandle() == Win32Thread.GetCurrentThread().DangerousGetHandle()) {
                int threadID = GetCurrentThreadId();
                breakpoint.ThreadHandle = Win32Thread.OpenThread(ThreadAccess.All, inheritHandle: false, threadID);
                if (breakpoint.ThreadHandle.IsInvalid)
                    throw new Win32Exception();
                isSelf = true;
            }

            breakpoint.CompletionEvent = Win32Event.CreateEvent(IntPtr.Zero, manualReset: false, initialState: false, name: null);
            breakpoint.Op = Operation.Remove;

            Win32Thread.ThreadProc threadProc = Win32HardwareBreakpoint.th;
            var th = Marshal.GetFunctionPointerForDelegate(threadProc);
            if (Win32Thread.CreateThread(IntPtr.Zero, UIntPtr.Zero, th, (IntPtr)handle, 0, out int _).IsInvalid)
                throw new Win32Exception();
            WaitForSingleObject(breakpoint.CompletionEvent, Constants.INFINITE);
            breakpoint.CompletionEvent.Close();
            if (isSelf) {
                breakpoint.ThreadHandle.Close();
            }

            handle.Free();
        }

        public void Dispose() {
            if (!this.SUCC)
                return;

            Remove(this.gcHandle);
            this.SUCC = false;
        }
    }
}
