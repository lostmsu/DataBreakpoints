namespace DataBreakpoints.Win32 {
    using M128A_Half = System.UInt64;
    unsafe struct XmmLegacyStruct {
        fixed M128A_Half Header[2*2];
        fixed M128A_Half Legacy[8*2];
        M128A Xmm0;
        M128A Xmm1;
        M128A Xmm2;
        M128A Xmm3;
        M128A Xmm4;
        M128A Xmm5;
        M128A Xmm6;
        M128A Xmm7;
        M128A Xmm8;
        M128A Xmm9;
        M128A Xmm10;
        M128A Xmm11;
        M128A Xmm12;
        M128A Xmm13;
        M128A Xmm14;
        M128A Xmm15;
    }
}
