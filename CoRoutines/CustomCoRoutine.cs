using System;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Microsoft.CodeAnalysis.Scripting;
using System.Windows.Forms;
using System.Reflection;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;
using System.Runtime.Loader;

using ExileCore2;
using ExileCore2.Shared;
using ExileCore2.Shared.Enums;

using static Copilot.Copilot;
using Copilot.Utils;
using Copilot.Api;
using Copilot.Settings;

namespace Copilot.CoRoutines;

public class CustomCoRoutine
{
    private readonly CustomSettings Settings;
    private readonly string _taskId;
    private readonly LoggerPlus Log;

    private static InteractiveAssemblyLoader loader;
    private static MetadataReference metadataReference;

    public CustomCoRoutine(CustomSettings settings)
    {
        Settings = settings;
        _taskId = $"CustomCoRoutine_{Settings.TaskName}";
        Log = new LoggerPlus(_taskId);
    }

    static CustomCoRoutine()
    {
        unsafe
        {
            Assembly.GetExecutingAssembly().TryGetRawMetadata(out byte* blob, out int length);
            var moduleMetadata = ModuleMetadata.CreateFromMetadata((IntPtr)blob, length);
            var assemblyMetadata = AssemblyMetadata.Create(moduleMetadata);
            metadataReference = assemblyMetadata.GetReference();
            loader = new InteractiveAssemblyLoader();
            loader.RegisterDependency(typeof(Copilot).Assembly);
        }
    }

    public void Init()
    {
        if (Settings.IsEnabled)
        {
            TaskRunner.Run(Custom_Task, _taskId);
        }
    }

    public void Stop()
    {
        TaskRunner.Stop(_taskId);
    }

    private ScriptOptions ScriptOptions => ScriptOptions.Default
        .AddReferences(
            typeof(Vector2).Assembly,
            typeof(GameStat).Assembly,
            typeof(Core).Assembly
        )
        .AddReferences(typeof(Keys).Assembly)
        .AddReferences(metadataReference)
        .AddImports(
            "System.Collections.Generic", "System.Linq", "System.Numerics", "System.Windows.Forms", "System.Threading.Tasks",
            "Copilot", "Copilot.Classes", "Copilot.Utils", "Copilot.Settings", "Copilot.Settings.Tasks", "Copilot.Api",
            "ExileCore2", "ExileCore2.Shared", "ExileCore2.Shared.Enums",
            "ExileCore2.Shared.Helpers", "ExileCore2.PoEMemory.Components", "ExileCore2.PoEMemory.MemoryObjects",
            "ExileCore2.PoEMemory", "ExileCore2.PoEMemory.FilesInMemory"
        );

    private delegate string ScriptFuncAsync(ScriptGlobals globals);

    private AssemblyLoadContext CreateAlc()
    {
        var assemblyLoadContext = new AssemblyLoadContext($"bbb{Guid.NewGuid()}", true);
        assemblyLoadContext.Resolving += (context, name) => name.Name == "Copilot" ? Assembly.GetExecutingAssembly() : null;
        return assemblyLoadContext;
    }

    private (Func<ScriptGlobals, string> Func, string Exception) RebuildFunction(string SourceSnippet)
    {
        try
        {
            var @delegate = DelegateCompiler.CompileDelegate<ScriptFuncAsync>(
                SourceSnippet,
                ScriptOptions,
                CreateAlc()
            );
            return (s => @delegate(s), null);
        }
        catch (Exception e)
        {
            return (null, $"Expression compilation failed: {e.Message}");
        }
    }

    public async SyncTask<bool> Custom_Task()
    {
        Log.Message("Custom Task started");

        var (func, exception) = RebuildFunction(Settings.CodeSnippet);
        if (exception != null)
        {
            Log.Error($"Compilation Error: {exception}");
            return false;
        }

        while (true)
        {
            await Task.Delay(Settings.ActionCooldown);

            try
            {
                var globals = new ScriptGlobals
                {
                    Player = _player,
                    Target = _target,
                    Settings = Settings
                };
                var result = func(globals);
                Log.Message($"Result: {result}");
            }
            catch (Exception e)
            {
                Log.Error($"Runtime Error: {e.Message}");
                continue;
            }
        }
    }
}

public class ScriptGlobals
{
    public EntityWrapper Player { get; set; }
    public EntityWrapper Target { get; set; }
    public CustomSettings Settings { get; set; }
}