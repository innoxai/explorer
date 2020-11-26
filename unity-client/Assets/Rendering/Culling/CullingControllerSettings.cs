﻿namespace DCL.Rendering
{
    /// <summary>
    /// This class contains the settings CullingController. Mainly used by the quality settings panel. 
    /// </summary>
    [System.Serializable]
    public class CullingControllerSettings
    {
        public bool enableObjectCulling = true;
        public bool enableShadowCulling = true;
        public bool enableAnimationCulling = true;

        public float enableAnimationCullingDistance = 7.5f;

        public CullingControllerProfile rendererProfile =
            new CullingControllerProfile
            {
                visibleDistanceThreshold = 30,
                shadowDistanceThreshold = 20,
                emissiveSizeThreshold = 2.5f,
                opaqueSizeThreshold = 6,
                shadowRendererSizeThreshold = 10,
                shadowMapProjectionSizeThreshold = 4
            };

        public CullingControllerProfile skinnedRendererProfile =
            new CullingControllerProfile
            {
                visibleDistanceThreshold = 50,
                shadowDistanceThreshold = 40,
                emissiveSizeThreshold = 2.5f,
                opaqueSizeThreshold = 6,
                shadowRendererSizeThreshold = 5,
                shadowMapProjectionSizeThreshold = 4,
            };

        /// <summary>
        /// Returns a clone of this object. The profiles are also cloned, making this return a deep clone.
        /// </summary>
        /// <returns>The deep clone.</returns>
        public CullingControllerSettings Clone()
        {
            var clone = this.MemberwiseClone() as CullingControllerSettings;
            clone.rendererProfile = rendererProfile.Clone();
            clone.skinnedRendererProfile = skinnedRendererProfile.Clone();
            return clone;
        }
    }
}