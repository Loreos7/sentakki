using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects
{
    [TestFixture]
    public class TestSceneSlideFan : OsuTestScene
    {
        private readonly Container content;
        protected override Container<Drawable> Content => content;

        protected override Ruleset CreateRuleset() => new SentakkiRuleset();

        private int depthIndex;

        [Cached]
        private readonly SlideFanChevrons fanChevrons;

        public TestSceneSlideFan()
        {
            base.Content.Add(content = new SentakkiInputManager(new SentakkiRuleset().RulesetInfo));
            Add(new SentakkiRing()
            {
                RelativeSizeAxes = Axes.None,
                Size = new Vector2(SentakkiPlayfield.RINGSIZE),
                Rotation = -22.5f
            });

            Add(fanChevrons = new SlideFanChevrons());

            AddStep("Miss Single", () => testSingle(2000));
            AddStep("Hit Single", () => testSingle(2000, true));
            AddUntilStep("Wait for object despawn", () => !Children.Any(h => (h is DrawableSentakkiHitObject hitObject) && hitObject.AllJudged == false));
        }

        private void testSingle(double duration, bool auto = false)
        {
            var slide = new Slide
            {
                //Break = true,
                SlideInfoList = new List<SlideBodyInfo>
                {
                    new SlideBodyInfo {
                        SlidePathParts = new[] {new SlideBodyPart(SlidePaths.PathShapes.Fan, 4, false)},
                        Duration = 1000,
                    },
                },
                StartTime = Time.Current + 1000,
            };

            slide.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { });

            DrawableSlide dSlide;

            Add(dSlide = new DrawableSlide(slide)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Depth = depthIndex++,
                Auto = auto
            });

            foreach (DrawableSentakkiHitObject nested in dSlide.NestedHitObjects)
                foreach (DrawableSentakkiHitObject nested2 in nested.NestedHitObjects)
                    nested2.Auto = auto;
        }
    }
}
