using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using FashionStore.Services;
using FashionStore.Data;
using FashionStore.Core;
using FashionStore.Data.Interfaces;
using FashionStore.Data.Repositories;
using FashionStore.ViewModels;

namespace FashionStore
{
    public partial class App : Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // 0. Setup Serilog
            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FashionStore", "Logs", "log-.txt");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("--- KHOI DONG UNG DUNG ---");

            // 1. Setup Global Error Handling
            SetupGlobalErrorHandling();

            // 2. Setup Dependency Injection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();

            // 3. Load Settings and set Vietnamese culture
            var settings = SettingsManager.Load();

            var cultureInfo = new System.Globalization.CultureInfo("vi-VN");
            cultureInfo.NumberFormat.CurrencySymbol = "đ";

            System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
            System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    System.Windows.Markup.XmlLanguage.GetLanguage(cultureInfo.IetfLanguageTag)));

            base.OnStartup(e);

            // Run database initialization asynchronously to not block UI show
            Dispatcher.BeginInvoke(new Action(TryInitializeDatabaseWithFallback), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IVoucherRepository, VoucherRepository>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            
            // Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IVoucherService, VoucherService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<ICalculationService, CalculationService>();
            
            // ViewModels
            services.AddTransient<InvoiceManagementViewModel>();
            // services.AddTransient<MainViewModel>();
        }


        private void SetupGlobalErrorHandling()
        {
            this.DispatcherUnhandledException += (sender, args) =>
            {
                args.Handled = true;
                string errorMessage = $"Đã xảy ra lỗi không mong muốn:\n\n{args.Exception.Message}";
                if (args.Exception.InnerException != null)
                {
                    errorMessage += $"\n\nChi tiết: {args.Exception.InnerException.Message}";
                }
                
                Log.Error(args.Exception, "Dispatcher Unhandled Exception");
                
                MessageBox.Show(errorMessage, "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                Log.Fatal(ex, "Critical AppDomain Unhandled Exception");
            };
            
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                args.SetObserved();
                Log.Error(args.Exception, "Unobserved Task Exception");
            };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                Log.Information("--- DONG UNG DUNG ---");
                Log.CloseAndFlush();
            }
            catch { }

            try
            {
                foreach (Window w in Current.Windows)
                {
                    try { w.Close(); } catch { }
                }
            }
            catch { }

            try
            {
                MySql.Data.MySqlClient.MySqlConnection.ClearAllPools();
            }
            catch { }

            try
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.GC.Collect();
            }
            catch { }

            base.OnExit(e);

            try
            {
                System.Environment.Exit(0);
            }
            catch
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }

        private void TryInitializeDatabaseWithFallback()
        {
            try
            {
                DatabaseHelper.InitializeDatabase();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Khởi tạo cơ sở dữ liệu thất bại.\n\n{ex.Message}", "Lỗi cơ sở dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                var result = MessageBox.Show("Mở cài đặt để cấu hình kết nối cơ sở dữ liệu ngay bây giờ?", "Kết nối cơ sở dữ liệu", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var settings = new SettingsWindow();
                    settings.ShowDialog();
                    try
                    {
                        DatabaseHelper.InitializeDatabase();
                    }
                    catch (System.Exception retryEx)
                    {
                        MessageBox.Show($"Cơ sở dữ liệu vẫn không khả dụng. Ứng dụng có thể không hoạt động đúng.\n\n{retryEx.Message}", "Lỗi cơ sở dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
