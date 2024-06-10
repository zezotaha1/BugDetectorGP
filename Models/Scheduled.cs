using System;

public class ScheduledTask
{
    public DateTime ExecuteAt { get; set; }
    public Func<Task> FunctionToExecute { get; set; }
}

public class ScheduledTaskComparer : IComparer<ScheduledTask>
{
    public int Compare(ScheduledTask x, ScheduledTask y)
    {
        return x.ExecuteAt.CompareTo(y.ExecuteAt);
    }
}
