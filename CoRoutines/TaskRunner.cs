using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using ExileCore2.Shared;

namespace Copilot.CoRoutines;
public static class TaskRunner
{
    private static readonly ConcurrentDictionary<string, CancellationTokenSource> Tasks = [];

    public static void Run(Func<SyncTask<bool>> task, string name)
    {
        var cts = new CancellationTokenSource();
        Tasks[name] = cts;
        Task.Run(async () =>
        {
            var sTask = task();
            while (sTask != null && !cts.Token.IsCancellationRequested)
            {
                TaskUtils.RunOrRestart(ref sTask, () => null);
                await TaskUtils.NextFrame();
            }

            Tasks.TryRemove(new KeyValuePair<string, CancellationTokenSource>(name, cts));
        });
    }

    public static void Stop(string name)
    {
        if (Tasks.TryGetValue(name, out var cts))
        {
            cts.Cancel();
            Tasks.TryRemove(new KeyValuePair<string, CancellationTokenSource>(name, cts));
        }
        SyncInput.ReleaseKeys();
    }

    public static bool Has(string name)
    {
        return Tasks.ContainsKey(name);
    }
}