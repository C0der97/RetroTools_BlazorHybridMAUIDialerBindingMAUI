using Android.Content;
using Android.Telecom;
using Android.Telephony;
using Android.OS;
using PayRemind.Contracts;
using PayRemind.Platforms.Android;
using Application = Android.App.Application;

[assembly: Dependency(typeof(CallHandlingService))]
namespace PayRemind.Platforms.Android
{
    public class CallHandlingService : ICallHandler
    {
        public void AnswerCall()
        {
            var call = CallService.Instance?.ActiveCall;
            if (call != null)
            {
                call.Answer(VideoProfileState.AudioOnly);
            }
            else
            {
                // Legacy method or fallback
                var telecomManager = (TelecomManager)Application.Context.GetSystemService(Context.TelecomService);
                if (Application.Context.CheckSelfPermission(global::Android.Manifest.Permission.AnswerPhoneCalls) == global::Android.Content.PM.Permission.Granted)
                {
                    telecomManager.AcceptRingingCall();
                }
            }
        }

        public void RejectCall()
        {
             var call = CallService.Instance?.ActiveCall;
            if (call != null)
            {
                call.Reject(false, "");
            }
            else
            {
                var telecomManager = (TelecomManager)Application.Context.GetSystemService(Context.TelecomService);
                 if (Application.Context.CheckSelfPermission(global::Android.Manifest.Permission.AnswerPhoneCalls) == global::Android.Content.PM.Permission.Granted)
                {
                    telecomManager.EndCall();
                }
            }
        }

        public void EndCall()
        {
            var call = CallService.Instance?.ActiveCall;
            if (call != null)
            {
                call.Disconnect();
            }
            else
            {
                // Fallback using TelecomManager
                var telecomManager = (TelecomManager)Application.Context.GetSystemService(Context.TelecomService);
                if (telecomManager != null)
                {
                    telecomManager.EndCall();
                }
            }
        }

        public void ToggleMute()
        {
            var service = CallService.Instance;
            if (service != null && service.CallAudioState != null)
            {
                bool isMuted = service.CallAudioState.IsMuted;
                service.SetMuted(!isMuted);
            }
        }

        public void ToggleSpeaker()
        {
            var service = CallService.Instance;
            if (service != null && service.CallAudioState != null)
            {
                CallAudioRoute route = service.CallAudioState.Route;
                if (route == CallAudioRoute.Speaker)
                {
                    service.SetAudioRoute(CallAudioRoute.Earpiece);
                }
                else
                {
                    service.SetAudioRoute(CallAudioRoute.Speaker);
                }
            }
        }
        
        public void PlaceCall(string phoneNumber)
        {
            try
            {
                var telecomManager = (TelecomManager)Application.Context.GetSystemService(Context.TelecomService);
                if (telecomManager != null)
                {
                    var uri = global::Android.Net.Uri.Parse($"tel:{phoneNumber}");
                    var extras = new Bundle();
                    telecomManager.PlaceCall(uri, extras);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error placing call: {ex.Message}");
            }
        }
    }
}
