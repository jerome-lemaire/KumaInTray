using System.Text.Json;

public static class Translator
{
    private static Dictionary<string, string> _texts = new();

    public static void Initialize(string? langOverride)
    {
        // On prend la langue forcée dans la conf, sinon la langue du système
        string langCode = string.IsNullOrWhiteSpace(langOverride) 
            ? System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName 
            : langOverride.ToLower();

        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string localeFilePath = Path.Combine(basePath, "locales", $"{langCode}.json");

        // Si le fichier de la langue n'existe pas, on bascule sur l'anglais par défaut
        if (!File.Exists(localeFilePath))
        {
            localeFilePath = Path.Combine(basePath, "locales", "en.json");
        }

        // Si l'anglais existe bien, on charge le dictionnaire
        if (File.Exists(localeFilePath))
        {
            string jsonContent = File.ReadAllText(localeFilePath);
            _texts = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent) ?? new();
        }
    }

    // Récupère la traduction, ou renvoie la clé si elle n'existe pas dans le JSON
    public static string Get(string key)
    {
        return _texts.TryGetValue(key, out string? value) ? value : key;
    }
}