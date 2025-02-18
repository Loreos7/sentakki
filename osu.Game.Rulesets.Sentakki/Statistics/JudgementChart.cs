using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Statistics
{
    public class JudgementChart : CompositeDrawable
    {
        private const double entry_animation_duration = 150;
        private const double bar_fill_duration = 3000;
        public JudgementChart(List<HitEvent> hitEvents)
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Size = new Vector2(500, 250);
            AddRangeInternal(new Drawable[]{
                new NoteEntry
                {
                    ObjectName = "Tap",
                    HitEvents = hitEvents.Where(e=> e.HitObject is Tap x && !x.Break).ToList(),
                    Position = new Vector2(0, 0),
                    InitialLifetimeOffset = entry_animation_duration * 0
                },
                new NoteEntry
                {
                    ObjectName = "Hold",
                    HitEvents = hitEvents.Where(e => (e.HitObject is Hold or Hold.HoldHead) && !((SentakkiLanedHitObject)e.HitObject).Break).ToList(),
                    Position = new Vector2(0, .16f),
                    InitialLifetimeOffset = entry_animation_duration * 1
                },
                new NoteEntry
                {
                    ObjectName = "Slide",
                    HitEvents = hitEvents.Where(e => e.HitObject is SlideBody).ToList(),
                    Position = new Vector2(0, .32f),
                    InitialLifetimeOffset = entry_animation_duration * 2
                },
                new NoteEntry
                {
                    ObjectName = "Touch",
                    HitEvents = hitEvents.Where(e => e.HitObject is Touch).ToList(),
                    Position = new Vector2(0, .48f),
                    InitialLifetimeOffset = entry_animation_duration * 3
                },
                new NoteEntry
                {
                    ObjectName = "Touch Hold",
                    HitEvents = hitEvents.Where(e => e.HitObject is TouchHold).ToList(),
                    Position = new Vector2(0, .64f),
                    InitialLifetimeOffset = entry_animation_duration * 4
                },
                new NoteEntry
                {
                    ObjectName = "Break",
                    HitEvents = hitEvents.Where(e => e.HitObject is SentakkiLanedHitObject x && x.Break).ToList(),
                    Position = new Vector2(0, .80f),
                    InitialLifetimeOffset = entry_animation_duration * 5
                },
            });
        }
        public class NoteEntry : Container
        {
            public double InitialLifetimeOffset;
            private Container progressBox = null!;
            private RollingCounter<long> noteCounter = null!;

            public string ObjectName = "Object";
            public List<HitEvent> HitEvents = null!;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                float GoodCount = 0;
                float GreatCount = 0;
                float PerfectCount = 0;

                foreach (var e in HitEvents)
                {
                    switch (e.Result)
                    {
                        case HitResult.Great:
                            ++PerfectCount;
                            goto case HitResult.Good;
                        case HitResult.Good:
                            ++GreatCount;
                            goto case HitResult.Ok;
                        case HitResult.Ok:
                            ++GoodCount;
                            break;
                    }
                }

                Color4 textColour = !HitEvents.Any() ? Color4Extensions.FromHex("bcbcbc") : (PerfectCount == HitEvents.Count) ? Color4.White : Color4Extensions.FromHex("#3c5394");
                Color4 boxColour = !HitEvents.Any() ? Color4Extensions.FromHex("808080") : (PerfectCount == HitEvents.Count) ? Color4Extensions.FromHex("fda908") : Color4Extensions.FromHex("#DCE9F9");
                Color4 borderColour = !HitEvents.Any() ? Color4Extensions.FromHex("536277") : (PerfectCount == HitEvents.Count) ? Color4Extensions.FromHex("fda908") : Color4Extensions.FromHex("#98b8df");
                Color4 numberColour = (PerfectCount == HitEvents.Count && HitEvents.Any()) ? Color4.White : Color4Extensions.FromHex("#3c5394");

                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopCentre;
                RelativePositionAxes = Axes.Both;
                RelativeSizeAxes = Axes.Both;
                Size = new Vector2(1, .16f);
                Scale = new Vector2(1, 0);
                Alpha = 0;
                Masking = true;
                BorderThickness = 2;
                BorderColour = borderColour;
                CornerRadius = 5;
                CornerExponent = 2.5f;
                AlwaysPresent = true;

                InternalChildren = new Drawable[]{
                    new Box {
                        RelativeSizeAxes = Axes.Both,
                        Colour = boxColour,
                    },
                    new Container { // Left
                        RelativeSizeAxes = Axes.Both,
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft,
                        Size = new Vector2(.33f, 1),
                        Child = new OsuSpriteText
                        {
                            Colour = textColour,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = ObjectName.ToUpper(),
                            Font = OsuFont.Torus.With(size: 20, weight: FontWeight.Bold)
                        }
                    },
                    progressBox = new Container { // Centre
                        RelativeSizeAxes = Axes.Both,
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Size = new Vector2(.34f, .8f),
                        CornerRadius = 5,
                        CornerExponent = 2.5f,
                        Masking = true,
                        BorderThickness = 2,
                        BorderColour = Color4.Black,
                        Children = new Drawable[]{
                            new Box{
                                RelativeSizeAxes = Axes.Both,
                                Colour = !HitEvents.Any() ? Color4Extensions.FromHex("343434"):Color4.DarkGray,
                            }
                        }
                    },
                    new Container { // Right
                        RelativeSizeAxes = Axes.Both,
                        Origin = Anchor.CentreRight,
                        Anchor = Anchor.CentreRight,
                        Size = new Vector2(.33f, 1),
                        Child = noteCounter = new TotalNoteCounter
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = numberColour,
                            Current = { Value = 0 },
                        }
                    },
                };

                progressBox.AddRange(new Drawable[]{
                    new ChartBar(HitResult.Ok, GoodCount/HitEvents.Count){
                        InitialLifetimeOffset = InitialLifetimeOffset
                    },
                    new ChartBar(HitResult.Good, GreatCount/HitEvents.Count){
                        InitialLifetimeOffset = InitialLifetimeOffset
                    },
                    new ChartBar(HitResult.Great, PerfectCount/HitEvents.Count){
                        InitialLifetimeOffset = InitialLifetimeOffset
                    },
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                ScheduleAfterChildren(() =>
                {
                    using (BeginDelayedSequence(InitialLifetimeOffset, true))
                    {
                        this.ScaleTo(1, entry_animation_duration, Easing.OutBack).FadeIn();
                        noteCounter.Current.Value = HitEvents.Count;
                    }
                });
            }

            public class TotalNoteCounter : RollingCounter<long>
            {
                protected override double RollingDuration => bar_fill_duration;

                protected override Easing RollingEasing => Easing.OutPow10;

                protected override LocalisableString FormatCount(long count) => count.ToString("N0");

                protected override OsuSpriteText CreateSpriteText()
                {
                    return new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold),
                    };
                }
            }

            private class ChartBar : Container
            {
                public double InitialLifetimeOffset;

                public ChartBar(HitResult result, float progress)
                {
                    RelativeSizeAxes = Axes.Both;
                    Size = new Vector2(float.IsNaN(progress) ? 0 : progress, 1);
                    Scale = new Vector2(0, 1);
                    Add(new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = result.GetColorForSentakkiResult()
                    });
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();
                    this.Delay(InitialLifetimeOffset).ScaleTo(1, bar_fill_duration, Easing.OutPow10);
                }
            }
        }
    }
}
