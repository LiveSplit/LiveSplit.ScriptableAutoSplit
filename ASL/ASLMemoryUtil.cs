using System;
using System.Diagnostics;
using LiveSplit.ComponentUtil;

namespace LiveSplit.ASL
{
    public class MemoryUtilWrapper
    {
        private Process _process;

        public MemoryUtilWrapper(Process process)
        {
            _process = process;
        }

        public T ReadValue<T>(IntPtr addr, T default_ = default(T)) where T : struct
        {
            T val;
            if (!_process.ReadValue(addr, out val))
                val = default_;
            return val;
        }

        public byte[] ReadBytes(IntPtr addr, int count)
        {
            byte[] bytes;
            if (!_process.ReadBytes(addr, count, out bytes))
                return new byte[0];
            return bytes;
        }

        public IntPtr ReadPointer(IntPtr addr, IntPtr default_ = default(IntPtr))
        {
            IntPtr ptr;
            if (!_process.ReadPointer(addr, out ptr))
                return default_;
            return ptr;
        }

        public string ReadString(IntPtr addr, int len, string default_ = "")
        {
            string str;
            if (!_process.ReadString(addr, len, out str))
                return default_;
            return str;
        }
    }
}
