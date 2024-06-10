using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class TaskSchedulerService : BackgroundService
{
    private readonly SortedSet<ScheduledTask> _taskQueue = new(new ScheduledTaskComparer());
    private readonly SemaphoreSlim _signal = new(0);

    public void ScheduleTask(DateTime executeAt, Func<Task> task)
    {
        lock (_taskQueue)
        {
            _taskQueue.Add(new ScheduledTask
            {
                ExecuteAt = executeAt,
                FunctionToExecute = task
            });
            _signal.Release();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            ScheduledTask nextTask = null;

            lock (_taskQueue)
            {
                if (_taskQueue.Count > 0 && _taskQueue.Min.ExecuteAt <= DateTime.Now.ToLocalTime())
                {
                    nextTask = _taskQueue.Min;
                    _taskQueue.Remove(nextTask);
                }
            }

            if (nextTask != null)
            {
                await nextTask.FunctionToExecute();
            }
            else
            {
                await _signal.WaitAsync(stoppingToken);
            }
        }
    }
}
