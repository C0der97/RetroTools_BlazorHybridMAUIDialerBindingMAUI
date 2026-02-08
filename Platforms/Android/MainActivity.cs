using Android.App;
using Android.App.Roles;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Telecom;
using Android.Util;
using Android.Widget;
using CommunityToolkit.Mvvm.Messaging;
using PayRemind.Messages;
using PayRemind.Pages;
using PayRemind.Platforms.Android;
using PayRemind.Platforms.Android.Wrappers;

namespace PayRemind
{
    [Activity(Theme = "@style/Maui.SplashTheme", 
        MainLauncher = true,
        Exported = true,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ConfigChanges.ScreenSize | 
        ConfigChanges.Orientation | ConfigChanges.UiMode | 
        ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | 
        ConfigChanges.Density)]


    [IntentFilter(
        [Intent.ActionDial],
        Categories = [Intent.CategoryDefault],
        DataScheme = "tel")]

    [IntentFilter(
        [Intent.ActionDial],
        Categories = [Intent.CategoryDefault])]

    [IntentFilter(
        [Intent.ActionView, Intent.ActionDial],
        Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable, ],
        DataScheme = "tel")]

    [IntentFilter(
        [Intent.ActionSendto],
        Categories = [Intent.CategoryDefault],
        DataSchemes = ["sms", "smsto", "mms", "mmsto"])]
    public class MainActivity : MauiAppCompatActivity
    {
        public static MainActivity? ActivityCurrent { get; set; }
        private static int REQUEST_ID = 1007;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ActivityCurrent = this;

            App.AppActive = true;


            if (Intent != null && Intent.HasExtra("incoming_number"))
            {
                var incomingNumber = Intent.GetStringExtra("incoming_number");

                if (Microsoft.Maui.Controls.Application.Current != null)
                {
                    Microsoft.Maui.Controls.Application.Current.MainPage = new NavigationPage(new CallPage(incomingNumber));
                }
            }



            if (savedInstanceState == null)
            {


                if (savedInstanceState == null && Build.VERSION.SdkInt >= BuildVersionCodes.N)
                {
                    // Verificar si se debe abrir una página específica

                    if (GetSystemService(name: RoleService) is RoleManager roleManager)
                    {
                        Intent? intent = roleManager?.CreateRequestRoleIntent(
                            RoleManager.RoleCallScreening);
                        StartActivityForResult(intent, REQUEST_ID);
                    }

                }



//                bool isQPlus = Build.VERSION.SdkInt >= BuildVersionCodes.Q;

//                if (isQPlus)
//                {
//                    Log.Debug("Cosa", "entra en isQPlus");

//                    SetDefaultCallerIdApp();
//                }
//                else
//                {
//                    Log.Debug("Cosa", "no entra en isQPlus");


//                    if (OperatingSystem.IsAndroidVersionAtLeast(29)
//                        && GetSystemService(name: RoleService) is RoleManager roleManager)  // Android Q (API 29) y superior
//                    {
//                        if (roleManager != null &&
//                            roleManager.IsRoleAvailable(RoleManager.RoleDialer) &&
//                            !roleManager.IsRoleHeld(RoleManager.RoleDialer))
//                        {
//                            var intent = roleManager.CreateRequestRoleIntent(RoleManager.RoleDialer);
//                            //Platform.CurrentActivity?.StartActivityForResult(intent, 1007);
//                            StartActivityForResult(intent, 1007);

//                        }
//                    }
//                    else
//                    {
//                        Intent intent = new Intent(TelecomManager.ActionChangeDefaultDialer)
//                            .PutExtra(TelecomManager.ExtraChangeDefaultDialerPackageName,
//                            Platform.CurrentActivity?.PackageName);
//                        try
//                        {
//                            //Platform.CurrentActivity?.StartActivityForResult(intent, 1007);

//                            StartActivityForResult(intent, 1007);

//                        }
//                        catch (ActivityNotFoundException)
//                        {
//                            // Toast.MakeText(Platform.CurrentActivity, "No app found", ToastLength.Short).Show();
//                            // O usa el sistema de notificación de MAUI:
//                            MainThread.BeginInvokeOnMainThread(() =>
//                            {
//#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
//                                Toast.MakeText(this, "ERORR", ToastLength.Short)
//                                           .Show();
//#pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
//                            });
//                        }
//                        catch (Exception ex)
//                        {
//#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
//                            Toast.MakeText(this, "ERORR", ToastLength.Short)
//                                          .Show();
//#pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
//                        }
//                    }
//                }


            }



            //var serviceIntent = new Intent(this, typeof(CallService));
            //StartForegroundService(serviceIntent);



            if (Intent != null &&  Intent.GetBooleanExtra("OpenCallPage", false))
            {
                string? incomingNumber = Intent.GetStringExtra("IncomingNumber");

                WeakReferenceMessenger.Default.Send(new TabIndexMessage(2, incomingNumber ?? ""));
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent
                                             data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            Log.Debug("Cosa", "Obteniendo respuesta ");


            if (requestCode == REQUEST_ID)
            {
                if (resultCode == Result.Ok)
                {
                    Log.Debug("Cosa", "aceptado como default");


                    WeakReferenceMessenger.Default.Send(new TabIndexMessage(true, "Aceptada"));
                }
                else
                {
                    Log.Debug("Cosa", "No aceptado como default");


                    Toast.MakeText(this, "No aceptado como default", ToastLength.Short)
                                     .Show();

                }
            }

        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            if (intent != null)
            {

                if (intent.GetStringExtra("incoming_number") != string.Empty)
                {
                    var incomingNumber = intent.GetStringExtra("incoming_number");
                    Microsoft.Maui.Controls.Application.Current.MainPage = new CallPage(incomingNumber);
                }


                if (intent.GetBooleanExtra("OpenCallPage", false))
                {
                    string incomingNumber = intent.GetStringExtra("IncomingNumber");
                }
            }
        }

        private void SetDefaultCallerIdApp()
        {
            RoleManager? roleManager = GetSystemService(Context.RoleService) as RoleManager;
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
           
            
            //OLD BAD
            //if (roleManager.IsRoleAvailable(RoleManager.RoleCallScreening) && !roleManager.IsRoleHeld(RoleManager.RoleCallScreening))
            //{
            //    Intent? intent = roleManager?.CreateRequestRoleIntent(RoleManager.RoleCallScreening);
            //    //StartActivityForResult(intent, 1010);
            //    Platform.CurrentActivity?.StartActivityForResult(intent, REQUEST_ID);




            //}



            if (roleManager != null &&
                   roleManager.IsRoleAvailable(RoleManager.RoleDialer) &&
                   !roleManager.IsRoleHeld(RoleManager.RoleDialer))
            {
                var intent = roleManager.CreateRequestRoleIntent(RoleManager.RoleDialer);
                //Platform.CurrentActivity?.StartActivityForResult(intent, 1007);
                StartActivityForResult(intent, REQUEST_ID);
            }


#pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Microsoft.Maui.ApplicationModel.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

    }
}
