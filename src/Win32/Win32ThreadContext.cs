namespace DataBreakpoints.Win32 {
    using System;
    using System.Runtime.InteropServices;
    using static PInvoke.Kernel32;
    using DWORD64 = System.UInt64;
    using DWORD = System.UInt32;
    using WORD = System.UInt16;
    using M128A_Half = System.UInt64;
    using Win32Exception = System.ComponentModel.Win32Exception;

    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    unsafe struct Win32ThreadContext
    {
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        static extern bool GetThreadContext(SafeObjectHandle threadHandle, IntPtr context);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetThreadContext(SafeObjectHandle threadHandle, IntPtr context);

        public void Get(SafeObjectHandle threadHandle) {
            var raw = Marshal.AllocHGlobal(Marshal.SizeOf<Win32ThreadContext>() + 8);
            try {
                var aligned = new IntPtr(16 * (((long)raw + 15) / 16));
                Marshal.StructureToPtr(this, aligned, fDeleteOld: true);

                if (!GetThreadContext(threadHandle, aligned))
                    throw new Win32Exception();

                this = Marshal.PtrToStructure<Win32ThreadContext>(aligned);
            } finally {
                Marshal.FreeHGlobal(raw);
            }
        }

        public void Set(SafeObjectHandle threadHandle) {
            var raw = Marshal.AllocHGlobal(Marshal.SizeOf<Win32ThreadContext>() + 8);
            try {
                var aligned = new IntPtr(16 * (((long)raw + 15) / 16));
                Marshal.StructureToPtr(this, aligned, fDeleteOld: true);

                if (!SetThreadContext(threadHandle, aligned))
                    throw new Win32Exception();

                this = Marshal.PtrToStructure<Win32ThreadContext>(aligned);
            } finally {
                Marshal.FreeHGlobal(raw);
            }
        }

        //
        // Register parameter home addresses.
        //
        // N.B. These fields are for convience - they could be used to extend the
        //      context record in the future.
        //

        DWORD64 P1Home;
        DWORD64 P2Home;
        DWORD64 P3Home;
        DWORD64 P4Home;
        DWORD64 P5Home;
        DWORD64 P6Home;

        //
        // Control flags.
        //

        public ContextFlags ContextFlags;
        DWORD MxCsr;

        //
        // Segment Registers and processor flags.
        //

        WORD SegCs;
        WORD SegDs;
        WORD SegEs;
        WORD SegFs;
        WORD SegGs;
        WORD SegSs;
        DWORD EFlags;

        //
        // Debug registers
        //

        public IntPtr Dr0;
        public IntPtr Dr1;
        public IntPtr Dr2;
        public IntPtr Dr3;
        public IntPtr Dr6;
        public IntPtr Dr7;

        //
        // Integer registers.
        //

        DWORD64 Rax;
        DWORD64 Rcx;
        DWORD64 Rdx;
        DWORD64 Rbx;
        DWORD64 Rsp;
        DWORD64 Rbp;
        DWORD64 Rsi;
        DWORD64 Rdi;
        DWORD64 R8;
        DWORD64 R9;
        DWORD64 R10;
        DWORD64 R11;
        DWORD64 R12;
        DWORD64 R13;
        DWORD64 R14;
        DWORD64 R15;

        //
        // Program counter.
        //

        DWORD64 Rip;

        //
        // Floating point state.
        //

        XmmSaveAreaOrLegacyStruct xmmContext;

    //
    // Vector registers.
    //

    fixed M128A_Half VectorRegister[26*2];
DWORD64 VectorControl;

//
// Special debug control registers.
//

DWORD64 DebugControl;
DWORD64 LastBranchToRip;
DWORD64 LastBranchFromRip;
DWORD64 LastExceptionToRip;
DWORD64 LastExceptionFromRip;
    }
}
