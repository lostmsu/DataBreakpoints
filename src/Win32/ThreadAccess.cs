namespace DataBreakpoints.Win32 {
    using System;

    [Flags]
    enum ThreadAccess {
        StandardRightsRequired = 0x000F0000,
        Synchronize = 0x00100000,
        All = StandardRightsRequired | Synchronize | 0xFFFF,
    }
}
