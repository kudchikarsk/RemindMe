using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace RemindMe.Animators
{
    public class WaterAnimator
    {
        private readonly ImageView water;
        private readonly ImageView wave1;
        private readonly ImageView wave2;
        private readonly ImageView wave3;
        private readonly ImageView wave4;
        private readonly Resources resources;
        private AnimatorSet wavesAnimator;
        private AnimatorSet waterAnimator;
        private AnimatorSet waterAnimatorReverse;
        private AnimatorSet waterTranslateSet;
        private AnimatorSet waterScaleSet;

        public event EventHandler OnEnd = delegate {};

        public WaterAnimator(
            ImageView water, 
            ImageView wave1, 
            ImageView wave2, 
            ImageView wave3, 
            ImageView wave4,
            Resources resources)
        {
            this.water = water;
            this.wave1 = wave1;
            this.wave2 = wave2;
            this.wave3 = wave3;
            this.wave4 = wave4;
            this.resources = resources;
        }

        public void Build()
        {
            BuildWaves();
            BuildWaterTranslate();
            wavesAnimator.Start();
        }

        public void Play(long duration, long playFrom = 0)
        {

            waterAnimator.SetDuration(duration);           
            waterAnimator.Start();

            foreach (var anim in waterTranslateSet.ChildAnimations)
            {
                (anim as ValueAnimator).CurrentPlayTime = playFrom;
            }

            foreach (var anim in waterScaleSet.ChildAnimations)
            {
                (anim as ValueAnimator).CurrentPlayTime = playFrom;
            }
        }

        public void Reset()
        {
            waterAnimator.Cancel();

            var resetDuration = 256;

            waterAnimatorReverse = AnimateWaterReverse(resources);
            waterAnimatorReverse.SetDuration(resetDuration);
            waterAnimatorReverse.Start();

            waterAnimator.StartDelay = resetDuration;

        }

        public void Pause()
        {
            waterAnimator?.Pause();
            wavesAnimator?.Pause();
        }

        public void Resume()
        {
            waterAnimator?.Resume();
            wavesAnimator?.Resume();
        }

        public void SetDuration(long duration)
        {
            waterAnimator.SetDuration(duration);
        }

        private void BuildWaves()
        {
            //build waves animator
            var waveSet1 = AnimateWaves(wave1, wave2, reverse: true);
            var waveSet2 = AnimateWaves(wave3, wave4);
            wavesAnimator = new AnimatorSet();
            wavesAnimator.PlayTogether(waveSet1, waveSet2);
        }

        private void BuildWaterTranslate()
        {
            waterAnimator = AnimateWater();
            waterAnimator.AnimationEnd += OnEnd;
        }

        public void Cancel()
        {
            waterAnimator.Cancel();
        }

        public void End()
        {
            waterAnimator.End();
        }

        private AnimatorSet AnimateWaterReverse(Resources resources)
        {
            var waterTranslateY = ObjectAnimator.OfFloat(water, "translationY", resources.GetDimension(Resource.Dimension.water_translation_y));
            var wave1TranslateY = ObjectAnimator.OfFloat(wave1, "translationY", resources.GetDimension(Resource.Dimension.wave_translation_y));
            var wave2TranslateY = ObjectAnimator.OfFloat(wave2, "translationY", resources.GetDimension(Resource.Dimension.wave_translation_y));
            var wave3TranslateY = ObjectAnimator.OfFloat(wave3, "translationY", resources.GetDimension(Resource.Dimension.wave_translation_y));
            var wave4TranslateY = ObjectAnimator.OfFloat(wave4, "translationY", resources.GetDimension(Resource.Dimension.wave_translation_y));
            
            var waterTranslateSet = new AnimatorSet();
            waterTranslateSet.SetInterpolator(new Android.Views.Animations.LinearInterpolator());
            waterTranslateSet.PlayTogether(
                    waterTranslateY,
                    wave1TranslateY,
                    wave2TranslateY,
                    wave3TranslateY,
                    wave4TranslateY);
            

            return waterTranslateSet;
        }

        private AnimatorSet AnimateWater()
        {
            var waterTranslateY = Animate(water, "translationY", water.Height);        
            var wave1TranslateY = Animate(wave1, "translationY", water.Height - wave1.Height);
            var wave2TranslateY = Animate(wave2, "translationY", water.Height - wave2.Height);
            var wave3TranslateY = Animate(wave3, "translationY", water.Height - wave3.Height);
            var wave4TranslateY = Animate(wave4, "translationY", water.Height - wave4.Height);

            waterTranslateSet = new AnimatorSet();
            waterTranslateSet.SetInterpolator(new Android.Views.Animations.LinearInterpolator());
            waterTranslateSet.PlayTogether(
                    waterTranslateY,
                    wave1TranslateY,
                    wave2TranslateY,
                    wave3TranslateY,
                    wave4TranslateY);


            wave1.PivotY = wave1.Height;
            wave2.PivotY = wave2.Height;
            wave3.PivotY = wave3.Height;
            wave4.PivotY = wave4.Height;

            var wave1ScaleY = Animate(wave1, "scaleY", 0);
            var wave2ScaleY = Animate(wave2, "scaleY", 0);
            var wave3ScaleY = Animate(wave3, "scaleY", 0);
            var wave4ScaleY = Animate(wave4, "scaleY", 0);
            waterScaleSet = new AnimatorSet();
            waterScaleSet.SetInterpolator(new Android.Views.Animations.LinearInterpolator());
            waterScaleSet.PlayTogether(
                    wave1ScaleY,
                    wave2ScaleY,
                    wave3ScaleY,
                    wave4ScaleY
                );



            var waterAnimatorSet = new AnimatorSet();
            waterAnimatorSet.PlayTogether(waterTranslateSet, waterScaleSet);

            return waterAnimatorSet;
        }

        private static AnimatorSet AnimateWaves(ImageView wave1, ImageView wave2, bool reverse = false)
        {

            var wave1X = 0;
            var wave1TranslateTo = wave1.Width;

            var wave2X = -wave2.Width;
            var wave2TranslateTo = 0;

            if (reverse)
            {
                wave1X = wave2.Width;
                wave1TranslateTo = 0;

                wave2X = 0;
                wave2TranslateTo = -wave2.Width;
            }

            wave1.TranslationX = wave1X;
            wave2.TranslationX = wave2X;

            var wave1TranslateX = AnimateRepeat(wave1, "translationX", wave1TranslateTo);
            var wave2TranslateX = AnimateRepeat(wave2, "translationX", wave2TranslateTo);

            var waveSet = new AnimatorSet();
            waveSet.SetDuration(1000);
            waveSet.SetInterpolator(new Android.Views.Animations.LinearInterpolator());
            waveSet.Play(wave1TranslateX).With(wave2TranslateX);

            return waveSet;
        }

        private static ObjectAnimator AnimateRepeat(View view, string property, int value)
        {
            var anim = Animate(view, property, value);
            anim.RepeatMode = ValueAnimatorRepeatMode.Restart;
            anim.RepeatCount = -1;
            return anim;
        }

        private static ObjectAnimator Animate(View view, string property, float value)
        {
            var anim = ObjectAnimator.OfFloat(view, property, value);
            
            return anim;
        }
    }
}