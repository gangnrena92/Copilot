using System.Collections.Generic;
using System.Windows.Forms;

namespace Copilot.Settings;

public class CustomSettings
{
    public bool IsEnabled { get; set; }
    public int ActionCooldown { get; set; }
    public string TaskName { get; set; }
    public List<Keys> Keys { get; set; }
    public List<int> Ranges { get; set; }
    public string CodeSnippet { get; set; }

    public CustomSettings(bool isEnabled = false, int actionCooldown = 1000, string taskName = "Custom Task Name", List<Keys> keys = null, List<int> ranges = null, string codeSnippet = "")
    {
        IsEnabled = isEnabled;
        ActionCooldown = actionCooldown;
        TaskName = taskName;
        Keys = keys ?? new List<Keys>();
        Ranges = ranges ?? new List<int>();
        CodeSnippet = codeSnippet;
    }
}