using FashionStore.App.ViewModels;
using FashionStore.App.Views;
using FashionStore.Core.Interfaces;
using FashionStore.Core.Settings;
using FashionStore.Data;
using FashionStore.Data.Repositories;
using FashionStore.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.IO;
using System.Windows;


namespace FashionStore.App
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
            ServiceLocator.ServiceProvider = ServiceProvider;

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

            // 4. Show Splash Screen
            var splash = new SplashWindow();
            splash.Show();

            base.OnStartup(e);

            // Run database initialization and app setup asynchronously 
            Task.Run(async () => {
                try 
                {
                    // Artificial delay to show branding (can be removed in production if too fast)
                    await Task.Delay(1500); 
                    
                    await Dispatcher.InvokeAsync(() => {
                        TryInitializeDatabaseWithFallback();
                        
                        var mainWindow = new MainWindow();
                        Application.Current.MainWindow = mainWindow;
                        mainWindow.Show();
                        splash.Close();
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error during app startup sequence");
                }
            });
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Repositories
            services.AddScoped<ISystemSettingsRepository, SystemSettingsRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IVoucherRepository, VoucherRepository>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<IPromotionRepository, PromotionRepository>();
            services.AddScoped<IStockMovementRepository, StockMovementRepository>();
            services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
            services.AddScoped<IFinanceRepository, FinanceRepository>();
            services.AddScoped<IShiftRepository, ShiftRepository>();

            // Services
            services.AddScoped<ISystemSettingsService, SystemSettingsService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<FashionStore.Core.Interfaces.IProductService, FashionStore.Services.ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IVoucherService, VoucherService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<IPromotionService, PromotionService>();
            services.AddScoped<ICalculationService, CalculationService>();
            services.AddScoped<IBankStatementService, SePayBankService>();
            services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            services.AddScoped<IFinanceService, FinanceService>();
            services.AddScoped<IShiftService, ShiftService>();
            services.AddScoped<StockMovementService>();

            // Infrastructure
            services.AddSingleton<ICacheService, InMemoryCacheService>();
            services.AddSingleton<FashionStore.Core.Interfaces.INotificationService, FashionStore.Services.NotificationService>();

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
            catch (Exception ex)
            {
                Log.Warning(ex, "Error while closing Serilog on exit");
            }

            try
            {
                foreach (Window w in Current.Windows)
                {
                    try { w.Close(); } catch (Exception ex) { Log.Warning(ex, "Error closing window {Window}", w.GetType().Name); }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error while closing windows on exit");
            }

            try
            {
                MySql.Data.MySqlClient.MySqlConnection.ClearAllPools();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error clearing connection pools");
            }

            try
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.GC.Collect();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error during GC collect on exit");
            }

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
