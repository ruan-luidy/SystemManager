// SysManager · ServiceRegistration — DI container configuration
// Author: laurentiu021 · https://github.com/laurentiu021/SystemManager
// License: MIT

using Microsoft.Extensions.DependencyInjection;
using SysManager.Features.About;
using SysManager.Features.AppAlerts;
using SysManager.Features.AppAlerts.Services;
using SysManager.Features.AppBlocker;
using SysManager.Features.AppUpdates;
using SysManager.Features.AudioMixer;
using SysManager.Features.AudioMixer.Services;
using SysManager.Features.BandwidthMonitor;
using SysManager.Features.BandwidthMonitor.Services;
using SysManager.Features.BatteryHealth;
using SysManager.Features.BootAnalyzer;
using SysManager.Features.BrowserCleaner;
using SysManager.Features.BulkInstaller;
using SysManager.Features.BulkInstaller.Services;
using SysManager.Features.Cleanup;
using SysManager.Features.CliInterface;
using SysManager.Features.ContextMenu;
using SysManager.Features.ContextMenu.Services;
using SysManager.Features.CpuAffinity;
using SysManager.Features.DarkMode;
using SysManager.Features.Dashboard;
using SysManager.Features.Dashboard.Services;
using SysManager.Features.Debloater;
using SysManager.Features.Debloater.Services;
using SysManager.Features.DeepCleanup;
using SysManager.Features.Defender;
using SysManager.Features.Defender.Services;
using SysManager.Features.DiskAnalyzer;
using SysManager.Features.DiskAnalyzer.Services;
using SysManager.Features.DisplayProfile;
using SysManager.Features.DisplayProfile.Services;
using SysManager.Features.DnsHosts;
using SysManager.Features.DnsHosts.Services;
using SysManager.Features.Drivers;
using SysManager.Features.DuplicateFile;
using SysManager.Features.DuplicateFile.Services;
using SysManager.Features.EdgeOneDrive;
using SysManager.Features.EdgeOneDrive.Services;
using SysManager.Features.EnvironmentVariables;
using SysManager.Features.EnvironmentVariables.Services;
using SysManager.Features.FileLock;
using SysManager.Features.FileLock.Services;
using SysManager.Features.FileShredder;
using SysManager.Features.FileShredder.Services;
using SysManager.Features.Gaming;
using SysManager.Features.LargeFiles;
using SysManager.Features.LargeFiles.Services;
using SysManager.Features.LegacyPanels;
using SysManager.Features.LegacyPanels.Services;
using SysManager.Features.Logs;
using SysManager.Features.Logs.Services;
using SysManager.Features.NetworkRepair;
using SysManager.Features.NotificationBlocker;
using SysManager.Features.Performance;
using SysManager.Features.Ping;
using SysManager.Features.Privacy;
using SysManager.Features.PrivacyMonitor;
using SysManager.Features.PrivacyMonitor.Services;
using SysManager.Features.ProcessManager;
using SysManager.Features.Profile;
using SysManager.Features.ResourceHistory;
using SysManager.Features.ResourceHistory.Services;
using SysManager.Features.RestorePoints;
using SysManager.Features.ScheduledMaintenance;
using SysManager.Features.ScheduledMaintenance.Services;
using SysManager.Features.SettingsWatchdog;
using SysManager.Features.ShortcutCleaner;
using SysManager.Features.SpeedTest;
using SysManager.Features.StandbyMemory;
using SysManager.Features.StandbyMemory.Services;
using SysManager.Features.Startup;
using SysManager.Features.Startup.Services;
using SysManager.Features.SystemFixes;
using SysManager.Features.SystemFixes.Services;
using SysManager.Features.SystemHealth;
using SysManager.Features.SystemHealth.Services;
using SysManager.Features.SystemReport;
using SysManager.Features.TaskScheduler;
using SysManager.Features.TaskScheduler.Services;
using SysManager.Features.TimerResolution;
using SysManager.Features.Traceroute;
using SysManager.Features.TweaksHub;
using SysManager.Features.TweaksHub.Services;
using SysManager.Features.Uninstaller;
using SysManager.Features.Uninstaller.Services;
using SysManager.Features.WindowsFeatures;
using SysManager.Features.WindowsFeatures.Services;
using SysManager.Features.WindowsServices;
using SysManager.Features.WindowsServices.Services;
using SysManager.Features.WindowsUpdate;
using SysManager.Features.WindowsUpdate.Services;
using SysManager.Shared;
using SysManager.Shared.Helpers;
using SysManager.Shared.Services;

namespace SysManager;

/// <summary>
/// Registers all services and ViewModels in the DI container.
/// Called once at application startup from <see cref="App.OnStartup"/>.
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        // ── Core services ──────────────────────────────────────────────
        // PowerShellRunner is Transient — each consumer gets its own instance
        // to avoid LineReceived event cross-talk between tabs. All consumers
        // depend on IPowerShellRunner (substitutable in tests via NSubstitute).
        services.AddTransient<IPowerShellRunner, PowerShellRunner>();
        services.AddSingleton<SystemInfoService>();
        // WingetService is Transient so each consuming ViewModel gets its own
        // IPowerShellRunner instance — avoids LineReceived cross-talk when
        // Dashboard and AppUpdates both run winget concurrently.
        services.AddTransient<IWingetService, WingetService>();
        services.AddSingleton<TrayIconService>();
        services.AddSingleton<IUpdateService, UpdateService>();
        services.AddSingleton<ShortcutCleanerService>();
        services.AddSingleton<DiskHealthService>();
        services.AddSingleton<TemperatureService>();
        services.AddSingleton<SystemReportService>();
        services.AddSingleton<BatteryService>();
        services.AddSingleton<ITuneUpService, TuneUpService>();
        services.AddSingleton<HealthScoreService>();
        services.AddSingleton<AppAlertService>();
        services.AddSingleton<IAppBlockerService, AppBlockerService>();
        services.AddSingleton<DeepCleanupService>();
        services.AddSingleton<ICleanupPreScanService, CleanupPreScanService>();
        services.AddSingleton<DiskAnalyzerService>();
        services.AddSingleton<DiskScanHistoryService>();
        services.AddSingleton<DuplicateFileService>();
        services.AddSingleton<EventLogService>();
        services.AddSingleton<FixedDriveService>();
        services.AddSingleton<LargeFileScanner>();
        services.AddSingleton<MemoryTestService>();
        services.AddSingleton<NetworkRepairService>();
        services.AddSingleton<PerformanceService>();
        services.AddSingleton<PingMonitorService>();
        services.AddSingleton<ProcessManagerService>();
        services.AddSingleton<SpeedTestHistoryService>();
        services.AddSingleton<SpeedTestService>();
        // The same instance behind the seam: the network tabs take it concrete through NetworkSharedState,
        // and the Dashboard's quick test takes the interface.
        services.AddSingleton<ISpeedTestService>(sp => sp.GetRequiredService<SpeedTestService>());
        services.AddSingleton<StartupService>();
        services.AddSingleton<TracerouteMonitorService>();
        services.AddSingleton<TracerouteService>();
        services.AddSingleton<UninstallerService>();
        services.AddSingleton<BulkInstallerService>();
        services.AddSingleton<AppIconService>();
        services.AddSingleton<FileShredderService>();
        services.AddSingleton<PrivacyService>();
        services.AddSingleton<DnsService>();
        services.AddSingleton<HostsFileService>();
        services.AddSingleton<IContextMenuService, ContextMenuService>();
        services.AddSingleton<EnvironmentVariableService>();
        services.AddSingleton<RestorePointService>();
        // Singleton on purpose: "one restore point per session" has to mean the whole app, or two
        // tabs each take their own and the second burns the 24-hour rate limit on a duplicate.
        services.AddSingleton<ISessionRestorePoint>(sp =>
            new SessionRestorePoint(sp.GetRequiredService<RestorePointService>().CreateAsync));
        services.AddSingleton<DebloaterService>();
        services.AddSingleton<EdgeOneDriveService>();
        services.AddSingleton<LegacyPanelService>();
        services.AddSingleton<SystemFixService>();
        services.AddSingleton<ProfileService>();
        services.AddSingleton<BiosService>();
        services.AddSingleton<WindowsUpdatePolicyService>();
        services.AddSingleton<BrowserCleanerService>();
        services.AddSingleton<PrivacyMonitorService>();
        services.AddSingleton<BootAnalyzerService>();
        services.AddSingleton<IWindowsUpdateService, WindowsUpdateService>();
        services.AddSingleton<ITimerResolutionService, TimerResolutionService>();
        services.AddSingleton<IFileLockService, FileLockService>();
        services.AddSingleton<DisplayProfileService>();
        services.AddSingleton<ICpuAffinityService, CpuAffinityService>();
        services.AddSingleton<DefenderService>();
        services.AddSingleton<TaskSchedulerService>();
        services.AddSingleton<IWindowsThemeService, WindowsThemeService>();
        services.AddSingleton<StandbyMemoryService>();
        services.AddSingleton<StandbyPreferenceService>();
        services.AddSingleton<ServiceStartupLedgerService>();
        services.AddSingleton<CrashMarkerService>();
        services.AddSingleton<ResourceHistoryService>();
        services.AddSingleton<BandwidthHistoryService>();
        services.AddSingleton<ISettingsWatchdogService, SettingsWatchdogService>();
        services.AddSingleton<MaintenanceSchedulerService>();
        services.AddSingleton<ITweaksHubService, TweaksHubService>();
        services.AddSingleton<IAudioMixerService, AudioMixerService>();
        services.AddSingleton<VolumePresetService>();
        services.AddSingleton<INotificationBlockerService, NotificationBlockerService>();
        // Gaming Profile orchestrates the audited services above; it needs the process's
        // elevation state at construction (a value DI can't resolve), hence the factory.
        services.AddSingleton<IGamingProfileService>(sp => new GamingProfileService(
            sp.GetRequiredService<PerformanceService>(),
            sp.GetRequiredService<ITimerResolutionService>(),
            sp.GetRequiredService<ICpuAffinityService>(),
            sp.GetRequiredService<StandbyMemoryService>(),
            sp.GetRequiredService<ISessionRestorePoint>(),
            AdminHelper.IsElevated()));

        // Registered as the concrete type as well, because the shell needs Bind() while every tab needs
        // only the interface. One instance either way — two registrations of the same object, not two
        // objects, or the shell would bind an instance nobody navigates through.
        services.AddSingleton<NavigationService>();
        services.AddSingleton<INavigationService>(sp => sp.GetRequiredService<NavigationService>());

        // ── ViewModels (Singleton — one instance per tab) ──────────────
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<AppUpdatesViewModel>();
        services.AddSingleton<WindowsUpdateViewModel>();
        services.AddSingleton<SystemHealthViewModel>();
        services.AddSingleton<CleanupViewModel>();
        services.AddSingleton<DeepCleanupViewModel>();
        services.AddSingleton<LargeFilesViewModel>();
        services.AddSingleton<DuplicateFileViewModel>();
        services.AddSingleton<DiskAnalyzerViewModel>();
        services.AddSingleton<ProcessManagerViewModel>();
        services.AddSingleton<BatteryHealthViewModel>();
        services.AddSingleton<UninstallerViewModel>();
        services.AddSingleton<PerformanceViewModel>();
        services.AddSingleton<StartupViewModel>();
        services.AddSingleton<NetworkSharedState>();
        services.AddSingleton<PingViewModel>();
        services.AddSingleton<TracerouteViewModel>();
        services.AddSingleton<SpeedTestViewModel>();
        services.AddSingleton<NetworkRepairViewModel>();
        services.AddSingleton<DriversViewModel>();
        services.AddSingleton<LogsViewModel>();
        services.AddSingleton<AboutViewModel>();
        services.AddSingleton<ServicesViewModel>();
        services.AddSingleton<AppAlertsViewModel>();
        services.AddSingleton<ShortcutCleanerViewModel>();
        services.AddSingleton<WindowsFeaturesService>();
        services.AddSingleton<WindowsFeaturesViewModel>();
        services.AddSingleton<AppBlockerViewModel>();
        services.AddSingleton<BulkInstallerViewModel>();
        services.AddSingleton<FileShredderViewModel>();
        services.AddSingleton<PrivacyViewModel>();
        services.AddSingleton<DnsHostsViewModel>();
        services.AddSingleton<ContextMenuViewModel>();
        services.AddSingleton<SystemReportViewModel>();
        services.AddSingleton<EnvironmentVariablesViewModel>();
        services.AddSingleton<RestorePointsViewModel>();
        services.AddSingleton<DebloaterViewModel>();
        services.AddSingleton<EdgeOneDriveViewModel>();
        services.AddSingleton<LegacyPanelsViewModel>();
        services.AddSingleton<SystemFixesViewModel>();
        services.AddSingleton<ProfileViewModel>();
        services.AddSingleton<BrowserCleanerViewModel>();
        services.AddSingleton<PrivacyMonitorViewModel>();
        services.AddSingleton<BootAnalyzerViewModel>();
        services.AddSingleton<TimerResolutionViewModel>();
        services.AddSingleton<FileLockViewModel>();
        services.AddSingleton<DisplayProfileViewModel>();
        services.AddSingleton<CpuAffinityViewModel>();
        services.AddSingleton<DefenderViewModel>();
        services.AddSingleton<TaskSchedulerViewModel>();
        services.AddSingleton<DarkModeViewModel>();
        services.AddSingleton<StandbyMemoryViewModel>();
        services.AddSingleton<ResourceHistoryViewModel>();
        services.AddSingleton<BandwidthMonitorViewModel>();
        services.AddSingleton<SettingsWatchdogViewModel>();
        services.AddSingleton<CliInterfaceViewModel>();
        services.AddSingleton<ScheduledMaintenanceViewModel>();
        services.AddSingleton<TweaksHubViewModel>();
        services.AddSingleton<AudioMixerViewModel>();
        services.AddSingleton<GamingProfileViewModel>();
        services.AddSingleton<NotificationBlockerViewModel>();

        return services;
    }
}
