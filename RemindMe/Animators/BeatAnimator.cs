using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace RemindMe.Animators
{
    public class BeatAnimator
    {
        private readonly TextView elapsedTimeTxt;
        private AnimatorSet beatAnimator;

        public BeatAnimator(TextView textView)
        {
            this.elapsedTimeTxt = textView;
        }

        public void Build()
        {
            beatAnimator = AnimateBeat();
        }

        public void Play()
        {
            beatAnimator.Start();
        }

        private AnimatorSet AnimateBeat()
        {
            var scaleOutX = ObjectAnimator.OfFloat(elapsedTimeTxt, "scaleX", 1.05f);
            var scaleOutY = ObjectAnimator.OfFloat(elapsedTimeTxt, "scaleY", 1.05f);

            var beatOut = new AnimatorSet();
            beatOut.PlayTogether(scaleOutX, scaleOutY);
            beatOut.SetDuration(128);

            var restoreX = ObjectAnimator.OfFloat(elapsedTimeTxt, "scaleX", 1f);
            var restoreY = ObjectAnimator.OfFloat(elapsedTimeTxt, "scaleY", 1f);

            var restore = new AnimatorSet();
            restore.PlayTogether(restoreX, restoreY);
            restore.SetDuration(128);

            var beatAnimator = new AnimatorSet();
            beatAnimator.Play(beatOut).Before(restore);
            beatAnimator.SetInterpolator(new Android.Views.Animations.LinearInterpolator());
            return beatAnimator;
        }
    }
}