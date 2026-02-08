using Android.App;
using Android.Content;
using Android.Provider;
using Android.Telephony;

namespace PayRemind.Platforms.Android
{
    [BroadcastReceiver(Name = "com.retrocode.payremind.SmsReceiver", Exported = true, Permission = global::Android.Manifest.Permission.BroadcastSms)]
    [IntentFilter(new[] { Telephony.Sms.Intents.SmsDeliverAction })]
    public class SmsReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            // Handle incoming SMS
        }
    }

    [BroadcastReceiver(Name = "com.retrocode.payremind.MmsReceiver", Exported = true, Permission = global::Android.Manifest.Permission.BroadcastWapPush)]
    [IntentFilter(new[] { Telephony.Sms.Intents.WapPushDeliverAction }, DataMimeType = "application/vnd.wap.mms-message")]
    public class MmsReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            // Handle incoming MMS
        }
    }

    [Service(Name = "com.retrocode.payremind.HeadlessSmsSendService", Exported = true, Permission = global::Android.Manifest.Permission.SendRespondViaMessage)]
    [IntentFilter(new[] { "android.intent.action.RESPOND_VIA_MESSAGE" }, DataSchemes = new[] { "sms", "smsto", "mms", "mmsto" })]
    public class HeadlessSmsSendService : Service
    {
        public override global::Android.OS.IBinder OnBind(Intent intent)
        {
            return null;
        }
    }
}
