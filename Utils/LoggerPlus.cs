namespace Copilot.Utils;
public class LoggerPlus
{
    private Copilot Copilot => Copilot.Main;

    private string _fileName;

    public LoggerPlus(string fileName)
    {
        _fileName = fileName;
    }

    public void Message(string message, int delay = 5)
    {
        if (!Copilot.Settings.Additional.Debug) return;
        Copilot.LogMessage($"[{_fileName}] {message}", delay);
    }

    public void Error(string message)
    {
        if (!Copilot.Settings.Additional.Debug) return;
        Copilot.LogError($"[{_fileName}] {message}", 10);
    }
}