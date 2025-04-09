namespace Copilot.Utils;
public class LoggerPlus
{
    private Copilot Copilot => Copilot.Main;

    private string _fileName;

    public LoggerPlus(string fileName)
    {
        _fileName = fileName;
    }

    public void Message(string message)
    {
        if (!Copilot.Settings.Additional.Debug) return;
        Copilot.LogMessage($"[{_fileName}] {message}");
    }

    public void Error(string message)
    {
        if (!Copilot.Settings.Additional.Debug) return;
        Copilot.LogError($"[{_fileName}] {message}");
    }
}