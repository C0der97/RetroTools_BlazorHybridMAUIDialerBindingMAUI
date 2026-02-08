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

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddMudServices();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

#if ANDROID
            builder.Services.AddSingleton<PhoneCallReceiver>();
            builder.Services.AddSingleton<PayRemind.Contracts.IContactsService, PayRemind.Platforms.Android.ContactsService>();
            builder.Services.AddSingleton<PayRemind.Contracts.ICallLogService, PayRemind.Platforms.Android.CallLogService>();
            builder.Services.AddSingleton<PayRemind.Contracts.ICallHandler, PayRemind.Platforms.Android.CallHandlingService>();
            builder.Services.AddSingleton<PayRemind.Contracts.ISmsService, PayRemind.Platforms.Android.SmsService>();
            builder.Services.AddSingleton<IViewConverterService, ViewConverterService>(); // Registered ContactsService

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