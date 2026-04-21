using UnityEngine;

namespace Game.UI.Styling
{
    /// <summary>
    /// Centralized UI and board colors for the current project.
    /// 
    /// ======================================
    /// UI KIT REFERENCE (HEX)
    /// ======================================
    /// Active numbers — #2C2A28
    /// Inactive (faded) numbers — #E8EDF1
    /// New numbers (blue) — #2F86B7
    /// Cell borders — #E2E2E2
    /// Background — #FAFEFF
    /// Buttons (+ / hint) — #3A8DDE
    /// Gold (base) — #F5C84C
    /// Gold (highlight) — #FFE08A
    /// Gold (shadow) — #D4A937
    ///
    /// ======================================
    /// UI KIT THIS PROJECT (HEX)
    /// ======================================
    ///
    /// 
    /// ======================================
    /// </summary>
    public static class GamePalette
    {
        public static readonly Color GameBackground = new Color(0.97f, 0.95f, 0.92f, 1f);
        public static readonly Color BoardTileNormalBackground = new Color(1.00f, 1.00f, 1.00f, 1.0f);
        public static readonly Color BoardTileSelectedBackground = new Color(1.00f, 0.96f, 0.82f, 1.0f);
        public static readonly Color BoardTileClearedBackground = new Color(1.00f, 1.00f, 1.00f, 1.0f);
        public static readonly Color BoardTileHintBackground = new Color(0.98f, 0.88f, 0.82f, 1.0f);
        public static readonly Color BoardTileHintPulseBackground = new Color(0.86f, 0.97f, 0.84f, 1f);
        public static readonly Color BoardTileText = new Color(0.1814f, 0.1668f, 0.1623f, 1f);
        public static readonly Color GridLineColor = new Color(0.8856f, 0.8798f, 0.8785f, 1f);
        public static readonly Color InactiveNumberColor = new Color(0.9061f, 0.9062f, 0.9180f, 1f);

        public static readonly Color HudPanelBackground = new Color(0.16f, 0.18f, 0.24f, 0.96f);
        public static readonly Color BoardScrollBackground = new Color(0.97f, 0.95f, 0.92f, 1f);
        public static readonly Color PrimaryButton = new Color(0.28f, 0.56f, 0.92f, 1f);
        public static readonly Color LockedButton = new Color(0.28f, 0.44f, 0.60f, 0.95f);
        public static readonly Color DisabledButton = new Color(0.30f, 0.32f, 0.38f, 0.9f);
        public static readonly Color TooltipText = new Color(1f, 0.93f, 0.70f, 1f);
        public static readonly Color PrimaryText = Color.white;
        public static readonly Color SecondaryText = new Color(0.86f, 0.90f, 0.96f, 1f);
        public static readonly Color DisabledText = new Color(1f, 1f, 1f, 0.65f);
        public static readonly Color LockedText = new Color(1f, 1f, 1f, 0.82f);
        public static readonly Color DeveloperPairLine = new Color(1f, 0.42f, 0.18f, 0.55f);

        public static readonly Color GameOverOverlay = new Color(0.05f, 0.06f, 0.09f, 0.84f);

        public static readonly Color DeveloperPanelBackground = new Color(0.14f, 0.17f, 0.22f, 0.96f);
        public static readonly Color DeveloperPanelButton = new Color(0.82f, 0.40f, 0.26f, 1f);
        public static readonly Color DeveloperPanelInfo = new Color(0.88f, 0.91f, 0.96f, 1f);
    }
}
