namespace DataBreakpoints {
    using System;

    public interface IDataBreakpointFactory
    {
        IDisposable Set(int threadID, IntPtr address, UIntPtr size, DataBreakpointTrigger trigger);
    }
}
