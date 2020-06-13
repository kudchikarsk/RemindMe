using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Animation;
using System;
using Android.Views.Animations;
using Android.Views;
using System.Timers;
using RemindMe.Animators;
using Android.Content;
using Xamarin.Essentials;
using Android.Util;
using RemindMe.Services;
using static Android.Content.Res.Resources;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace RemindMe
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        #region private fields
        private RelativeLayout rootLayout;
        private ImageButton more;
        private ImageView water, wave1, wave2, wave3, wave4;
        private TextView remainingPercentTxt;
        private TextView timerPrev;
        private TextView timerCurr;
        private TextView timerNext;
        private RelativeLayout timerLayout;
        private int timerTextHeight;
        private Button reset;
        private WaterAnimator waterAnimator;
        private BeatAnimator beatAnimator;
        private double oldPercent = 0;
        private string originalText;
        private Intent timerService;
        private TimerBroadcastReceiver timerBraodcast;
        private bool isStarted;
        private bool isWarned;
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            var themeId = Preferences.Get("theme", 0);
            if (themeId > 0) SetTheme(themeId);

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            #region bind controls
            rootLayout = FindViewById<RelativeLayout>(Resource.Id.rootLayout);
            more = FindViewById<ImageButton>(Resource.Id.more);
            more.Click += More_Click;
            water = FindViewById<ImageView>(Resource.Id.water);
            wave1 = FindViewById<ImageView>(Resource.Id.wave1);
            wave2 = FindViewById<ImageView>(Resource.Id.wave2);
            wave3 = FindViewById<ImageView>(Resource.Id.wave3);
            wave4 = FindViewById<ImageView>(Resource.Id.wave4);
            remainingPercentTxt = FindViewById<TextView>(Resource.Id.remainingPercentTxt);
            timerPrev = FindViewById<TextView>(Resource.Id.timerPrev);
            timerCurr = FindViewById<TextView>(Resource.Id.timerCurr);
            timerNext = FindViewById<TextView>(Resource.Id.timerNext);
            timerLayout = FindViewById<RelativeLayout>(Resource.Id.timerLayout);
            timerCurr.Post(() =>
            {
                timerTextHeight = timerCurr.Height;
            });
            timerLayout.RemoveAllViews();
            reset = FindViewById<Button>(Resource.Id.reset);
            reset.Click += Reset_Click;
            originalText = reset.Text;
            #endregion

            #region build animators
            waterAnimator = new WaterAnimator(water, wave1, wave2, wave3, wave4, Resources);
            water.Post(() =>
            {
                waterAnimator.Build();
            });
            beatAnimator = new BeatAnimator(remainingPercentTxt);
            beatAnimator.Build();
            #endregion

            #region  initialize services
            timerService = new Intent(this, typeof(TimerService));
            timerBraodcast = new TimerBroadcastReceiver();
            timerBraodcast.TimerStarted += Receiver_TimeStarted;
            timerBraodcast.TimeChanged += Receiver_TimeChanged;
            timerBraodcast.CountdownFinished += Receiver_CountdownFinished;
            RegisterReceiver(timerBraodcast, new IntentFilter("com.kudchikarsk.remindme.timer"));
            #endregion

            var start = Intent.GetBooleanExtra("start", false);
            if (start)
            {
                StartTimerService();
                reset.Text = originalText;
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            waterAnimator.Resume();
            
        }

        protected override void OnPause()
        {
            base.OnPause();
            waterAnimator.Pause();
            isStarted = false;

            
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            UnregisterReceiver(timerBraodcast);
            timerBraodcast.TimerStarted -= Receiver_TimeStarted;
            timerBraodcast.TimeChanged -= Receiver_TimeChanged;
            timerBraodcast.CountdownFinished -= Receiver_CountdownFinished;
            timerBraodcast.Dispose();
            timerBraodcast = null;

        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void Receiver_TimeStarted(long duration, long playFrom)
        {
            water.Post(() =>
            {
                waterAnimator.Reset();
                waterAnimator.Play(duration, playFrom);
                isStarted = true;
                isWarned = false;
                var percent = CalcRemainingPercent(duration, playFrom);
                UpdateRemainingPercent(percent);
            });
        }

        private async void Receiver_TimeChanged(long duration, long elapsed)
        {
            double percent = CalcRemainingPercent(duration, elapsed);

            water.Post(() =>
            {
                if (!isStarted)
                {
                    waterAnimator.Cancel();
                    waterAnimator.Play(duration, elapsed);
                    isStarted = true;
                    isWarned = false;
                }

                //update percent text
                UpdateRemainingPercent(percent);

                var timeSpan = TimeSpan.FromMilliseconds(duration - elapsed);

                UpdateClockTimer(timeSpan);

            });

            //send warning if applicable
            await SendWarning(percent);
        }
        

        private async void Receiver_CountdownFinished()
        {
            reset.Post(() =>
            {
                waterAnimator.End();
                UpdateRemainingPercent(0);
                reset.Text = "RESET";
            });

            await SendTimesUpMessage();
        }

        private void More_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(MenuActivity));
            StartActivity(intent);
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            StartTimerService();
            reset.Text = originalText;
        }

        private void UpdateRemainingPercent(double percent)
        {
            remainingPercentTxt.Post(() =>
            {
                //set remaining time in percent
                remainingPercentTxt.Text = percent.ToString();
                remainingPercentTxt.Invalidate();
                remainingPercentTxt.RequestLayout();

                //animate beat if value change
                if (percent > 0 && oldPercent != percent)
                    beatAnimator.Play();
                oldPercent = percent;
            });
        }

        private async Task SendTimesUpMessage()
        {
            await SpeakAsync(GetAttrString(Resource.Attribute.timeIsUpMsg));
        }

        private async Task SendWarning(double percent)
        {
            if (percent < 10 && percent > 5 && !isWarned)
            {
                isWarned = true;
                await SpeakAsync(GetAttrString(Resource.Attribute.warningMsg));
            }
        }

        private void UpdateClockTimer(TimeSpan timeSpan)
        {
            timerLayout.Post(() =>
            {
                var text = timeSpan.ToString(@"hh\:mm\:ss");
                TextView remainingTime = CreateTimerTextView(text);
                AddNewTimeInLayout(remainingTime);
                if (timeSpan.TotalSeconds == 0)
                {
                    PushBlankTimer(timeSpan);
                }

            });
        }

        private static double CalcRemainingPercent(long duration, long elapsed)
        {
            return (1 - (((double)elapsed) / duration)) * 100;
        }

        private void PushBlankTimer(TimeSpan timeSpan)
        {
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                timerLayout.Post(() =>
                {
                    var blankTime = CreateTimerTextView("");
                    AddNewTimeInLayout(blankTime);
                });
            });
        }        

        private void AddNewTimeInLayout(View newTimeView)
        {
            timerLayout.AddView(newTimeView, 0);
            newTimeView.Post(() =>
            {
                newTimeView.TranslationY = -newTimeView.Height;
                AnimateTimerTextViews();

                if (timerLayout.ChildCount > 4)
                {
                    timerLayout.RemoveView(
                        timerLayout.GetChildAt(
                            timerLayout.ChildCount - 1));
                }
            });

        }

        private void AnimateTimerTextViews()
        {
            var animators = new List<AnimatorSet>();
            for (int i = 0; i < timerLayout.ChildCount; i++)
            {
                var view = timerLayout.GetChildAt(i);
                var translateY = ObjectAnimator.OfFloat(view, "translationY", i * view.Height);
                var alpha = ObjectAnimator.OfFloat(view, "alpha", GetAlpha(i));
                var iset = new AnimatorSet();
                iset.PlayTogether(translateY, alpha);
                iset.SetInterpolator(new Android.Views.Animations.LinearInterpolator());
                iset.SetDuration(500);
                animators.Add(iset);
            }

            var set = new AnimatorSet();
            set.PlayTogether(animators.ToArray());
            set.Start();
        }

        private TextView CreateTimerTextView(string text)
        {
            var textView = new TextView(new ContextThemeWrapper(this, Resource.Style.timer_view));
            textView.Text = text;
            textView.Gravity = GravityFlags.CenterHorizontal;
            textView.TranslationY = -timerTextHeight;
            textView.Alpha = 0;
            return textView;
        }

        private float GetAlpha(int i)
        {
            switch (i)
            {
                case 0:
                case 2:
                    return 0.35f;
                case 3:
                    return 0;
                default:
                    return 1;

            }
        }

        private void StartTimerService()
        {
            var hr = Preferences.Get("hr", 0);
            var min = Preferences.Get("min", 2);
            var sec = Preferences.Get("sec", 0);
            timerService.PutExtra("hr", hr);
            timerService.PutExtra("min", min);
            timerService.PutExtra("sec", sec);
            StartService(timerService);
        }

        private async Task SpeakAsync(string value)
        {
            SpeechOptions settings = await GetSpeechOptions();
            await TextToSpeech.SpeakAsync(value, settings);
        }

        private static async Task<SpeechOptions> GetSpeechOptions()
        {
            var locales = await TextToSpeech.GetLocalesAsync();
            var defaultLocale = GetDefaultLanguage(locales);
            var selectedLocale = GetSelectedLanguageOrDefault(locales, defaultLocale);
            var settings = new SpeechOptions()
            {
                Locale = selectedLocale,
            };
            return settings;
        }

        private static Locale GetSelectedLanguageOrDefault(IEnumerable<Locale> locales, Locale defaultLocale)
        {
            return locales.FirstOrDefault(l => l.Name == Preferences.Get("locale", defaultLocale.Name));
        }

        private static Locale GetDefaultLanguage(IEnumerable<Locale> locales)
        {
            return locales
                .FirstOrDefault(l => 
                        l.Name
                        .ToLower()
                        .Contains("english")
                    ) ?? 
                locales.FirstOrDefault();
        }

        private string GetAttrString(int attrId)
        {
            TypedValue value = new TypedValue();

            if (!Theme.ResolveAttribute(attrId, value, true))
            {
                return null;
            }

            return value.CoerceToString();
        }
    }
}