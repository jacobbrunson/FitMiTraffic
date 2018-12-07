using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using System;

namespace NewTrafficRacer
{
	[Activity(Label = "NewTrafficRacer"
		, MainLauncher = true
		, Icon = "@drawable/icon"
		, Theme = "@style/Theme.Splash"
		, AlwaysRetainTaskState = true
		, LaunchMode = Android.Content.PM.LaunchMode.SingleInstance
		, ScreenOrientation = ScreenOrientation.FullUser
		, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout)]
	public class Activity1 : Microsoft.Xna.Framework.AndroidGameActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

            Android.Graphics.Point displaySize = new Android.Graphics.Point();
            this.WindowManager.DefaultDisplay.GetSize(displaySize);

            string content_dir = Intent.GetStringExtra("CONTENT_DIR");
            int duration = Intent.GetIntExtra("duration", 15);

            Console.WriteLine(GetType().AssemblyQualifiedName);

            var g = new TrafficGame(displaySize.X, displaySize.Y, content_dir, duration);
			SetContentView((View)g.Services.GetService(typeof(View)));
			g.Run();
		}
	}
}

