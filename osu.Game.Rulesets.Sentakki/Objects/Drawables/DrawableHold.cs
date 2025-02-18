﻿using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableHold : DrawableSentakkiLanedHitObject, IKeyBindingHandler<SentakkiAction>
    {
        public new Hold HitObject => (Hold)base.HitObject;
        public DrawableHoldHead Head => headContainer.Child;

        private Container<DrawableHoldHead> headContainer = null!;

        public HoldBody NoteBody = null!;

        public override double LifetimeStart
        {
            get => base.LifetimeStart;
            set
            {
                base.LifetimeStart = value;
                NoteBody.LifetimeStart = value;
            }
        }
        public override double LifetimeEnd
        {
            get => base.LifetimeEnd;
            set
            {
                base.LifetimeEnd = value;
                NoteBody.LifetimeEnd = value;
            }
        }

        public DrawableHold() : this(null) { }

        public DrawableHold(Hold? hitObject = null)
            : base(hitObject) { }

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            AddRangeInternal(new Drawable[]{
                NoteBody = new HoldBody(),
                headContainer = new Container<DrawableHoldHead> { RelativeSizeAxes = Axes.Both },
            });
        }

        protected override void OnFree()
        {
            base.OnFree();
            HoldStartTime = null;
            TotalHoldTime = 0;
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            double animTime = AdjustedAnimationDuration / 2;
            NoteBody.FadeInFromZero(animTime).ScaleTo(1, animTime);

            NoteBody.FadeColour(AccentColour.Value);

            using (BeginDelayedSequence(animTime, true))
            {
                // This is the movable length (not including start position)
                float totalMovableDistance = SentakkiPlayfield.INTERSECTDISTANCE - SentakkiPlayfield.NOTESTARTDISTANCE;
                float originalStretchAmount = (float)(totalMovableDistance / animTime * (HitObject as IHasDuration).Duration);
                float stretchAmount = Math.Clamp((float)(totalMovableDistance / animTime * (HitObject as IHasDuration).Duration), 0, totalMovableDistance);
                float stretchTime = (float)(stretchAmount / totalMovableDistance * animTime);
                float excessDistance = (float)((-SentakkiPlayfield.INTERSECTDISTANCE + SentakkiPlayfield.NOTESTARTDISTANCE) / animTime);

                NoteBody.ResizeHeightTo(stretchAmount, stretchTime)
                        .Delay((HitObject as IHasDuration).Duration)
                        .MoveToY(-SentakkiPlayfield.INTERSECTDISTANCE, animTime)
                        .Delay(animTime - stretchTime)
                        .ResizeHeightTo(0, stretchTime);

                if (HoldStartTime == null && !Auto)
                    NoteBody.Delay(animTime).FadeColour(Color4.Gray, 100);
            }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Time.Current > HitObject.GetEndTime())
            {
                endHold();
                double totalHoldRatio = TotalHoldTime / ((IHasDuration)HitObject).Duration;
                HitResult result;

                if (totalHoldRatio >= .75 || Auto)
                    result = HitResult.Great;
                else if (totalHoldRatio >= .5)
                    result = HitResult.Good;
                else if (totalHoldRatio >= .25)
                    result = HitResult.Ok;
                else
                    result = HitResult.Miss;

                // Hold is over, but head windows are still active.
                // Only happens on super short holds
                // Force a miss on the head in this case
                if (!headContainer[0].Result.HasResult)
                    headContainer[0].MissForcefully();

                ApplyResult(result);
            }
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);
            const double time_fade_miss = 400;

            switch (state)
            {
                case ArmedState.Hit:
                    Expire();
                    break;

                case ArmedState.Miss:
                    NoteBody.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                        .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                        .MoveToOffset(new Vector2(0, -100), time_fade_miss, Easing.OutCubic)
                        .FadeOut(time_fade_miss);

                    using (BeginDelayedSequence(time_fade_miss, true))
                        this.FadeOut();
                    break;
            }
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Hold.HoldHead head:
                    return new DrawableHoldHead(head)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        AutoBindable = { BindTarget = AutoBindable }
                    };
            }
            return base.CreateNestedHitObject(hitObject);
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);
            switch (hitObject)
            {
                case DrawableHoldHead head:
                    headContainer.Child = head;
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            headContainer.Clear(false);
        }

        /// <summary>
        /// Time at which the user started holding this hold note. Null if the user is not holding this hold note.
        /// </summary>
        public double? HoldStartTime { get; private set; }
        public double TotalHoldTime;

        private bool beginHoldAt(double timeOffset)
        {
            if (timeOffset < -Head.HitObject.HitWindows.WindowFor(HitResult.Miss))
                return false;

            HoldStartTime = Math.Max(Time.Current, HitObject.StartTime);
            return true;
        }

        private void endHold()
        {
            if (HoldStartTime.HasValue)
                TotalHoldTime += Math.Max(Time.Current - HoldStartTime.Value, 0);

            HoldStartTime = null;
        }

        public bool OnPressed(KeyBindingPressEvent<SentakkiAction> e)
        {
            if (AllJudged)
                return false;

            if (e.Action != SentakkiAction.Key1 + HitObject.Lane)
                return false;

            if (beginHoldAt(Time.Current - Head.HitObject.StartTime))
            {
                Head.UpdateResult();
                NoteBody.FadeColour(AccentColour.Value, 50);
            }

            return true;
        }

        public void OnReleased(KeyBindingReleaseEvent<SentakkiAction> e)
        {
            if (AllJudged) return;
            if (HoldStartTime is null) return;

            if (e.Action != SentakkiAction.Key1 + HitObject.Lane)
                return;

            endHold();

            if (!AllJudged)
                NoteBody.FadeColour(Color4.Gray, 100);
        }
    }
}
