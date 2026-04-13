# KumaInTray 🐻🟩

[![Build Status](https://github.com/jerome-lemaire/KumaInTray/actions/workflows/build.yml/badge.svg)](https://github.com/jerome-lemaire/KumaInTray/actions)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

KumaInTray is a lightweight Windows application that sits in your system tray to monitor the real-time status of your services via **Uptime Kuma**. 

Perfect for keeping an eye on your homelab services (Jellyfin, Sonarr, Radarr, etc.) right from your Windows 11 taskbar, without needing to keep a browser tab open.

## ✨ Features

* **Visual Indicator:** Green dot when everything is fine, red if a service goes offline.
* **Smart Notifications:** Windows balloon tip the moment a downtime is detected, specifically listing the affected services.
* **Quick Access:** A simple left-click on the icon opens your Uptime Kuma dashboard.
* **Lightweight & Portable:** Compiled into a single, standalone `.exe` file.
* **Multilingual:** Native support for multiple languages via easy-to-edit JSON files.

## 🚀 Installation

1. Download the latest version from the [Releases](../../releases) page.
2. Extract the archive into your preferred folder.
3. Configure the `appsettings.json` file (see below).
4. Run `KumaInTray.exe`.

> **Tip:** To launch the app at Windows startup, press `Win + R`, type `shell:startup`, and place a shortcut to `KumaInTray.exe` in that folder.

## ⚙️ Configuration

Next to your executable, create or modify the `appsettings.json` file with your Uptime Kuma instance details:

```json
{
  "UptimeKuma": {
    "DashboardUrl": "http://192.168.X.X:3001/",
    "MetricsUrl": "http://192.168.X.X:3001/metrics",
    "ApiKey": "YOUR_API_KEY",
    "Language": "" 
  }
}
```

* **DashboardUrl:** The URL of your web interface (opens on left-click).
* **MetricsUrl:** The URL of the Uptime Kuma Prometheus endpoint.
* **ApiKey:** API key to authorize reading metrics (see instructions below).
* **Language:** Leave empty to use your Windows system language, or force a specific language (e.g., `"en"`, `"fr"`).

### 🔑 How to generate an API Key

Since Uptime Kuma secures its metrics endpoint by default, you need to provide an API key for KumaInTray to read the data:

1. Open your Uptime Kuma dashboard in your browser.
2. Click on your profile dropdown in the top right corner and select **Settings**.
3. Navigate to the **API Keys** tab on the left menu.
4. Click on the **Provision API Key** button.
5. Give it a descriptive name (e.g., `KumaInTray App`) and confirm.
6. **Important:** Copy the generated token immediately and paste it into your `appsettings.json` file. You will not be able to see this key again once you close the window!

## 🌍 Contributing: Adding a Translation

KumaInTray is designed to be easily translated by the community. You **don't need to know how to code** or recompile the project to add a language!

1. Fork this repository.
2. Go to the `locales/` folder.
3. Create a new file with the desired two-letter language code (e.g., `es.json` for Spanish, `de.json` for German).
4. Copy the following structure and translate the values on the right side of the colons:

```json
{
  "Checking": "Uptime Kuma - Checking...",
  "Quit": "Quit",
  "ServicesDown": "Uptime Kuma: One or more services are DOWN!",
  "AlertTitle": "Uptime Kuma Alert",
  "AlertMessage": "A service just went down! Check your dashboard.",
  "ServicesUp": "Uptime Kuma: All services are UP",
  "ConnectionError": "Connection error: ",
  "ConfigErrorTitle": "Error",
  "ConfigErrorMessage": "The UptimeKuma:MetricsUrl is missing."
}
```
5. Submit your *Pull Request*. Thank you for your help! ❤️

## 🛠️ Manual Compilation

If you want to compile the project yourself:
Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

```bash
git clone https://github.com/jerome-lemaire/KumaInTray.git
cd KumaInTray
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained false
```