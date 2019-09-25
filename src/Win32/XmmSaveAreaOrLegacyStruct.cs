namespace DataBreakpoints.Win32 {
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack = 16)]
    struct XmmSaveAreaOrLegacyStruct
    {
        [FieldOffset(0)] XmmSaveArea xmmSaveArea;
        [FieldOffset(0)] XmmLegacyStruct xmmLegacyStruct;
    }
}
