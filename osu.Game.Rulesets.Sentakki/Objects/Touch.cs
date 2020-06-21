using osu.Game.Rulesets.Sentakki.Scoring;
using osu.Game.Rulesets.Scoring;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Touch : SentakkiHitObject
    {
        public override bool IsBreak => false;
        public override Color4 NoteColor => HasTwin ? Color4.Gold : Color4.Cyan;

        public override float Angle => 0;

        // This is not actually used during the result check, since all valid hits result in a perfect judgement
        // The only reason that it's here is so that hits show on the accuracy meter at the side.
        protected override HitWindows CreateHitWindows() => new SentakkiHitWindows();
    }
}