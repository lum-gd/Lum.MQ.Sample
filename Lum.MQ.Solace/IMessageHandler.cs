﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Lum.MQ.Solace
{
    public interface IMessageHandler : IMessageHandlerBase
    {
        IMessageBox MessageBox { get; set; }
        Task<bool> HandleMessage(IReceivedMessageDto dto);
        List<string> Whos { get; }
        int ProcessingCount { get; }
        void InitConcurrency(DataflowBlockOptions dataflowBlockOptions);
    }

    public interface IMessageHandler<T> : IMessageHandler
    {
        void AddHandler(Action<T> action, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions);
        void AddHandler(Func<T, Task> action, string who, ExecutionDataflowBlockOptions executionDataflowBlockOptions);
    }
}
