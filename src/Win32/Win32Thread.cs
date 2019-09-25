namespace DataBreakpoints.Win32 {
    using System;
    using System.Runtime.InteropServices;
    using static PInvoke.Kernel32;

    class Win32Thread {
        [DllImport("kernel32.dll")]
        public static extern SafeObjectHandle GetCurrentThread();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeObjectHandle OpenThread(ThreadAccess access, bool inheritHandle, int threadID);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeObjectHandle CreateThread(
            IntPtr attributes, UIntPtr stackSize, IntPtr startAddress, IntPtr parameter,
            int creationFlags, out int threadID);

        public delegate int ThreadProc(IntPtr parameter);
    }
}
