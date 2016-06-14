using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace YoctoScheduler.Core.ExecutionTasks.SSIS
{
    class Job : IDisposable
    {
        private IntPtr jobHandle;

        public Job()
        {
            jobHandle = CreateJobObjectW(IntPtr.Zero, null);
            if (jobHandle == IntPtr.Zero)
            {
                throw new JobCreationError((int)GetLastError());
            }

            JOBOBJECT_BASIC_LIMIT_INFORMATION jbli = new JOBOBJECT_BASIC_LIMIT_INFORMATION();
            jbli.LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;

            JOBOBJECT_EXTENDED_LIMIT_INFORMATION jeli = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION();
            jeli.BasicLimitInformation = jbli;

            int len = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
            IntPtr jeliptr = Marshal.AllocHGlobal(len);
            Marshal.StructureToPtr(jeli, jeliptr, false);

            if (!SetInformationJobObject(
                jobHandle,
                JOBOBJECTINFOCLASS.JobObjectExtendedLimitInformation,
                jeliptr,
                (uint)len
                ))
            {
                throw new SetInformationJobObjectError((int)GetLastError());
            }
        }

        public void AssignProcess(Process process)
        {
            if (!AssignProcessToJobObject(jobHandle, process.Handle))
            {
                Console.WriteLine(GetLastError());
            }
        }

        public void Dispose()
        {
            if (!CloseHandle(jobHandle))
            {
                throw new JobHandleCloseError((int)GetLastError());
            }
        }

        #region Windows constants

        /// JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE -> 0x00002000
        public const int JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 8192;

        /// JOB_OBJECT_ASSIGN_PROCESS -> (0x0001)
        public const int JOB_OBJECT_ASSIGN_PROCESS = 1;

        #endregion

        #region Windows functions

        /// Return Type: HANDLE->void*
        ///lpJobAttributes: LPSECURITY_ATTRIBUTES->_SECURITY_ATTRIBUTES*
        ///lpName: LPCWSTR->WCHAR*
        [DllImportAttribute("kernel32.dll", EntryPoint = "CreateJobObjectW")]
        public static extern IntPtr CreateJobObjectW(
            [InAttribute()] IntPtr lpJobAttributes,
            [InAttribute()] [MarshalAsAttribute(UnmanagedType.LPWStr)] string lpName);

        /// Return Type: BOOL->int
        ///hObject: HANDLE->void*
        [DllImportAttribute("kernel32.dll", EntryPoint = "CloseHandle")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        public static extern bool CloseHandle([InAttribute()] IntPtr hObject);

        /// Return Type: DWORD->unsigned int
        [DllImportAttribute("kernel32.dll", EntryPoint = "GetLastError")]
        public static extern uint GetLastError();

        /// Return Type: BOOL->int
        ///hJob: HANDLE->void*
        ///hProcess: HANDLE->void*
        [DllImportAttribute("kernel32.dll", EntryPoint = "AssignProcessToJobObject")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        public static extern bool AssignProcessToJobObject([InAttribute()] IntPtr hJob, [InAttribute()] IntPtr hProcess);

        /// Return Type: BOOL->int
        ///hJob: HANDLE->void*
        ///JobObjectInformationClass: JOBOBJECTINFOCLASS->_JOBOBJECTINFOCLASS
        ///lpJobObjectInformation: LPVOID->void*
        ///cbJobObjectInformationLength: DWORD->unsigned int
        [DllImportAttribute("kernel32.dll", EntryPoint = "SetInformationJobObject")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        public static extern bool SetInformationJobObject(
            [InAttribute()] IntPtr hJob,
            JOBOBJECTINFOCLASS JobObjectInformationClass,
            [InAttribute()] IntPtr lpJobObjectInformation,
            uint cbJobObjectInformationLength
            );

        #endregion
    }

    #region Exceptions

    public class JobCreationError : Win32Exception
    {
        public JobCreationError(int error) : base(error) { }
    }

    public class JobHandleCloseError : Win32Exception
    {
        public JobHandleCloseError(int error) : base(error) { }
    }

    public class SetInformationJobObjectError : Win32Exception
    {
        public SetInformationJobObjectError(int error) : base(error) { }
    }


    #endregion

    #region Windows structures

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
    {

        /// JOBOBJECT_BASIC_LIMIT_INFORMATION->_JOBOBJECT_BASIC_LIMIT_INFORMATION
        public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;

        /// IO_COUNTERS->_IO_COUNTERS
        public IO_COUNTERS IoInfo;

        /// SIZE_T->ULONG_PTR->unsigned int
        public uint ProcessMemoryLimit;

        /// SIZE_T->ULONG_PTR->unsigned int
        public uint JobMemoryLimit;

        /// SIZE_T->ULONG_PTR->unsigned int
        public uint PeakProcessMemoryUsed;

        /// SIZE_T->ULONG_PTR->unsigned int
        public uint PeakJobMemoryUsed;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct JOBOBJECT_BASIC_LIMIT_INFORMATION
    {

        /// LARGE_INTEGER->_LARGE_INTEGER
        public LARGE_INTEGER PerProcessUserTimeLimit;

        /// LARGE_INTEGER->_LARGE_INTEGER
        public LARGE_INTEGER PerJobUserTimeLimit;

        /// DWORD->unsigned int
        public uint LimitFlags;

        /// SIZE_T->ULONG_PTR->unsigned int
        public uint MinimumWorkingSetSize;

        /// SIZE_T->ULONG_PTR->unsigned int
        public uint MaximumWorkingSetSize;

        /// DWORD->unsigned int
        public uint ActiveProcessLimit;

        /// ULONG_PTR->unsigned int
        public uint Affinity;

        /// DWORD->unsigned int
        public uint PriorityClass;

        /// DWORD->unsigned int
        public uint SchedulingClass;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct IO_COUNTERS
    {

        /// ULONGLONG->unsigned __int64
        public ulong ReadOperationCount;

        /// ULONGLONG->unsigned __int64
        public ulong WriteOperationCount;

        /// ULONGLONG->unsigned __int64
        public ulong OtherOperationCount;

        /// ULONGLONG->unsigned __int64
        public ulong ReadTransferCount;

        /// ULONGLONG->unsigned __int64
        public ulong WriteTransferCount;

        /// ULONGLONG->unsigned __int64
        public ulong OtherTransferCount;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Explicit)]
    public struct LARGE_INTEGER
    {

        /// Anonymous_9320654f_2227_43bf_a385_74cc8c562686
        [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
        public Anonymous_9320654f_2227_43bf_a385_74cc8c562686 Struct1;

        /// Anonymous_947eb392_1446_4e25_bbd4_10e98165f3a9
        [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
        public Anonymous_947eb392_1446_4e25_bbd4_10e98165f3a9 u;

        /// LONGLONG->__int64
        [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
        public long QuadPart;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct Anonymous_9320654f_2227_43bf_a385_74cc8c562686
    {

        /// DWORD->unsigned int
        public uint LowPart;

        /// LONG->int
        public int HighPart;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct Anonymous_947eb392_1446_4e25_bbd4_10e98165f3a9
    {

        /// DWORD->unsigned int
        public uint LowPart;

        /// LONG->int
        public int HighPart;
    }

    public enum JOBOBJECTINFOCLASS
    {
        /// JobObjectBasicAccountingInformation -> 1
        JobObjectBasicAccountingInformation = 1,
        JobObjectBasicLimitInformation,
        JobObjectBasicProcessIdList,
        JobObjectBasicUIRestrictions,
        JobObjectSecurityLimitInformation,
        JobObjectEndOfJobTimeInformation,
        JobObjectAssociateCompletionPortInformation,
        JobObjectBasicAndIoAccountingInformation,
        JobObjectExtendedLimitInformation,
        JobObjectJobSetInformation,
        MaxJobObjectInfoClass,
    }

    #endregion
}
