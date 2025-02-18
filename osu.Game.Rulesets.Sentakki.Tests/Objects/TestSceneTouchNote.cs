﻿using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects
{
    [TestFixture]
    public class TestSceneTouchNote : OsuTestScene
    {
        private readonly Container content;
        protected override Container<Drawable> Content => content;

        private int depthIndex;

        public TestSceneTouchNote()
        {
            base.Content.Add(content = new SentakkiInputManager(new SentakkiRuleset().RulesetInfo));
            base.Content.Add(new SentakkiRing()
            {
                RelativeSizeAxes = Axes.None,
                Size = new Vector2(SentakkiPlayfield.RINGSIZE)
            });

            AddStep("Miss Single", () => testAllPositions());
            AddStep("Hit Single", () => testAllPositions(true));
            AddUntilStep("Wait for object despawn", () => !Children.Any(h => (h is DrawableSentakkiHitObject sentakkiHitObject) && sentakkiHitObject.AllJudged == false));
        }

        private void testAllPositions(bool auto = false)
        {
            foreach (var position in SentakkiBeatmapConverter.VALID_TOUCH_POSITIONS)
            {
                var circle = new Touch
                {
                    StartTime = Time.Current + 1000,
                    Position = position,
                };

                circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { });

                Add(new DrawableTouch(circle)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Depth = depthIndex++,
                    Auto = auto
                });
            }
        }

        protected override Ruleset CreateRuleset() => new SentakkiRuleset();
    }
}
