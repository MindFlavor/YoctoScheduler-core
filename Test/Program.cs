﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoctoScheduler.Core;
using System.Threading;
using NCrontab;

namespace Test
{
    class Program
    {
        private const string LOG4NET_CONFIG = "Test.log4net.xml";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));

        static YoctoScheduler.Core.Server srvInstance;
        static void Main(string[] args)
        {
            #region setup logging        
            log4net.Config.XmlConfigurator.Configure(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(LOG4NET_CONFIG));
            log.InfoFormat("Test program v{0:S} started.", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            #endregion

            srvInstance = YoctoScheduler.Core.Server.New(
                System.Configuration.ConfigurationManager.ConnectionStrings["YoctoScheduler"].ConnectionString,
                "Server di prova " + DateTime.Now.ToString());

            Console.WriteLine("Program running, please input a command!");

            bool fDone = false;
            while (!fDone)
            {
                var cmd = Console.ReadLine();
                var tokens = cmd.Split(' ');
                try
                {
                    switch (tokens[0])
                    {
                        case "new_task":
                            CreateTask();
                            break;
                        case "new_execution":
                            if (tokens.Length < 2)
                            {
                                Console.WriteLine("Syntax error, must specify a valid task id");
                                continue;
                            }
                            //CreateExecution(int.Parse(tokens[1]));
                            break;
                        case "new_schedule":
                            if (tokens.Length < 3)
                            {
                                Console.WriteLine("Syntax error, must specify a valid task id and a valid crontab");
                                continue;
                            }
                            CreateSchedule(int.Parse(tokens[1]), string.Join(" ", tokens.Skip(2)));

                            break;
                        case "quit":
                            fDone = true;
                            break;
                        case "help":
                            Console.WriteLine("Available commands:");
                            Console.WriteLine("\tnew_task");
                            Console.WriteLine("\tnew_execution <task_id>");
                            Console.WriteLine("\tnew_schedule <task_id> <cron expression>");
                            Console.WriteLine("\tquit");
                            Console.WriteLine("\thelp");
                            break;
                        case "":
                            break;
                        default:
                            log.WarnFormat("Syntax error, unknown command \"{0:S}\". Type help for help, quit to quit.", tokens[0]);
                            break;

                    }
                }
                catch (Exception e)
                {
                    log.ErrorFormat("Exception: " + e.ToString());
                }
            }
        }

        static void CreateTask()
        {
            var task = YoctoScheduler.Core.Task.New(System.Configuration.ConfigurationManager.ConnectionStrings["YoctoScheduler"].ConnectionString);
            log.InfoFormat("Created task {0:S}", task.ToString());
        }

        //static void CreateExecution(int TaskId)
        //{
        //    var status = YoctoScheduler.Server.ExecutionStatus.CreateExecutionStatus(TaskId, srvInstance.Id);

        //    log.InfoFormat("Created execution status", status.ToString());
        //}

        static void CreateSchedule(int taskId, string cron)
        {
            Schedule sched = Schedule.New(System.Configuration.ConfigurationManager.ConnectionStrings["YoctoScheduler"].ConnectionString,
                taskId, cron, false);
            log.InfoFormat("Created schedule {0:S}", sched.ToString());
        }
    }
}
