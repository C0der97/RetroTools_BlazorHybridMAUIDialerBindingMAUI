// Stub class to allow compilation without the ShowCaseView binding library
// The actual ShowCaseView functionality is disabled until the binding library is available

namespace PayRemind.MauiWrapper
{
    public class ShowCaseViewWrapper
    {
        // Stub method - does nothing
        public static void ShowGuideView(Android.Views.View targetView1, Android.Views.View targetView2, string title1, string content1, string title2, string content2)
        {
            // ShowCaseView binding library not available - tutorial feature disabled
            System.Diagnostics.Debug.WriteLine("ShowCaseView feature is disabled - binding library not available");
        }

        // Stub method - does nothing
        public static void ShowGuideView(Android.Views.View targetView1, string title1, string content1)
        {
            // ShowCaseView binding library not available - tutorial feature disabled
            System.Diagnostics.Debug.WriteLine("ShowCaseView feature is disabled - binding library not available");
        }
    }
}
