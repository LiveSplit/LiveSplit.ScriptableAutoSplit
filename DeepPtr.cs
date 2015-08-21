using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using LiveSplit.ComponentUtil;

namespace LiveSplit
{
    public class DeepPointer
    {
        private List<long> _offsets;
        private long _base;
        private string _module;

        public DeepPointer(string module, long base_, params long[] offsets)
        {
            _module = module.ToLower();
            _base = base_;
            _offsets = new List<long>();
            _offsets.Add(0); // deref base first
            _offsets.AddRange(offsets);
        }

        public DeepPointer(long base_, params long[] offsets)
        {
            _base = base_;
            _offsets = new List<long>();
            _offsets.Add(0); // deref base first
            _offsets.AddRange(offsets);
        }

        public bool Deref<T>(Process process, out T value) where T : struct
        {
            long offset = _offsets[_offsets.Count - 1];
            IntPtr ptr;
            if (!this.DerefOffsets(process, out ptr)
                || !ReadProcessValue(process, new IntPtr(ptr.ToInt64() + offset), out value))
            {
                value = default(T);
                return false;
            }

            return true;
        }

        public bool Deref(Process process, Type type, out object value)
        {
            long offset = _offsets[_offsets.Count - 1];
            IntPtr ptr;
            if (!this.DerefOffsets(process, out ptr)
                || !ReadProcessValue(process, new IntPtr(ptr.ToInt64() + offset), type, out value))
            {
                value = default(object);
                return false;
            }

            return true;
        }

        public bool Deref(Process process, out byte[] value, int elementCount)
        {
            long offset = _offsets[_offsets.Count - 1];
            IntPtr ptr;
            if (!this.DerefOffsets(process, out ptr)
                || !ReadProcessBytes(process, new IntPtr(ptr.ToInt64() + offset), elementCount, out value))
            {
                value = null;
                return false;
            }

            return true;
        }

        public bool Deref(Process process, out Vector3f value)
        {
            long offset = _offsets[_offsets.Count - 1];
            IntPtr ptr;
            float x, y, z;
            if (!this.DerefOffsets(process, out ptr)
                || !ReadProcessValue(process, new IntPtr(ptr.ToInt64() + offset + 0), out x)
                || !ReadProcessValue(process, new IntPtr(ptr.ToInt64() + offset + 4), out y)
                || !ReadProcessValue(process, new IntPtr(ptr.ToInt64() + offset + 8), out z))
            {
                value = new Vector3f();
                return false;
            }

            value = new Vector3f(x, y, z);
            return true;
        }

        public bool Deref(Process process, out string str, int max)
        {
            var sb = new StringBuilder(max);
            long offset = _offsets[_offsets.Count - 1];
            IntPtr ptr;
            if (!this.DerefOffsets(process, out ptr)
                || !ReadProcessString(process, new IntPtr(ptr.ToInt64() + offset), sb))
            {
                str = String.Empty;
                return false;
            }

            str = sb.ToString();
            return true;
        }

        bool DerefOffsets(Process process,  out IntPtr ptr)
        {
            bool is64Bit = process.Is64Bit();

            if (!String.IsNullOrEmpty(_module))
            {
                ProcessModuleWow64Safe module = process.ModulesWow64Safe()
                    .FirstOrDefault(m => m.ModuleName.ToLower() == _module);
                if (module == null)
                {
                    ptr = IntPtr.Zero;
                    return false;
                }

                ptr = new IntPtr(module.BaseAddress.ToInt64() + _base);
            }
            else
            {
                ptr = new IntPtr(process.MainModuleWow64Safe().BaseAddress.ToInt64() + _base);
            }


            for (int i = 0; i < _offsets.Count - 1; i++)
            {
                if (!ReadProcessPtr(process, new IntPtr(ptr.ToInt64() + _offsets[i]), is64Bit, out ptr)
                    || ptr == IntPtr.Zero)
                {
                    return false;
                }
            }

            return true;
        }

        static bool ReadProcessValue<T>(Process process, IntPtr addr, out T val) where T : struct
        {
            Type type = typeof(T);

            val = default(T);
            object val2;
            if (!ReadProcessValue(process, addr, type, out val2))
                return false;

            val = (T)val2;

            return true;
        }

        static bool ReadProcessValue(Process process, IntPtr addr, Type type, out object val)
        {
            byte[] bytes;

            val = null;
            int size  = type == typeof(bool) ? 1 : Marshal.SizeOf(type);
            if (!ReadProcessBytes(process, addr, size, out bytes))
                return false;

            val = ResolveToType(bytes, type);

            return true;
        }

        static bool ReadProcessBytes(Process process, IntPtr addr, int elementCount, out byte[] val)
        {
            var bytes = new byte[elementCount];

            int read;
            val = null;
            if (!SafeNativeMethods.ReadProcessMemory(process.Handle, addr, bytes, bytes.Length, out read) || read != bytes.Length)
                return false;

            val = bytes;

            return true;
        }

        static object ResolveToType(byte[] bytes, Type type)
        {
            object val = default(object);

            if (type == typeof(int))
            {
                val = (object)BitConverter.ToInt32(bytes, 0);
            }
            else if (type == typeof(uint))
            {
                val = (object)BitConverter.ToUInt32(bytes, 0);
            }
            else if (type == typeof(float))
            {
                val = (object)BitConverter.ToSingle(bytes, 0);
            }
            else if (type == typeof(double))
            {
                val = (object)BitConverter.ToDouble(bytes, 0);
            }
            else if (type == typeof(byte))
            {
                val = (object)bytes[0];
            }
            else if (type == typeof(bool))
            {
                if (bytes == null)
                    val = false;
                else
                    val = (object)BitConverter.ToBoolean(bytes, 0);
            }
            else if (type == typeof(short))
            {
                val = (object)BitConverter.ToInt16(bytes, 0);
            }
            else
            {  
                var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                try
                {
                    val = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type);
                }
                finally
                {
                    handle.Free();
                }
            }

            return val;
        }

        static bool ReadProcessPtr(Process process, IntPtr addr, bool is64Bit, out IntPtr val)
        {
            byte[] bytes = new byte[is64Bit ? 8 : 4];
            int read;
            val = IntPtr.Zero;
            if (!SafeNativeMethods.ReadProcessMemory(process.Handle, addr, bytes, bytes.Length, out read) || read != bytes.Length)
                return false;
            val = is64Bit ? (IntPtr) BitConverter.ToInt64(bytes, 0) : (IntPtr) BitConverter.ToInt32(bytes, 0);

            return true;
        }

        static bool ReadProcessString(Process process, IntPtr addr, StringBuilder sb)
        {
            byte[] bytes = new byte[sb.Capacity];
            int read;
            if (!SafeNativeMethods.ReadProcessMemory(process.Handle, addr, bytes, bytes.Length, out read) || read != bytes.Length)
                return false;

            if (read >= 2 && bytes[1] == '\x0') // hack to detect utf-16
                sb.Append(Encoding.Unicode.GetString(bytes));
            else
                sb.Append(Encoding.ASCII.GetString(bytes));


            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] == '\0')
                {
                    sb.Remove(i, sb.Length - i);
                    break;
                }
            }

            return true;
        }
    }

    public class Vector3f
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public int IX { get { return (int)this.X; } }
        public int IY { get { return (int)this.Y; } }
        public int IZ { get { return (int)this.Z; } }

        public Vector3f() { }

        public Vector3f(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public float Distance(Vector3f other)
        {
            float result = (this.X - other.X) * (this.X - other.X) +
                (this.Y - other.Y) * (this.Y - other.Y) +
                (this.Z - other.Z) * (this.Z - other.Z);
            return (float)Math.Sqrt(result);
        }

        public float DistanceXY(Vector3f other)
        {
            float result = (this.X - other.X) * (this.X - other.X) +
                (this.Y - other.Y) * (this.Y - other.Y);
            return (float)Math.Sqrt(result);
        }

        public override string ToString()
        {
            return this.X + " " + this.Y + " " + this.Z;
        }
    }

    static class SafeNativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize, // should be IntPtr if we ever need to read a size bigger than 32 bit address space
            out int lpNumberOfBytesRead);
    }
}

public static class Extensions
{
    [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWow64Process([In] IntPtr hProcess,
         [Out, MarshalAs(UnmanagedType.Bool)] out bool wow64Process);

    public static bool Is64Bit(this Process process)
    {
        bool procWow64;
        IsWow64Process(process.Handle, out procWow64);
        if (Environment.Is64BitOperatingSystem && !procWow64)
            return true;
        return false;
    }
}