﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Core.Tasks
{
    public interface ITask
    {
        void Start();
        void Stop();

        bool IsAlive();
    }
}