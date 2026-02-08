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
using CommunityToolkit.Mvvm.Messaging;
using PayRemind.Messages;

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

            // Check if this is an incoming or outgoing call
            var details = call.GetDetails();
            bool isIncoming = details?.CallDirection == global::Android.Telecom.CallDirection.Incoming;

            // Check the screen state
            bool isScreenLocked = ((KeyguardManager)GetSystemService(Context.KeyguardService)).IsKeyguardLocked;
            bool isDeviceInteractive = ((PowerManager)GetSystemService(Context.PowerService)).IsInteractive;

            if (isIncoming)
            {
                // For incoming calls, show notification and open app
                ShowNotification(call, true);
                
                // Send message to update UI for incoming call
                string phoneNumber = "Desconocido";
                try
                {
                    var handleUri = details?.Handle;
                    if (handleUri != null)
                    {
                        phoneNumber = handleUri.ToString()?.Replace("tel:", "") ?? "Desconocido";
                    }
                }
                catch { }
                WeakReferenceMessenger.Default.Send(new IncomingCallMessage(phoneNumber));
                
                if (!isDeviceInteractive || isScreenLocked)
                {
                    StartActivity(new Intent(this, typeof(MainActivity)));
                }
            }
            else
            {
                // For outgoing calls, just show a minimal ongoing notification
                ShowNotification(call, false);
                
                // Send message to update UI for outgoing call state
                WeakReferenceMessenger.Default.Send(new CallStateChangedMessage("Dialing"));
            }
        }

        public override void OnCallRemoved(Call call)
        {
            base.OnCallRemoved(call);
            if (ActiveCall == call) ActiveCall = null;
            call.UnregisterCallback(_callCallback);
            CancelNotification();
        }

        private void ShowNotification(Call call, bool isIncoming)
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

            var callDetails = call.GetDetails();
            var callHandle = callDetails?.Handle;
            string phoneNumber = "Desconocido";
            try
            {
                if (callHandle != null)
                {
                    phoneNumber = callHandle.ToString()?.Replace("tel:", "") ?? "Desconocido";
                }
            }
            catch { }
            
            global::Android.Graphics.Drawables.Drawable? persIcon = ContextCompat.GetDrawable(
                                            this, Resource.Drawable.ic_call_answer_low);
            global::Android.Graphics.Bitmap? bitmap = null;
            if (persIcon != null)
            {
                global::Android.Graphics.Drawables.BitmapDrawable bitmapDrawable = (BitmapDrawable)persIcon;
                bitmap = bitmapDrawable.Bitmap;
            }

            IconCompat iconNotif = IconCompat.CreateWithResource(this, Resource.Drawable.ic_call_answer);

            if (isIncoming)
            {
                // Incoming call notification with answer/decline buttons
                var intentAnswer = new Intent(this, typeof(PhoneCallReceiver));
                intentAnswer.SetAction("ANSWER");
                var pendingIntentAnswer = PendingIntent.GetBroadcast(this, 0, intentAnswer, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

                var intentHangup = new Intent(this, typeof(PhoneCallReceiver));
                intentHangup.SetAction("HANGUP");
                var pendingIntentHangup = PendingIntent.GetBroadcast(this, 1, intentHangup, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

                RemoteViews remoteViews = new RemoteViews(Platform.CurrentActivity?.PackageName, Resource.Layout.notification_custom);
                remoteViews.SetOnClickPendingIntent(Resource.Id.button_accept_call, pendingIntentAnswer);
                remoteViews.SetOnClickPendingIntent(Resource.Id.button_decline_call, pendingIntentHangup);
                remoteViews.SetTextViewText(Resource.Id.notification_caller_name, phoneNumber);
                remoteViews.SetTextViewText(Resource.Id.button_accept_call, "Contestar");
                remoteViews.SetTextViewText(Resource.Id.button_decline_call, "Colgar");

                RemoteViews remoteViewsSmall = new RemoteViews(Platform.CurrentActivity?.PackageName, Resource.Layout.notification_custom_small);
                remoteViewsSmall.SetOnClickPendingIntent(Resource.Id.button_accept_call, pendingIntentAnswer);
                remoteViewsSmall.SetOnClickPendingIntent(Resource.Id.button_decline_call, pendingIntentHangup);
                remoteViewsSmall.SetTextViewText(Resource.Id.button_accept_call, "Contestar");
                remoteViewsSmall.SetTextViewText(Resource.Id.button_decline_call, "Colgar");

                var notificationBuilder = new NotificationCompat.Builder(this, channelId)
                    .SetContentTitle("Llamada entrante")
                    .SetContentText($"Llamada de {phoneNumber}")
                    .SetSmallIcon(iconNotif)
                    .SetPriority(NotificationCompat.PriorityHigh)
                    .SetCustomBigContentView(remoteViews)
                    .SetStyle(new NotificationCompat.DecoratedCustomViewStyle())
                    .SetCustomContentView(remoteViewsSmall)
                    .SetOngoing(true)
                    .SetAutoCancel(true);

                Notification notification = notificationBuilder.Build();
                notificationManager.Notify(10203, notification);
            }
            else
            {
                // Outgoing call notification - simple ongoing notification
                var intentHangup = new Intent(this, typeof(PhoneCallReceiver));
                intentHangup.SetAction("HANGUP");
                var pendingIntentHangup = PendingIntent.GetBroadcast(this, 1, intentHangup, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

                var notificationBuilder = new NotificationCompat.Builder(this, channelId)
                    .SetContentTitle("Llamando...")
                    .SetContentText(phoneNumber)
                    .SetSmallIcon(iconNotif)
                    .SetPriority(NotificationCompat.PriorityDefault)
                    .AddAction(Resource.Drawable.ic_call_decline, "Colgar", pendingIntentHangup)
                    .SetOngoing(true)
                    .SetAutoCancel(false);

                Notification notification = notificationBuilder.Build();
                notificationManager.Notify(10203, notification);
            }
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
                WeakReferenceMessenger.Default.Send(new CallStateChangedMessage(state.ToString()));
                
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
