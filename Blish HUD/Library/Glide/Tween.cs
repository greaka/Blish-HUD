using System;
using System.Collections.Generic;

namespace Glide
{
    public partial class Tween
    {
        [Flags]
        public enum RotationUnit
        {
            Degrees,
            Radians
        }

        private MemberLerper.Behavior behavior;

        private bool firstUpdate;
        private readonly List<MemberLerper> lerpers;
        private TweenerImpl Parent;
        private readonly IRemoveTweens Remover;
        private int repeatCount, timesRepeated;
        private readonly List<object> start;
        private readonly List<object> end;
        private readonly Dictionary<string, int> varHash;

        private readonly List<MemberAccessor> vars;

        private Tween(object target, float duration, float delay, TweenerImpl parent)
        {
            this.Target = target;
            this.Duration = duration;
            this.Delay = delay;
            this.Parent = parent;
            this.Remover = parent;

            this.firstUpdate = true;

            this.varHash = new Dictionary<string, int>();
            this.vars = new List<MemberAccessor>();
            this.lerpers = new List<MemberLerper>();
            this.start = new List<object>();
            this.end = new List<object>();
            this.behavior = MemberLerper.Behavior.None;
        }

        /// <summary>
        ///     The time remaining before the tween ends or repeats.
        /// </summary>
        public float TimeRemaining => this.Duration - this.time;

        /// <summary>
        ///     A value between 0 and 1, where 0 means the tween has not been started and 1 means that it has completed.
        /// </summary>
        public float Completion
        {
            get
            {
                var c = this.time / this.Duration;
                return c < 0 ? 0 : c > 1 ? 1 : c;
            }
        }

        /// <summary>
        ///     Whether the tween is currently looping.
        /// </summary>
        public bool Looping => this.repeatCount != 0;

        /// <summary>
        ///     The object this tween targets. Will be null if the tween represents a timer.
        /// </summary>
        public object Target { get; }

        private void AddLerp(MemberLerper lerper, MemberAccessor info, object from, object to)
        {
            this.varHash.Add(info.MemberName, this.vars.Count);
            this.vars.Add(info);

            this.start.Add(from);
            this.end.Add(to);

            this.lerpers.Add(lerper);
        }

        private void Update(float elapsed)
        {
            if (this.firstUpdate)
            {
                this.firstUpdate = false;

                var i = this.vars.Count;
                while (i-- > 0)
                {
                    if (this.lerpers[i] != null) this.lerpers[i].Initialize(this.start[i], this.end[i], this.behavior);
                }
            }
            else
            {
                if (this.Paused)
                    return;

                if (this.Delay > 0)
                {
                    this.Delay -= elapsed;
                    if (this.Delay > 0)
                        return;
                }

                if ((this.time == 0) && (this.timesRepeated == 0) && (this.begin != null)) this.begin();

                this.time += elapsed;
                var setTimeTo = this.time;
                var t = this.time / this.Duration;
                var doComplete = false;

                if (this.time >= this.Duration)
                {
                    if (this.repeatCount != 0)
                    {
                        setTimeTo = 0;
                        this.Delay = this.repeatDelay;
                        this.timesRepeated++;

                        if (this.repeatCount > 0)
                            --this.repeatCount;

                        if (this.repeatCount < 0)
                            doComplete = true;
                    }
                    else
                    {
                        this.time = this.Duration;
                        t = 1;
                        this.Remover.Remove(this);
                        doComplete = true;
                    }
                }

                if (this.ease != null)
                    t = this.ease(t);

                var i = this.vars.Count;
                while (i-- > 0)
                {
                    if (this.vars[i] != null)
                        this.vars[i].Value = this.lerpers[i].Interpolate(t, this.vars[i].Value, this.behavior);
                }

                this.time = setTimeTo;

                //	If the timer is zero here, we just restarted.
                //	If reflect mode is on, flip start to end
                if ((this.time == 0) &&
                    ((this.behavior & MemberLerper.Behavior.Reflect) == MemberLerper.Behavior.Reflect))
                    Reverse();

                if (this.update != null) this.update();

                if (doComplete && (this.complete != null)) this.complete();
            }
        }

        #region Callbacks

        private Func<float, float> ease;
        private Action begin, update, complete;

        #endregion

        #region Timing

        public bool Paused { get; private set; }
        private float Delay, repeatDelay;
        private readonly float Duration;

        private float time;

        #endregion

        #region Behavior

        /// <summary>
        ///     Apply target values to a starting point before tweening.
        /// </summary>
        /// <param name="values">The values to apply, in an anonymous type ( new { prop1 = 100, prop2 = 0} ).</param>
        /// <returns>A reference to this.</returns>
        public Tween From(object values)
        {
            var props = values.GetType().GetProperties();
            for (var i = 0; i < props.Length; ++i)
            {
                var property = props[i];
                var propValue = property.GetValue(values, null);

                var index = -1;
                if (this.varHash.TryGetValue(property.Name, out index))
                {
                    //	if we're already tweening this value, adjust the range
                    this.start[index] = propValue;
                }

                //	if we aren't tweening this value, just set it
                var info = new MemberAccessor(this.Target, property.Name);
                info.Value = propValue;
            }

            return this;
        }

        /// <summary>
        ///     Set the easing function.
        /// </summary>
        /// <param name="ease">The Easer to use.</param>
        /// <returns>A reference to this.</returns>
        public Tween Ease(Func<float, float> ease)
        {
            this.ease = ease;
            return this;
        }

        /// <summary>
        ///     Set a function to call when the tween begins (useful when using delays). Can be called multiple times for compound
        ///     callbacks.
        /// </summary>
        /// <param name="callback">The function that will be called when the tween starts, after the delay.</param>
        /// <returns>A reference to this.</returns>
        public Tween OnBegin(Action callback)
        {
            if (this.begin == null)
                this.begin = callback;
            else
                this.begin += callback;
            return this;
        }

        /// <summary>
        ///     Set a function to call when the tween finishes. Can be called multiple times for compound callbacks.
        ///     If the tween repeats infinitely, this will be called each time; otherwise it will only run when the tween is
        ///     finished repeating.
        /// </summary>
        /// <param name="callback">The function that will be called on tween completion.</param>
        /// <returns>A reference to this.</returns>
        public Tween OnComplete(Action callback)
        {
            if (this.complete == null)
                this.complete = callback;
            else
                this.complete += callback;
            return this;
        }

        /// <summary>
        ///     Set a function to call as the tween updates. Can be called multiple times for compound callbacks.
        /// </summary>
        /// <param name="callback">The function to use.</param>
        /// <returns>A reference to this.</returns>
        public Tween OnUpdate(Action callback)
        {
            if (this.update == null)
                this.update = callback;
            else
                this.update += callback;
            return this;
        }

        /// <summary>
        ///     Enable repeating.
        /// </summary>
        /// <param name="times">Number of times to repeat. Leave blank or pass a negative number to repeat infinitely.</param>
        /// <returns>A reference to this.</returns>
        public Tween Repeat(int times = -1)
        {
            this.repeatCount = times;
            return this;
        }

        /// <summary>
        ///     Set a delay for when the tween repeats.
        /// </summary>
        /// <param name="delay">How long to wait before repeating.</param>
        /// <returns>A reference to this.</returns>
        public Tween RepeatDelay(float delay)
        {
            this.repeatDelay = delay;
            return this;
        }

        /// <summary>
        ///     Sets the tween to reverse every other time it repeats. Repeating must be enabled for this to have any effect.
        /// </summary>
        /// <returns>A reference to this.</returns>
        public Tween Reflect()
        {
            this.behavior |= MemberLerper.Behavior.Reflect;
            return this;
        }

        /// <summary>
        ///     Swaps the start and end values of the tween.
        /// </summary>
        /// <returns>A reference to this.</returns>
        public Tween Reverse()
        {
            var i = this.vars.Count;
            while (i-- > 0)
            {
                var s = this.start[i];
                var e = this.end[i];

                //	Set start to end and end to start
                this.start[i] = e;
                this.end[i] = s;

                this.lerpers[i].Initialize(e, s, this.behavior);
            }

            return this;
        }

        /// <summary>
        ///     Whether this tween handles rotation.
        /// </summary>
        /// <returns>A reference to this.</returns>
        public Tween Rotation(RotationUnit unit = RotationUnit.Degrees)
        {
            this.behavior |= MemberLerper.Behavior.Rotation;
            this.behavior |= unit == RotationUnit.Degrees
                ? MemberLerper.Behavior.RotationDegrees
                : MemberLerper.Behavior.RotationRadians;

            return this;
        }

        /// <summary>
        ///     Whether tweened values should be rounded to integer values.
        /// </summary>
        /// <returns>A reference to this.</returns>
        public Tween Round()
        {
            this.behavior |= MemberLerper.Behavior.Round;
            return this;
        }

        #endregion

        #region Control

        /// <summary>
        ///     Cancel tweening given properties.
        /// </summary>
        /// <param name="properties"></param>
        public void Cancel(params string[] properties)
        {
            var canceled = 0;
            for (var i = 0; i < properties.Length; ++i)
            {
                var index = 0;
                if (!this.varHash.TryGetValue(properties[i], out index))
                    continue;

                this.varHash.Remove(properties[i]);
                this.vars[index] = null;
                this.lerpers[index] = null;
                this.start[index] = null;
                this.end[index] = null;

                canceled++;
            }

            if (canceled == this.vars.Count)
                Cancel();
        }

        /// <summary>
        ///     Remove tweens from the tweener without calling their complete functions.
        /// </summary>
        public void Cancel()
        {
            this.Remover.Remove(this);
        }

        /// <summary>
        ///     Assign tweens their final value and remove them from the tweener.
        /// </summary>
        public void CancelAndComplete()
        {
            this.time = this.Duration;
            this.update = null;
            this.Remover.Remove(this);
        }

        /// <summary>
        ///     Set tweens to pause. They won't update and their delays won't tick down.
        /// </summary>
        public void Pause()
        {
            this.Paused = true;
        }

        /// <summary>
        ///     Toggle tweens' paused value.
        /// </summary>
        public void PauseToggle()
        {
            this.Paused = !this.Paused;
        }

        /// <summary>
        ///     Resumes tweens from a paused state.
        /// </summary>
        public void Resume()
        {
            this.Paused = false;
        }

        #endregion
    }
}