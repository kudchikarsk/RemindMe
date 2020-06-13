using Android.App;
using Android.Content;
using Android.OS;
using System;

namespace RemindMe
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { "com.kudchikarsk.remindme.timer" })]
    public class TimerBroadcastReceiver : BroadcastReceiver
    {
        public event Action<long, long> TimerStarted = delegate { };
        public event Action<long, long> TimeChanged = delegate { };
        public event Action CountdownFinished = delegate { };

        public TimerBroadcastReceiver()
        {

        }

        public override void OnReceive(Context context, Intent intent)
        {
            switch (intent.GetIntExtra("status", 0))
            {
                case 0:
                    TimerStarted(intent.GetLongExtra("duration", 0), intent.GetLongExtra("playFrom", 0));
                    return;

                case 1:
                    TimeChanged(intent.GetLongExtra("duration", 0), intent.GetLongExtra("playFrom", 0));
                    return;

                case 2:
                    CountdownFinished();
                    return;
            }
        }
    }
}