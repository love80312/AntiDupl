using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using AntiDupl.NET.Core;
using AntiDupl.NET.WPF.Core;
using AntiDupl.NET.WPF.Helper;
using AntiDupl.NET.WPF.Model;
using AntiDupl.NET.WPF.Properties;
using AntiDupl.NET.WPF.Service;
using AntiDupl.NET.WPF.UndoRedo;
using AntiDupl.NET.WPF.View;
using AntiDupl.NET.WPF.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AntiDupl.NET.Deep.Core;
using AntiDupl.NET.Deep.Extensions;

namespace AntiDupl.NET.WPF
{
    public partial class App
    {
        static ServiceProvider _serviceProvider;

        public App()
        {
            Startup += Application_Startup;
        }

        /// <summary>
        /// Читает параметр вида "&lt;name&gt; &lt;value&gt;" из аргументов командной строки
        /// (зеркало WF Program.GetParameter).
        /// </summary>
        static string GetParameter(string[] args, string name)
        {
            if (args == null)
                return null;
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (String.Compare(args[i], name, true) == 0)
                    return args[i + 1];
            }
            return null;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException +=
                   new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            // Стандартное логирование (ILogger) в файл: <BaseDirectory>\\logs
            FileLoggerProvider fileLogProvider =
                new FileLoggerProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"));

            // Стандартный DI-контейнер (Microsoft.Extensions.DependencyInjection)
            ServiceCollection services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddProvider(fileLogProvider);
            });

            // CLI: -s <dir> — custom user path; иначе используется каталог по умолчанию.
            // UserPath задаётся до ConfigurationModel.Load(), чтобы настройки читались правильно.
            string customSavePath = GetParameter(e.Args, "-s");
            if (customSavePath != null)
            {
                if (!Directory.Exists(customSavePath))
                    throw new Exception(string.Format("The directory '{0}' is not exists!", customSavePath));
                AntiDupl.NET.WPF.Resources.Resources.UserPath = customSavePath;
            }
            else
            {
                AntiDupl.NET.WPF.Resources.Resources.UserPath = AntiDupl.NET.WPF.Resources.Resources.GetDefaultUserPath();
            }

            services.AddSingleton<IConfigurationModel>(ConfigurationModel.Load());
            services.AddSingleton<ILanguageService, LanguageService>();

            services.AddSingleton(new CoreLib(AntiDupl.NET.WPF.Resources.Resources.UserPath));
            services.AddSingleton<IImageLoader, ImageLoader>();
            services.AddSingleton<IThumbnailProvider, ThumbnailProvider>();

            services.AddSingleton<IUndoRedoEngine, UndoRedoEngine>();
            services.AddSingleton<IViewModeModel, ViewModeModel>();
            services.AddSingleton<ICoreLocationSync, CoreLibLocationSync>();

            // Deep (DINOv2) search. This build uses CPU-only ONNX Runtime; GPU fallback UI is disabled.
            services.AddDeepSearch(
                () =>
                {
                    string modelsDirectory = AntiDupl.NET.Deep.Infra.DeepModelManager.GetModelsDirectory(AppContext.BaseDirectory);
                    Directory.CreateDirectory(modelsDirectory);
                    string cacheDirectory = Path.Combine(AntiDupl.NET.WPF.Resources.Resources.UserPath, "deep");
                    Directory.CreateDirectory(cacheDirectory);
                    string logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
                    return (ModelsDirectory: modelsDirectory, CacheDirectory: cacheDirectory, LogDirectory: logDirectory);
                },
                _ => { });

            services.AddSingleton<MainViewModel>();

            _serviceProvider = services.BuildServiceProvider();
            ServiceLocator.Provider = _serviceProvider;

            // 損毀的設定檔：寫入記錄並通知使用者。
            Helper.SerializeHelperEvents.LoadErrorOccurred += (s, e) =>
            {
                if (_serviceProvider != null)
                    _serviceProvider.GetRequiredService<ILoggerFactory>()
                        .CreateLogger("SerializeHelper")
                        .LogError("SerializeHelper failed to load {File}: {Message}", e.FilePath, e.Message);
                string format = Application.Current?.Resources["serialize_load_error"] as string
                    ?? "File {0} is corrupted and has been reset to default settings";
                MessageBox.Show(string.Format(format, e.FilePath),
                    Application.Current?.Resources["mainwindow_title"] as string ?? "AntiDupl",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            };

            ILoggerFactory loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.CreateLogger<App>()
                .LogInformation("Запуск приложения. Log: {LogPath}", fileLogProvider.LogFilePath);

            MainViewModel viewModel = _serviceProvider.GetRequiredService<MainViewModel>();

            // CPU-only build: do not prepend legacy CUDA/cuDNN paths to the process PATH.
            // The old properties remain in ConfigurationModel for configuration.xml compatibility.
            var shell = new MainWindow();
            shell.DataContext = viewModel;
            shell.Loaded += new RoutedEventHandler(viewModel.OnLoaded);
            shell.Closing += new CancelEventHandler(viewModel.OnClosing);
            shell.Show();
        }

        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogError("DispatcherUnhandledException", e.Exception);
            MessageBox.Show(e.Exception.Message, "Uncaught Thread Exception",
                            MessageBoxButton.OK, MessageBoxImage.Error);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            LogError("AppDomain.UnhandledException", ex);
            MessageBox.Show(ex.Message, "Uncaught Thread Exception",
                            MessageBoxButton.OK, MessageBoxImage.Error);
        }

        static void LogError(string source, Exception ex)
        {
            if (_serviceProvider == null || ex == null)
                return;
            _serviceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger(source).LogError(ex, "{Source}: {Message}", source, ex.Message);
        }
    }
}
