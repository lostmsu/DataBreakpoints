namespace DataBreakpoints.Win32 {
    using DWORD64 = System.UInt64;
    using DWORD = System.UInt32;
    using WORD = System.UInt16;
    using BYTE = System.Byte;
    using M128A_Half = System.UInt64;
    unsafe struct XmmSaveArea {
        WORD ControlWord;
        WORD StatusWord;
        BYTE TagWord;
        BYTE Reserved1;
        WORD ErrorOpcode;
        DWORD ErrorOffset;
        WORD ErrorSelector;
        WORD Reserved2;
        DWORD DataOffset;
        WORD DataSelector;
        WORD Reserved3;
        DWORD MxCsr;
        DWORD MxCsr_Mask;
        fixed M128A_Half FloatRegisters[8*2];

        fixed M128A_Half XmmRegisters[16*2];
        fixed BYTE  Reserved4[96];

#warning 32 bit support
        //fixed M128A_Half XmmRegisters[8*2];
        //fixed BYTE Reserved4[224];
    }
}
