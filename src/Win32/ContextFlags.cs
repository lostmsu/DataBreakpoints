namespace DataBreakpoints.Win32 {
    using System;
    using DWORD = System.UInt32;

    [Flags]
    enum ContextFlags: DWORD {
        Amd64 = 0x00100000,
        DebugRegisters = Amd64 | 0x00000010,
    }
}
