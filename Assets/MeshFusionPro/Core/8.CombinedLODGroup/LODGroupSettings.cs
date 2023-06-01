using UnityEngine;

namespace NGS.MeshFusionPro
{
    public struct LODGroupSettings
    {
        public float size;
        public int lodCount;
        public LODFadeMode fadeMode;
        public bool animateCrossFading;
        public float[] screenTransitionsHeight;
        public float[] fadeTransitionsWidth;

        public LODGroupSettings(LODGroup group)
        {
            size = group.size;
            lodCount = group.lodCount;
            fadeMode = group.fadeMode;
            animateCrossFading = group.animateCrossFading;

            screenTransitionsHeight = new float[lodCount];
            fadeTransitionsWidth = new float[lodCount];

            LOD[] lods = group.GetLODs();

            for (int i = 0; i < lodCount; i++)
            {
                LOD lod = lods[i];
                screenTransitionsHeight[i] = lod.screenRelativeTransitionHeight;
                fadeTransitionsWidth[i] = lod.fadeTransitionWidth;
            }
        }

        public bool IsEqual(LODGroupSettings settings,
            float screenHeightThreshold = 0.0001f,
            float fadeWidthThreshold = 0.0001f)
        {
            if (lodCount != settings.lodCount)
                return false;

            if (fadeMode != settings.fadeMode)
                return false;

            if (animateCrossFading != settings.animateCrossFading)
                return false;

            for (int i = 0; i < lodCount; i++)
            {
                float screenDiff = Mathf.Abs(screenTransitionsHeight[i] - settings.screenTransitionsHeight[i]);

                if (screenDiff > screenHeightThreshold)
                    return false;

                float fadeDiff = Mathf.Abs(fadeTransitionsWidth[i] - settings.fadeTransitionsWidth[i]);

                if (fadeDiff > fadeWidthThreshold)
                    return false;
            }

            return true;
        }
    }
}
