using Android.App;
using Android.Content;
using Android.Telecom;
using PayRemind.Data;
using System;
using Application = Android.App.Application;

namespace PayRemind.Platforms.Android
{

    public class IncomingCallService : CallScreeningService
    {
        public override async void OnScreenCall(Call.Details callDetails)
        {
            if (callDetails.CallDirection == CallDirection.Incoming)
            {
                // 'Handle' property on Java.Lang.Object claims the name. The binding usually exposes GetHandle() or Handle property with 'new'.
                // If it is nint, it means we got the wrong one.
                var handle = callDetails.GetHandle();
                string phoneNumber = handle?.SchemeSpecificPart;
                
                // We need to resolve the repository. 
                // Since this is an Android Service, we might not have direct access to Maui's DI container easily 
                // if the app hasn't fully started or if scopes are different.
                // However, MauiProgram.CreateMauiApp returns the built app which has Services.
                
                // For simplicity/reliability in this context, we'll try to get it from IPlatformApplication
                var app = Application.Context as MauiApplication;
                var services = IPlatformApplication.Current?.Services;

                if (services != null)
                {
                    try 
                    {
                        var repo = services.GetService<BlockedNumberRepository>();
                        if (repo != null)
                        {
                            bool isBlocked = await repo.IsBlockedAsync(phoneNumber);

                            if (isBlocked)
                            {
                                RespondToCall(callDetails, new CallResponse.Builder()
                                    .SetDisallowCall(true)
                                    .SetRejectCall(true)
                                    .SetSkipCallLog(false)
                                    .SetSkipNotification(true)
                                    .Build());
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"Error checking blocked number: {ex.Message}");
                    }
                }
            }

            // Allow normal processing if not blocked
            RespondToCall(callDetails, new CallResponse.Builder()
                .SetDisallowCall(false)
                .SetRejectCall(false)
                .SetSkipCallLog(false)
                .SetSkipNotification(false)
                .Build());
        }
    }
}
