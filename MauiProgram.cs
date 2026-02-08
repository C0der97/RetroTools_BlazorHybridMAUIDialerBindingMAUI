using CommunityToolkit.Maui;
using Maui.NullableDateTimePicker;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using MudBlazor.Services;
using PayRemind.Contracts;
using PayRemind.Data;


#if ANDROID
using PayRemind.Platforms.Android;
#endif
using PayRemind.Shared;
using Plugin.LocalNotification;

namespace PayRemind
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "notifications.db");

            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>()
                .ConfigureNullableDateTimePicker().
                UseLocalNotification().ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            })
                .ConfigureLifecycleEvents(events =>
                {
#if ANDROID
                events.AddAndroid(android => android.OnNewIntent((activity, intent) =>
                {
                    if (intent.Action == Android.Content.Intent.ActionCall)
                    {
                        var uri = intent.Data;
                        if (uri != null)
                        {
                            var phoneNumber = uri.SchemeSpecificPart;
                            // Aquí puedes manejar el número de teléfono,
                            // por ejemplo, navegando a tu página de marcación
                            // o iniciando directamente la llamada
                        }
                    }
                }));
#endif

                })
                .UseMauiCommunityToolkit();
                /*.UseSentry(options => {
                // The DSN is the only required setting.
                options.Dsn = "https://42213245402bf788a901d07f056950d4@o4507708174958592.ingest.us.sentry.io/4507708177448960";

                // Use debug mode if you want to see what the SDK is doing.
                // Debug messages are written to stdout with Console.Writeline,
                // and are viewable in your IDE's debug console or with 'adb logcat', etc.
                // This option is not recommended when deploying your application.
                options.Debug = true;

                // Set TracesSampleRate to 1.0 to capture 100% of transactions for performance monitoring.
                // We recommend adjusting this value in production.
                options.TracesSampleRate = 1.0;

                // Other Sentry options can be set here.
            });*/

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddMudServices();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

#if ANDROID
            builder.Services.AddSingleton<PhoneCallReceiver>();
            builder.Services.AddSingleton<ICallHandler, CallHandlingService>();
            builder.Services.AddSingleton<IViewConverterService, ViewConverterService>();
            builder.Services.AddSingleton<ICallLogService, CallLogService>();
            builder.Services.AddSingleton<IContactsService, ContactsService>(); // Registered ContactsService

            DependencyService.Register<IViewConverterService, ViewConverterService>();
            DependencyService.Register<ICallLogService, CallLogService>();
            DependencyService.Register<IContactsService, ContactsService>();



#endif

            builder.Services.AddSingleton<BlockedNumberRepository>(s => ActivatorUtilities.CreateInstance<BlockedNumberRepository>(s, dbPath));
            builder.Services.AddSingleton<SharedStateService>();
            return builder.Build();
        }
    }
}