using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;
using System.Diagnostics;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableTouch : DrawableSentakkiHitObject
    {
        // IsHovered is used
        public override bool HandlePositionalInput => true;

        protected override float SamplePlaybackPosition => (HitObject.Position.X + SentakkiPlayfield.INTERSECTDISTANCE) / (SentakkiPlayfield.INTERSECTDISTANCE * 2);

        protected override double InitialLifetimeOffset => 2000;

        private readonly CircularContainer circle1;
        private readonly CircularContainer circle2;
        private readonly CircularContainer circle3;
        private readonly CircularContainer circle4;

        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

        public DrawableTouch(SentakkiHitObject hitObject) : base(hitObject)
        {
            Size = new Vector2(240);
            Position = hitObject.Position;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Alpha = 0;
            Scale = Vector2.Zero;
            AlwaysPresent = true;
            AddRangeInternal(new Drawable[]{
                circle1 = new CircularContainer{
                    Masking = true,
                    Position = new Vector2(40, 0),
                    Size = new Vector2(40),
                    BorderColour = Color4.Red,
                    Origin = Anchor.Centre,
                    Anchor =Anchor.Centre,
                    BorderThickness = 3,
                    Child = new Box{
                        RelativeSizeAxes = Axes.Both,
                        Alpha= .2f,
                        AlwaysPresent = true,
                        Colour = Color4.Red,
                    }
                },
                circle2 = new CircularContainer{
                    Masking = true,
                    Position = new Vector2(-40, 0),
                    Size = new Vector2(40),
                    BorderColour = Color4.Red,
                    Origin = Anchor.Centre,
                    Anchor =Anchor.Centre,
                    BorderThickness = 3,
                    Child = new Box{
                        RelativeSizeAxes = Axes.Both,
                        Alpha= .2f,
                        AlwaysPresent = true,
                        Colour = Color4.Red,
                    }
                },
                circle3 = new CircularContainer{
                    Masking = true,
                    Position = new Vector2(0, 40),
                    Size = new Vector2(40),
                    BorderColour = Color4.Red,
                    Origin = Anchor.Centre,
                    Anchor =Anchor.Centre,
                    BorderThickness = 3,
                    Child = new Box{
                        RelativeSizeAxes = Axes.Both,
                        Alpha= .2f,
                        AlwaysPresent = true,
                        Colour = Color4.Red,
                    }
                },
                circle4 = new CircularContainer{
                    Masking = true,
                    Position = new Vector2(0, -40),
                    Size = new Vector2(40),
                    BorderColour = Color4.Red,
                    Origin = Anchor.Centre,
                    Anchor =Anchor.Centre,
                    BorderThickness = 3,
                    Child = new Box{
                        RelativeSizeAxes = Axes.Both,
                        Alpha= .2f,
                        AlwaysPresent = true,
                        Colour = Color4.Red,
                    }
                },
                new HitReceptor{
                    Hit = () =>
                    {
                        if (AllJudged)
                            return false;

                        UpdateResult(true);
                        return false;
                    },
                }
            });
        }

        // Easing functions for manual use.
        private readonly DefaultEasingFunction inOutBack = new DefaultEasingFunction(Easing.InOutBack);
        private readonly DefaultEasingFunction inQuint = new DefaultEasingFunction(Easing.InQuint);

        protected override void Update()
        {
            base.Update();
            if (Result.HasResult) return;

            double fadeIn = 500 * GameplaySpeed;
            double moveTo = 500 * GameplaySpeed;
            double animStart = HitObject.StartTime - fadeIn - moveTo;
            double currentProg = Clock.CurrentTime - animStart;

            // Calculate initial entry animation
            float fadeAmount = (float)(currentProg / fadeIn);
            if (fadeAmount < 0) fadeAmount = 0;
            else if (fadeAmount > 1) fadeAmount = 1;

            Alpha = fadeAmount * (float)inOutBack.ApplyEasing(fadeAmount);
            Scale = new Vector2(1f * fadeAmount * (float)inOutBack.ApplyEasing(fadeAmount));

            // Calculate position
            float moveAmount = (float)((currentProg - fadeIn) / moveTo);
            if (moveAmount < 0) moveAmount = 0;
            else if (moveAmount > 1) moveAmount = 1;

            // Used to simplify this crazy arse manual animating
            float moveAnimFormula(float originalValue) => (float)(originalValue - (originalValue * moveAmount * inQuint.ApplyEasing(moveAmount)));

            circle1.Position = new Vector2(moveAnimFormula(40), 0);
            circle2.Position = new Vector2(moveAnimFormula(-40), 0);
            circle3.Position = new Vector2(0, moveAnimFormula(40));
            circle4.Position = new Vector2(0, moveAnimFormula(-40));

            // Used to simplify this crazy arse manual animating
            float finalSize = 80;
            float sizeAnimFormula(float originalValue) => (float)(originalValue + (finalSize - originalValue) * moveAmount * inQuint.ApplyEasing(moveAmount));

            circle1.Size = new Vector2(sizeAnimFormula(40));
            circle2.Size = new Vector2(sizeAnimFormula(40));
            circle3.Size = new Vector2(sizeAnimFormula(40));
            circle4.Size = new Vector2(sizeAnimFormula(40));
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered)
            {
                if (Auto && timeOffset > 0)
                    ApplyResult(r => r.Type = HitResult.Perfect);

                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = HitResult.Miss);

                return;
            }
            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (timeOffset < 0 && result <= HitResult.Miss)
                return;
            if (result >= HitResult.Meh && timeOffset < 0)
                result = HitResult.Perfect;

            ApplyResult(r => r.Type = result);
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);

            switch (state)
            {
                case ArmedState.Hit:
                    this.ScaleTo(1.5f, 200).FadeOut(200).Then().Expire();

                    break;

                case ArmedState.Miss:
                    this.ScaleTo(0, 400).FadeOut(400).Then().Expire();

                    break;
            }
        }
    }
}