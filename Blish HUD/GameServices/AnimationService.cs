using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Library.Glide.CustomLerpers;
using Glide;
using Microsoft.Xna.Framework;

namespace Blish_HUD
{
    public class EaseAnimation : IDisposable
    {
        private readonly AnimationService.EasingFunctionDelegate AnimationFunction;
        private double ChangeInValue;
        private double Duration;

        private bool Repeat;

        private double StartTime;

        private double StartValue;

        public EaseAnimation(AnimationService.EasingFunctionDelegate animFunc, double startValue, double changeInValue,
            double duration)
        {
            this.StartTime = DateTime.Now.TimeOfDay.TotalMilliseconds;

            this.CurrentValue = startValue;
            this.AnimationFunction = animFunc;
            this.StartValue = startValue;
            this.ChangeInValue = changeInValue;
            this.Duration = duration;
        }

        public bool Active { get; private set; }

        public double CurrentValue { get; private set; }
        public int CurrentValueInt => (int) Math.Round(this.CurrentValue);

        public bool Done { get; private set; }

        public void Dispose()
        {
            GameService.Animation.RemoveAnim(this);
        }

        public event EventHandler<EventArgs> AnimationCompleted;

        public void Start(bool repeat = false)
        {
            this.Repeat = repeat;
            this.StartTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            this.CurrentValue = this.StartValue;
            this.Done = false;
            this.Active = true;
        }

        public void Stop()
        {
            this.Active = false;
        }

        public void Reverse()
        {
            var tempStartVal = this.StartValue;
            this.Duration = this.Duration * this.CurrentValue / this.ChangeInValue;
            this.StartValue = this.CurrentValue;
            this.ChangeInValue = -this.CurrentValue;

            Start();
        }

        public void Update(GameTime gameTime)
        {
            // Ensure everything is able to get the last value from the tween before it stops updating
            if (this.Done) this.Active = false;

            var currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds - this.StartTime;

            if (currentTime > this.Duration)
            {
                currentTime = this.Duration;
                this.Done = true;

                AnimationCompleted?.Invoke(this, null);
            }

            this.CurrentValue =
                this.AnimationFunction.Invoke(currentTime, this.StartValue, this.ChangeInValue, this.Duration);

            if (this.Done && this.Repeat) Start(this.Repeat);
        }
    }

    public class AnimationService : GameService
    {
        public delegate double EasingFunctionDelegate(double currentTime, double startValue, double changeInValue,
            double duration);

        public enum EasingMethod
        {
            Linear,
            EaseInQuad,
            EaseOutQuad,
            EaseInOutQuad,
            EaseInCubic,
            EaseOutCubic,
            EaseInOutCubic,
            EaseInQuart,
            EaseOutQuart,
            EaseInOutQuart,
            EaseInQuint,
            EaseOutQuint,
            EaseInOutQuint,
            EaseInSine,
            EaseOutSine,
            EaseInOutSine,
            EaseInExpo,
            EaseOutExpo,
            EaseInOutExpo,
            EaseInCirc,
            EaseOutCirc,
            EaseInOutCirc
        }

        private List<EaseAnimation> CurrentAnimations;

        private Dictionary<EasingMethod, EasingFunctionDelegate> EaseFuncs;
        public Tweener Tweener { get; private set; }

        protected override void Initialize()
        {
            this.CurrentAnimations = new List<EaseAnimation>();
            this.EaseFuncs = new Dictionary<EasingMethod, EasingFunctionDelegate>();

            Glide.Tween.TweenerImpl.SetLerper<PointLerper>(typeof(Point));

            this.Tweener = new Tweener();

            this.EaseFuncs.Add(EasingMethod.Linear, CalcLinear);
            this.EaseFuncs.Add(EasingMethod.EaseInExpo, CalcExponentialEasingIn);
            this.EaseFuncs.Add(EasingMethod.EaseInOutQuad, CalcQuadraticEasingInOut);
        }

        public EaseAnimation Tween(double startValue, double changeInValue, double duration, EasingMethod method)
        {
            var nanim = new EaseAnimation(this.EaseFuncs[method], startValue, changeInValue, duration);
            this.CurrentAnimations.Add(nanim);
            return nanim;
        }

        public void RemoveAnim(EaseAnimation anim)
        {
            this.CurrentAnimations.Remove(anim);
        }

        protected override void Update(GameTime gameTime)
        {
            this.CurrentAnimations.Where(a => a.Active).ToList().ForEach(a => a.Update(gameTime));

            this.Tweener.Update((float) gameTime.ElapsedGameTime.TotalSeconds);
        }

        // Easing functions:

        private double CalcLinear(double currentTime, double startValue, double changeInValue, double duration)
        {
            return changeInValue * currentTime / duration + startValue;
        }

        private double CalcExponentialEasingIn(double currentTime, double startValue, double changeInValue,
            double duration)
        {
            return changeInValue * Math.Pow(2, 10 * (currentTime / duration - 1)) + startValue;
        }

        private double CalcQuadraticEasingInOut(double currentTime, double startValue, double changeInValue,
            double duration)
        {
            currentTime /= duration / 2;
            if (currentTime < 1) return changeInValue / 2 * currentTime * currentTime + startValue;
            currentTime--;
            return -changeInValue / 2 * (currentTime * (currentTime - 2) - 1) + startValue;
        }

        protected override void Load()
        {
            // TODO: Set up animation service
        }

        protected override void Unload()
        {
            // TODO: Clean up animation service
        }
    }
}