using Android.App;
using Android.Content;
using Android.Telecom;
using Android.Telephony;
using Android.Util;
using Android.Widget;

namespace PayRemind.Platforms.Android
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter([TelephonyManager.ActionPhoneStateChanged, "ANSWER", "HANGUP"  ])]
    public class PhoneCallReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            string? action = intent.Action;


            if (action != TelephonyManager.ActionPhoneStateChanged && action != "ANSWER" && action != "HANGUP")
            {
                Log.Debug("Cosa", "Received intent with non-phone state action");
                return;

            }


            if (action == "ANSWER")
            {
                TelecomManager? telecomManager = context.GetSystemService(Context.TelecomService) as TelecomManager;

                telecomManager?.AcceptRingingCall();

                Toast.MakeText(context, "Contestar Llamada", ToastLength.Short)
                    .Show();
            }
            else if (action == "HANGUP")
            {
                TelecomManager? telecomManager = context.GetSystemService(Context.TelecomService) as TelecomManager;


                if (telecomManager != null && context != null)
                {
                    telecomManager?.EndCall();

                    Toast.MakeText(context, "Colgar Llamada", ToastLength.Short).Show();
                }

                }


                string? state = intent.GetStringExtra(TelephonyManager.ExtraState);
            if (state == TelephonyManager.ExtraStateRinging)
            {

                string? incomingNumber = intent?.GetStringExtra(name: 
                    TelephonyManager.ExtraIncomingNumber
                    );

                //var serviceIntent = new Intent(context, typeof(CallService));
                //_ = context.StartForegroundService(serviceIntent);


                if (!App.AppActive)
                {
                    //var startIntent = new Intent(context, typeof(MainActivity));
                    ////startIntent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    //startIntent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    //startIntent.PutExtra("OpenCallPage", true);
                    //startIntent.PutExtra("IncomingNumber", incomingNumber);
                    //context.StartActivity(startIntent);
                }
                else
                {
                        // Abre la aplicación y navega a la página de llamadas
                        //var startIntent = new Intent(context, typeof(MainActivity));
                        //startIntent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                        intent?.PutExtra("OpenCallPage", true);
                        intent?.PutExtra("IncomingNumber", incomingNumber);
                        //context.StartActivity(startIntent);
                }
            }
            else if(state == TelephonyManager.ExtraStateIdle)
            {
                if (Microsoft.Maui.Controls.Application.Current != null)
                {
                    Microsoft.Maui.Controls.Application.Current.MainPage = new MainPage();
                }
            }

        }
    }
    }
