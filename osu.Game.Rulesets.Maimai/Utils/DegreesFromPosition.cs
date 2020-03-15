﻿using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Maimai.UI;
using osuTK;

namespace osu.Game.Rulesets.Maimai
{
    public static class Utils
    {
        public static float GetDegreesFromPosition(this Vector2 target, Vector2 self)
        {
            float degrees = (float)MathHelper.RadiansToDegrees(Math.Atan2(target.X - self.X, target.Y - self.Y));

            return degrees;
        }
        public static float/*<int, Nullable<int>> */GetNotePathFromDegrees(float degrees)
        {
            if (degrees < 0) degrees += 360;
            float SingleThreshold = 40f; // 40 Degrees margin from centre
            int result = 0;

            for (int i = 0; i < MaimaiPlayfield.pathAngles.Length; ++i)
            {
                if (MaimaiPlayfield.pathAngles[i] - degrees >= -22.5f && MaimaiPlayfield.pathAngles[i] - degrees <= 22.5f)
                    result = i;

                //if (pathAngles[i] - degrees >= -45 && pathAngles[i] - degrees <= -SingleThreshold)
                //    return new Tuple<int, Nullable<int>>((i == 0 ? 7 : i), i);
                //else if (pathAngles[i] - degrees > -SingleThreshold && pathAngles[i] - degrees <= SingleThreshold)
                //    return new Tuple<int, Nullable<int>>(i, null);
                //else if (pathAngles[i] - degrees <= 45 && pathAngles[i] - degrees >= SingleThreshold)
                //    return new Tuple<int, Nullable<int>>(i, (i == 7 ? 0 : i));

            }
            return MaimaiPlayfield.pathAngles[result];
        }
    }
}