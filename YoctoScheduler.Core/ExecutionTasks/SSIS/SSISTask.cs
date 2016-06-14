using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Threading;

namespace YoctoScheduler.Core.ExecutionTasks.SSIS
{
    class SSISTask : JsonBasedTask<Configuration>
    {
        public override string Do()
        {
            string DTExecPath;

            // Retrieve DTExec path based on specified version and architecture
            try
            {
                RegistryKey baseKey;

                if (Configuration.Use32Bit)
                {
                    baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                }
                else
                {
                    baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                }

                RegistryKey subKey = baseKey.OpenSubKey(
                    String.Format(
                        @"SOFTWARE\Microsoft\Microsoft SQL Server\{0}\SSIS\Setup\DTSPath",
                        Configuration.SQLVersion
                        ),
                    writable: false
                    );

                DTExecPath = Path.Combine((string)subKey.GetValue(null), @"Binn\DTExec.exe");

                if (!File.Exists(DTExecPath))
                {
                    throw new FileNotFoundException(string.Format("DTExec could not be found at path {0}.", DTExecPath));
                }

                subKey.Close();
                baseKey.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve DTExec path.", ex);
            }

            Process dtExec = null;

            // DTExec execution
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = DTExecPath;
                psi.Arguments = Configuration.Arguments;

                Job job = new Job();

                dtExec = Process.Start(psi);
                job.AssignProcess(dtExec);

                dtExec.WaitForExit();

                if (dtExec.ExitCode != 0)
                {
                    throw new Exception(String.Format("DTExec failed with exit code {0}.", dtExec.ExitCode));
                }

                return "DTExec execution succeeded";
            }
            catch (ThreadAbortException)
            {
                if (dtExec != null)
                {
                    dtExec.Kill();
                }
                return "Process killed.";
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to execute DTExec.", ex);
            }
        }
    }
}
