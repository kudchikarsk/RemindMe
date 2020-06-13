using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using static RemindMe.MainActivity;

namespace RemindMe.Services
{
    [Service]
    class TimerService : Service
    {
        private CountDownTimer timer;
        private Intent timerBroadcastIntent;

        public override void OnCreate()
        {
            base.OnCreate();
            timer = new CountDownTimer(0,0);
            timer.TimeChanged += Timer_TimeChanged;
            timer.CountDownFinished += Timer_CountDownFinished;
            timerBroadcastIntent = new Intent("com.kudchikarsk.remindme.timer");
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (intent != null)
            {
                var hr = intent.GetIntExtra("hr", 0);
                var min = intent.GetIntExtra("min", 0);
                var sec = intent.GetIntExtra("sec", 0);
                timer.SetTime(new DateTime(1, 1, 1, hr, min, sec));
            }
            
            timer.Start();
            TimerStarted();

            return StartCommandResult.NotSticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (timer != null) 
            {
                timer.TimeChanged -= Timer_TimeChanged;
                timer.CountDownFinished -= Timer_CountDownFinished;
                timer.Dispose();
            }
            timer = null;
        }

        private void TimerStarted()
        {
            timerBroadcastIntent.PutExtra("status", 0);
            timerBroadcastIntent.PutExtra("duration", timer.Duration);
            timerBroadcastIntent.PutExtra("playFrom", timer.Elpased);
            SendBroadcast(timerBroadcastIntent);
        }

        private void Timer_TimeChanged()
        {
            timerBroadcastIntent.PutExtra("status", 1);
            timerBroadcastIntent.PutExtra("duration", timer.Duration);
            timerBroadcastIntent.PutExtra("playFrom", timer.Elpased);
            SendBroadcast(timerBroadcastIntent);
        }

        private void Timer_CountDownFinished()
        {
            timerBroadcastIntent.PutExtra("status", 2);
            SendBroadcast(timerBroadcastIntent);
            timer.TimeChanged -= Timer_TimeChanged;
            timer.CountDownFinished -= Timer_CountDownFinished;
            timer.Dispose();
            timer = null;
            StopSelf();
        }
    }
}