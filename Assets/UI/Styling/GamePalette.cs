using UnityEngine;

namespace Game.UI.Styling
{
    /// <summary>
    /// Centralized UI and board colors for the current Rewrite project.
    /// Keeping the palette in one place makes visual tuning easier without
    /// introducing a larger theming system yet.
    /// 
    /// ======================================
    /// COZY UI KIT (RGBA 0–1)
    /// ======================================
    /// BASE
    /// Background — RGBA(0.97, 0.95, 0.92, 1.0)
    /// Surface / Cells — RGBA(1.00, 1.00, 1.00, 1.0)
    /// Secondary Surface — RGBA(0.93, 0.91, 0.88, 1.0)
    /// GRID
    /// Grid Lines — RGBA(0.88, 0.85, 0.80, 1.0)
    /// Cell Border Active — RGBA(0.82, 0.78, 0.72, 1.0)
    /// NUMBERS
    /// Primary Numbers — RGBA(0.18, 0.17, 0.16, 1.0)
    /// New Numbers — RGBA(0.35, 0.52, 0.78, 1.0)
    /// Disabled Numbers — RGBA(0.65, 0.63, 0.60, 1.0)
    /// STATES
    /// Cell Highlight — RGBA(1.00, 0.94, 0.78, 1.0)
    /// Cell Hover / Press — RGBA(0.98, 0.90, 0.70, 1.0)
    /// Match Success — RGBA(0.86, 0.95, 0.88, 1.0)
    /// Hint — RGBA(0.98, 0.88, 0.82, 1.0)
    /// UI ELEMENTS
    /// Top Bar Background — RGBA(0.95, 0.93, 0.90, 1.0)
    /// Buttons Primary — RGBA(0.35, 0.52, 0.78, 1.0)
    /// Buttons Secondary — RGBA(0.85, 0.82, 0.78, 1.0)
    /// Button Text — RGBA(1.00, 1.00, 1.00, 1.0)
    /// ACCENTS
    /// Coin Gold — RGBA(1.00, 0.80, 0.30, 1.0)
    /// Coin Shadow — RGBA(0.80, 0.60, 0.20, 1.0)
    /// FX
    /// Shadow — RGBA(0.00, 0.00, 0.00, 0.05)
    /// Overlay / Dim — RGBA(0.00, 0.00, 0.00, 0.25)
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
