using System;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Linq;

namespace KumaTray;

static class Program
{
  [STAThread]
  static void Main()
  {
    // AppDomain.CurrentDomain.BaseDirectory est plus sûr que Directory.GetCurrentDirectory() 
    // quand l'appli est lancée via un raccourci au démarrage de Windows
    var builder = new ConfigurationBuilder()
        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

    IConfiguration config = builder.Build();

    // Récupération de l'URL depuis le JSON
    string? metricsUrl = config["UptimeKuma:MetricsUrl"];
    string? apiKey = config["UptimeKuma:ApiKey"];
    string? langOverride = config["UptimeKuma:Language"];
    string? dashboardUrl = config["UptimeKuma:DashboardUrl"];

    // Initialisation des traductions avant de lancer l'interface
    Translator.Initialize(langOverride);

    if (string.IsNullOrWhiteSpace(metricsUrl))
    {
      MessageBox.Show(Translator.Get("ConfigErrorMessage"), Translator.Get("ConfigErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
      return;
    }

    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);

    // On passe l'URL récupérée au constructeur
    Application.Run(new TrayApplicationContext(metricsUrl, apiKey, dashboardUrl));
  }
}

public class TrayApplicationContext : ApplicationContext
{
  private readonly NotifyIcon _trayIcon;
  private readonly System.Windows.Forms.Timer _pollTimer;
  private readonly HttpClient _httpClient;
  private readonly string _metricsUrl;
  private readonly string? _dashboardUrl;
  private bool _wasDownLastCheck = false;
  public TrayApplicationContext(string metricsUrl, string? apiKey, string? dashboardUrl)
  {
    _metricsUrl = metricsUrl;
    _dashboardUrl = dashboardUrl;
    _httpClient = new HttpClient();

    // Ajout automatique de l'authentification si la clé est présente
    if (!string.IsNullOrWhiteSpace(apiKey))
    {
      var authBytes = System.Text.Encoding.ASCII.GetBytes($"api:{apiKey}");
      _httpClient.DefaultRequestHeaders.Authorization =
          new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
    }

    _trayIcon = new NotifyIcon
    {
      Icon = CreateIcon(Color.Gray),
      Visible = true,
      Text = Translator.Get("Checking")
    };

    // Ajout d'un menu clic-droit pour pouvoir fermer l'appli
    var menu = new ContextMenuStrip();
    menu.Items.Add(Translator.Get("Quit"), null, (s, e) => Exit());
    _trayIcon.ContextMenuStrip = menu;

    _trayIcon.MouseClick += TrayIcon_MouseClick;

    // Configuration du timer (ex: check toutes les 30 secondes)
    _pollTimer = new System.Windows.Forms.Timer { Interval = 30000 };
    _pollTimer.Tick += async (s, e) => await CheckUptimeKumaAsync();
    _pollTimer.Start();

    // Lancement du premier check sans attendre
    _ = CheckUptimeKumaAsync();
  }

  private async Task CheckUptimeKumaAsync()
  {
    try
    {
      string metrics = await _httpClient.GetStringAsync(_metricsUrl);

      // La Regex cherche les lignes à 0 et capture la valeur de monitor_name
      MatchCollection downMatches = Regex.Matches(metrics, @"monitor_status\{[^}]*monitor_name=""([^""]+)""[^}]*\}\s*0");

      if (downMatches.Count > 0)
      {
        // On extrait tous les noms capturés (Groupe 1 de la Regex) et on les joint avec une virgule
        var downServices = downMatches.Select(m => m.Groups[1].Value).ToList();
        string namesList = string.Join(", ", downServices);

        _trayIcon.Icon = CreateIcon(Color.Red);

        // Au survol (limité à 63 caractères par Windows)
        string hoverText = $"{Translator.Get("ServicesDown")} ({downMatches.Count})";
        _trayIcon.Text = hoverText.Length > 63 ? hoverText.Substring(0, 63) : hoverText;

        // On n'affiche la bulle Windows qu'au moment de la bascule UP -> DOWN
        if (!_wasDownLastCheck)
        {
          // Dans la notification (BalloonTip), on a beaucoup plus de place
          string alertMessage = $"{Translator.Get("AlertMessage")}\n\nHS : {namesList}";

          // Sécurité : on tronque si la liste est vraiment gigantesque pour éviter un crash
          if (alertMessage.Length > 200) alertMessage = alertMessage.Substring(0, 200) + "...";

          _trayIcon.ShowBalloonTip(7000, Translator.Get("AlertTitle"), alertMessage, ToolTipIcon.Error);
          _wasDownLastCheck = true;
        }
      }
      else
      {
        _trayIcon.Icon = CreateIcon(Color.Green);
        _trayIcon.Text = Translator.Get("ServicesUp");
        _wasDownLastCheck = false;
      }
    }
    catch (Exception ex)
    {
      _trayIcon.Icon = CreateIcon(Color.Orange);
      string errorText = $"{Translator.Get("ConnectionError")}{ex.Message}";
      _trayIcon.Text = errorText.Length > 63 ? errorText.Substring(0, 63) : errorText;
    }
  }

  // Génère un petit cercle coloré en guise d'icône
  private Icon CreateIcon(Color color)
  {
    using Bitmap bitmap = new Bitmap(16, 16);
    using Graphics g = Graphics.FromImage(bitmap);
    g.Clear(Color.Transparent);
    using Brush brush = new SolidBrush(color);
    g.FillEllipse(brush, 2, 2, 12, 12);

    return Icon.FromHandle(bitmap.GetHicon());
  }

  private void TrayIcon_MouseClick(object? sender, MouseEventArgs e)
  {
    // On ne réagit qu'au clic gauche (le clic droit gère déjà le menu)
    if (e.Button == MouseButtons.Left && !string.IsNullOrWhiteSpace(_dashboardUrl))
    {
      OpenBrowser(_dashboardUrl);
    }
  }
  private void OpenBrowser(string url)
  {
    try
    {
      // UseShellExecute = true est obligatoire en .NET Core pour lancer une URL
      Process.Start(new ProcessStartInfo
      {
        FileName = url,
        UseShellExecute = true
      });
    }
    catch (Exception ex)
    {
      MessageBox.Show($"{Translator.Get("ConnectionError")}{ex.Message}", Translator.Get("ConfigErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
  }
  private void Exit()
  {
    _trayIcon.Visible = false;
    Application.Exit();
  }
}