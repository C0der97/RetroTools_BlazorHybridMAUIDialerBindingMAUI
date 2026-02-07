using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Telecom;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.Core.Graphics.Drawable;
using Microsoft.Maui.Controls.PlatformConfiguration;
using static Android.Icu.Text.CaseMap;
using Person = Android.App.Person;

namespace PayRemind.Platforms.Android
{
    [Service(Enabled = true, Exported = true,
        Permission = "android.permission.BIND_INCALL_SERVICE"
        )]
    [IntentFilter(["android.telecom.InCallService"])]
    [MetaData(
        "android.telecom.IN_CALL_SERVICE_UI",
        Value = "true"
    )]
    public class CallService : InCallService
    {
        public static CallService Instance { get; private set; }
        public Call ActiveCall { get; private set; }

        private readonly Call.Callback _callCallback = new CallCallback();

        public override IBinder OnBind(Intent intent)
        {
            Instance = this;
            return base.OnBind(intent);
        }

        public override bool OnUnbind(Intent intent)
        {
            Instance = null;
            return base.OnUnbind(intent);
        }

        public override void OnCallAdded(Call call)
        {
            base.OnCallAdded(call);
            ActiveCall = call;
            call.RegisterCallback(_callCallback);

            // Check the screen state and whether to show the notification
            bool isScreenLocked = ((KeyguardManager)GetSystemService(Context.KeyguardService)).IsKeyguardLocked;
            bool isDeviceInteractive = ((PowerManager)GetSystemService(Context.PowerService)).IsInteractive;

            if (!isDeviceInteractive || isScreenLocked)
            {
                ShowNotification(call);
                StartActivity(new Intent(this, typeof(MainActivity)));
            }
            else
            {
                ShowNotification(call);
            }
        }

        public override void OnCallRemoved(Call call)
        {
            base.OnCallRemoved(call);
            if (ActiveCall == call) ActiveCall = null;
            call.UnregisterCallback(_callCallback);
            CancelNotification();
        }

        private void ShowNotification(Call call)
        {
            var notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);

            string channelId = "phone_call_channel";
            string channelName = "Phone Call Notifications";

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(channelId, channelName, NotificationImportance.High)
                {
                    LockscreenVisibility = NotificationVisibility.Private
                };
                notificationManager.CreateNotificationChannel(channel);
            }

            var notificationIntent = new Intent(this, typeof(MainActivity));
            var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.Immutable);

            var intentAnswer = new Intent(this, typeof(PhoneCallReceiver));
            intentAnswer.SetAction("ANSWER");
            var pendingIntentAnswer = PendingIntent.GetBroadcast(this, 0, intentAnswer, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var intentHangup = new Intent(this, typeof(PhoneCallReceiver));
            intentHangup.SetAction("HANGUP");
            var pendingIntentHangup = PendingIntent.GetBroadcast(this, 1, intentHangup, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);


            global::Android.Graphics.Drawables.Drawable? persIcon = ContextCompat.GetDrawable(
                                            this, Resource.Drawable.ic_call_answer_low);


            IconCompat iconNotif = IconCompat.CreateWithResource(this, Resource.Drawable.ic_call_answer_low);

            Icon personI =  Icon.CreateWithResource(this, Resource.Drawable.ic_call_answer_low);

            
            Person person = new Person.Builder()
                .SetName("Pepe Guama")
                .SetImportant(true)
                .SetIcon(personI)
                .Build();

            //LayoutInflater? layoutInflater = GetSystemService(LayoutInflaterService) as LayoutInflater;

            //global::Android.Views.View? view = layoutInflater?.Inflate(Resource.Layout.notification_custom, null);


            RemoteViews remoteViews = new RemoteViews(Platform.CurrentActivity?.PackageName, Resource.Layout.notification_custom);

            remoteViews.SetOnClickPendingIntent(Resource.Id.button_accept_call, pendingIntentAnswer);
            remoteViews.SetOnClickPendingIntent(Resource.Id.button_decline_call, pendingIntentHangup);
            remoteViews.SetTextViewText(Resource.Id.notification_caller_name, "Pepe Guama");
            remoteViews.SetTextViewText(Resource.Id.button_accept_call, "Contestar");
            remoteViews.SetTextViewText(Resource.Id.button_decline_call, "Colgar");


            RemoteViews remoteViewsSmall = new RemoteViews(Platform.CurrentActivity?.PackageName, Resource.Layout.notification_custom_small);


            remoteViewsSmall.SetOnClickPendingIntent(Resource.Id.button_accept_call, pendingIntentAnswer);
            remoteViewsSmall.SetOnClickPendingIntent(Resource.Id.button_decline_call, pendingIntentHangup);
            remoteViewsSmall.SetTextViewText(Resource.Id.button_accept_call, "Contestar");
            remoteViewsSmall.SetTextViewText(Resource.Id.button_decline_call, "Colgar");

            //var notificationStyle = Notification.CallStyle.ForIncomingCall(
            //    person,
            //    declineIntent,
            //    answerIntent)

            var notificationBuilder = new NotificationCompat.Builder(this, channelId)
                .SetContentTitle("Llamada entrante")
                .SetContentText("Tiene una llamada entrante.")
                .SetSmallIcon(iconNotif) // Ensure you have this drawable
                //.AddAction(Resource.Drawable.ic_call_answer, "Contestar", pendingIntentAnswer)
                //.AddAction(Resource.Drawable.ic_call_decline, "Colgar", pendingIntentHangup)
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetCustomBigContentView(remoteViews)
                .SetStyle(new NotificationCompat.DecoratedCustomViewStyle())
                .SetCustomContentView(remoteViewsSmall)
                .SetOngoing(true)
                .SetAutoCancel(true);

            Notification notification = notificationBuilder.Build();

            notificationManager.Notify(10203, notification);

            //StartForeground(1, notification, ForegroundService.TypePhoneCall);
        }

        private void CancelNotification()
        {
            var notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
            notificationManager.Cancel(1);
        }

        private class CallCallback : Call.Callback
        {
            public override void OnStateChanged(Call call, CallState state)
            {
                base.OnStateChanged(call, state);
                MessagingCenter.Send<object, string>(this, "CallStateChanged", state.ToString());
                
                if (state == CallState.Disconnected || state == CallState.Disconnecting)
                {
                    CallService.Instance?.CancelNotification();
                    if (CallService.Instance?.ActiveCall == call) 
                    {
                         // Using reflection to set private setter if needed, or just let OnCallRemoved handle it
                         // But we can't easily set ActiveCall property effectively from here if it was private set?
                         // Actually ActiveCall has private set, but this inner class is in CallService, so it can access it?
                         // No, Inner class access to outer class private members? Yes.
                         // But we need instance reference.
                    }
                }
            }
        }
    }
}
