using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ReadMemory
{
    class Memory
    {
        [DllImport("kernel32.dll")]
        private static unsafe extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
                                                                byte[] lpBuffer, UIntPtr nSize, uint lpNumberOfBytesWritten);

        //import kernel32 and create OpenProcess and ReadProcess functions
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Boolean bInheritHandle, UInt32 dwProcessId);
        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
        byte[] lpBuffer, UIntPtr nSize, uint lpNumberOfBytesWritten);

        //Create handle
        IntPtr Handle;

        //constructor
        public Memory(string sprocess, uint access)
        {
            //Get the specific process
            Process[] Processes = Process.GetProcessesByName(sprocess);
            Process nProcess = Processes[0];
            //access to the process
            //0x10 - read
            //0x20 - write
            //0x001F0FFF - all
            Handle = OpenProcess(access, false, (uint)nProcess.Id);
        }

        //function ReadString (returns string value)
        public string ReadString(uint pointer)
        {
            byte[] bytes = new byte[24];

            //Read the specific address within the process
            ReadProcessMemory(Handle, (IntPtr)pointer, bytes, (UIntPtr)24, 0);
            //Return the result as UTF8 String
            return Encoding.UTF8.GetString(bytes);
        }

        //function ReadOffset (returns int value)
        public int ReadOffset(uint pointer, uint offset)
        {
            byte[] bytes = new byte[24];

            //Creating the address (reading the Base and add the offset)
            uint adress = (uint)ReadPointer(pointer) + offset;
            //Reading the specific address within the process
            ReadProcessMemory(Handle, (IntPtr)adress, bytes, (UIntPtr)sizeof(int), 0);
            //Return the result as 4 byte int
            return BitConverter.ToInt32(bytes, 0);
        }

        public float ReadFloat(uint pointer)
        {
            byte[] bytes = new byte[24];

            //Reading the specific address within the process
            ReadProcessMemory(Handle, (IntPtr)pointer, bytes, (UIntPtr)sizeof(int), 0);
            //Return the result as 4 byte int
            return BitConverter.ToSingle(bytes, 0);
        }

        public float ReadFloatOffset(uint pointer, uint offset)
        {
            byte[] bytes = new byte[24];

            //Creating the address (reading the Base and add the offset)
            uint adress = (uint)ReadPointer(pointer) + offset;
            //Reading the specific address within the process
            ReadProcessMemory(Handle, (IntPtr)adress, bytes, (UIntPtr)sizeof(int), 0);
            //Return the result as 4 byte int
            return BitConverter.ToSingle(bytes, 0);
        }

        //function ReadPointer (returns int value)
        public int ReadPointer(uint pointer)
        {
            byte[] bytes = new byte[24];

            //Reading the specific address within the process
            ReadProcessMemory(Handle, (IntPtr)pointer, bytes, (UIntPtr)sizeof(int), 0);
            //Return the result as 4 byte int
            return BitConverter.ToInt32(bytes, 0);
        }
        public void Write(uint pointer, byte[] Buffer)
        {
            WriteProcessMemory(Handle, (IntPtr)pointer, Buffer, (UIntPtr)Buffer.Length, 0);
        }

        public void WritePointer(uint pointer, uint offset, byte[] Buffer)
        {
            uint adress = (uint)ReadPointer(pointer) + offset;
            WriteProcessMemory(Handle, (IntPtr)adress, Buffer, (UIntPtr)Buffer.Length, 0);
        }
    }
}