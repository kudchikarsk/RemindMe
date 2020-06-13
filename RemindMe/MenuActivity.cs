using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Support.V7.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Essentials;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Support.V4.Content;
using RemindMe.Services;

namespace RemindMe
{
    [Activity(Label = "MenuActivity")]
    public class MenuActivity : AppCompatActivity
    {
        private RelativeLayout rootLayout;
        private LinearLayout waterOption;
        private LinearLayout coffeeOption;
        private LinearLayout officeOption;
        private LinearLayout beerOption;
        private LinearLayout selectVoiceOption;
        private ImageButton moreBtn;
        private Intent mainIntent;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            var themeId = Preferences.Get("theme", 0);
            if (themeId > 0) SetTheme(themeId);

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_menu);

            #region bind controls
            rootLayout = FindViewById<RelativeLayout>(Resource.Id.rootLayout);
            waterOption = FindViewById<LinearLayout>(Resource.Id.waterOption);
            coffeeOption = FindViewById<LinearLayout>(Resource.Id.coffeeOption);
            officeOption = FindViewById<LinearLayout>(Resource.Id.officeOption);
            beerOption = FindViewById<LinearLayout>(Resource.Id.beerOption);
            selectVoiceOption = FindViewById<LinearLayout>(Resource.Id.selectVoiceOption);
            moreBtn = FindViewById<ImageButton>(Resource.Id.moreOption);

            waterOption.Click += WaterOption_Click;
            coffeeOption.Click += CoffeeOption_Click;
            officeOption.Click += OfficeOption_Click;
            beerOption.Click += BeerOption_Click;
            selectVoiceOption.Click += SelectVoiceOption_Click;
            moreBtn.Click += MoreBtn_Click;
            #endregion

            #region initialize components
            mainIntent = new Intent(this, typeof(MainActivity));
            mainIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
            #endregion
        }

        private void MoreBtn_Click(object sender, EventArgs e)
        {
            Finish();
        }

        private void SelectVoiceOption_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(SelectVoiceActivity));
            StartActivity(intent);
        }

        private void WaterOption_Click(object sender, EventArgs e)
        {
            SetBackground(Resource.Color.blueDark);
            Preferences.Set("hr", 1);
            Preferences.Set("min", 0);
            Preferences.Set("sec", 0);
            TakeABreak(Resource.Style.BlueTheme);
        }

        private void CoffeeOption_Click(object sender, EventArgs e)
        {
            SetBackground(Resource.Color.coffeeDark);
            Preferences.Set("hr", 1);
            Preferences.Set("min", 0);
            Preferences.Set("sec", 0);
            TakeABreak(Resource.Style.CoffeeTheme);
        }

        private void OfficeOption_Click(object sender, EventArgs e)
        {
            SetBackground(Resource.Color.clockDark);
            Preferences.Set("hr", 0);
            Preferences.Set("min", 45);
            Preferences.Set("sec", 0);
            TakeABreak(Resource.Style.ClockTheme);
        }

        private void BeerOption_Click(object sender, EventArgs e)
        {
            SetBackground(Resource.Color.beerDark);
            Preferences.Set("hr", 2);
            Preferences.Set("min", 0);
            Preferences.Set("sec", 0);
            TakeABreak(Resource.Style.BeerTheme);
        }

        private void SetBackground(int id)
        {
            rootLayout.SetBackgroundColor(
                new Color(ContextCompat.GetColor(this, id)));
        }

        private void TakeABreak(int value)
        {
            Preferences.Set("theme", value);
            var timerService = new Intent(this, typeof(TimerService));
            StopService(timerService);
            Task.Run(() =>
            {
                Task.Delay(3000);
                mainIntent.PutExtra("start", true);
                StartActivity(mainIntent);
            });
            
        }
    }
}