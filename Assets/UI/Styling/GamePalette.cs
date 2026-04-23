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
        // Board
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
        public static readonly Color AdSlotBackground = new Color(0.9490f, 0.9333f, 0.9098f, 1f);
        public static readonly Color AdSlotBorder = new Color(0.8667f, 0.8314f, 0.7922f, 1f);
        public static readonly Color ScoreValueText = new Color(0.10f, 0.11f, 0.13f, 1f);
        public static readonly Color SafeAreaDebugFill = new Color(0.2353f, 0.5725f, 0.9922f, 0.16f);
        
        // Buttons
        public static readonly Color ActionButtonSurface = new Color(0.9608f, 0.7451f, 0.3098f, 1f);
        public static readonly Color ActionButtonSurfaceDisabled = new Color(0.92f, 0.92f, 0.91f, 1f);
        public static readonly Color ActionButtonIcon = new Color(1f, 1f, 1f, 1f);
        public static readonly Color ActionButtonIconDisabled = new Color(0.60f, 0.66f, 0.73f, 1f);
        public static readonly Color ActionButtonBadgeBackground = new Color(0.9882f, 0.8157f, 0.4588f, 1f);
        public static readonly Color ActionButtonBadgeText = new Color(0.2980f, 0.2549f, 0.1843f, 1f);
        
        // Tab Bar
        public static readonly Color TabBarBackground = new Color(0.9804f, 0.9647f, 0.9412f, 0.98f);
        public static readonly Color TabBarBorder = new Color(0.8667f, 0.8314f, 0.7922f, 1f);
        public static readonly Color TabBarActive = new Color(0.2275f, 0.5529f, 0.8706f, 1f);
        public static readonly Color TabBarInactive = new Color(0.5059f, 0.5137f, 0.5451f, 1f);
        
        // Daily challenges
        public static readonly Color DailyHeaderIcon = new Color(0.2275f, 0.5529f, 0.8706f, 1f);
        public static readonly Color DailyHeaderText = new Color(0.10f, 0.11f, 0.13f, 1f);
        public static readonly Color DailyWeekdayText = new Color(0.74f, 0.73f, 0.76f, 1f);
        public static readonly Color DailyDayText = new Color(0.36f, 0.36f, 0.40f, 1f);
        public static readonly Color DailyFutureDayText = new Color(0.84f, 0.83f, 0.85f, 1f);
        public static readonly Color DailySelectedDayBackground = new Color(0.2275f, 0.5529f, 0.8706f, 1f);
        public static readonly Color DailySelectedDayText = Color.white;
        public static readonly Color DailyProgressTrack = new Color(0.90f, 0.91f, 0.93f, 1f);
        public static readonly Color DailyProgressFill = new Color(0.2275f, 0.5529f, 0.8706f, 1f);
        public static readonly Color DailyCompletedRing = new Color(0.9608f, 0.7451f, 0.3098f, 1f);
        public static readonly Color DailyTodayDot = new Color(0.2275f, 0.5529f, 0.8706f, 1f);
        public static readonly Color DailyProgressBarTrack = new Color(0.90f, 0.91f, 0.93f, 1f);
        public static readonly Color DailyProgressBarFill = new Color(0.2275f, 0.5529f, 0.8706f, 1f);

        public static readonly Color GameOverOverlay = new Color(0.05f, 0.06f, 0.09f, 0.84f);

        // Developer Panel
        public static readonly Color DeveloperPanelBackground = new Color(0.14f, 0.17f, 0.22f, 0.96f);
        public static readonly Color DeveloperPanelButton = new Color(0.82f, 0.40f, 0.26f, 1f);
        public static readonly Color DeveloperPanelInfo = new Color(0.88f, 0.91f, 0.96f, 1f);
    }
}
