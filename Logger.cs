public enum LogLevel
{
    DEBUG = 0,
    INFO = 1,
    WARN = 2,
    ERROR = 3,
    NONE = 4
}

public static class Logger
{
    private static readonly string LogDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
    private static readonly object _lock = new();

    // Niveau par défaut si rien n'est précisé dans le fichier de configuration
    public static LogLevel MinimumLevel { get; set; } = LogLevel.INFO;

    public static void Log(string message, LogLevel level = LogLevel.INFO)
    {
        // Si le niveau du message est inférieur au niveau minimum requis, on l'ignore
        if (level < MinimumLevel) return;

        try
        {
            lock (_lock)
            {
                if (!Directory.Exists(LogDir))
                {
                    Directory.CreateDirectory(LogDir);
                }

                string fileName = $"KumaInTray_{DateTime.Now:yyyy-MM-dd}.log";
                string filePath = Path.Combine(LogDir, fileName);
                
                // On formate l'entrée avec le nom du niveau (ex: [INFO], [ERROR])
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}";

                File.AppendAllText(filePath, logEntry);
            }
        }
        catch
        {
            // On ignore silencieusement les erreurs d'écriture
        }
    }
}