using System;
using Game.UI.Layout;
using Game.UI.Styling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.AppTabs
{
    [DisallowMultipleComponent]
    public sealed class AppTabsView : MonoBehaviour
    {
        private const float TabBarHeight = 140f;
        private const float MinTabBarHeight = 116f;
        private const float MaxTabBarHeight = 156f;

        private RectTransform _safeAreaRect;
        private RectTransform _contentAreaRect;
        private RectTransform _tabBarRect;
        private MainTabView _mainTabView;
        private PlaceholderTabView _dailyTabView;
        private PlaceholderTabView _journeyTabView;
        private PlaceholderTabView _meTabView;
        private TabBarView _tabBarView;
        private Vector2 _lastSafeAreaSize = new Vector2(-1f, -1f);
        private AppTabId _selectedTab = AppTabId.Main;

        public event Action ContinueClicked;
        public event Action NewGameClicked;

        public static AppTabsView Create(
            Transform parent,
            TMP_FontAsset regularFont,
            TMP_FontAsset boldFont,
            bool showSafeAreaDebugOverlay,
            Texture2D mainIconTexture,
            Texture2D dailyIconTexture,
            Texture2D journeyIconTexture,
            Texture2D meIconTexture)
        {
            var screenObject = new GameObject(
                "AppTabsView",
                typeof(RectTransform),
                typeof(AppTabsView));

            screenObject.transform.SetParent(parent, false);

            var rootRect = (RectTransform)screenObject.transform;
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var backgroundObject = new GameObject("BackgroundFullBleed", typeof(RectTransform), typeof(Image));
            backgroundObject.transform.SetParent(screenObject.transform, false);

            var backgroundRect = (RectTransform)backgroundObject.transform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            var backgroundImage = backgroundObject.GetComponent<Image>();
            backgroundImage.color = GamePalette.GameBackground;

            if (showSafeAreaDebugOverlay)
            {
                SafeAreaDebugOverlayView.Create(
                    screenObject.transform,
                    "SafeAreaDebugOverlay",
                    GamePalette.SafeAreaDebugFill);
            }

            SafeAreaView safeAreaView = SafeAreaView.Create(screenObject.transform, "SafeAreaContent");
            RectTransform safeAreaRect = (RectTransform)safeAreaView.transform;

            var contentAreaObject = new GameObject("ContentArea", typeof(RectTransform));
            contentAreaObject.transform.SetParent(safeAreaRect, false);

            var contentAreaRect = (RectTransform)contentAreaObject.transform;
            contentAreaRect.anchorMin = new Vector2(0f, 0f);
            contentAreaRect.anchorMax = new Vector2(1f, 1f);
            contentAreaRect.offsetMin = new Vector2(0f, TabBarHeight);
            contentAreaRect.offsetMax = Vector2.zero;

            var tabBarObject = new GameObject("TabBarArea", typeof(RectTransform));
            tabBarObject.transform.SetParent(safeAreaRect, false);

            var tabBarRect = (RectTransform)tabBarObject.transform;
            tabBarRect.anchorMin = new Vector2(0f, 0f);
            tabBarRect.anchorMax = new Vector2(1f, 0f);
            tabBarRect.pivot = new Vector2(0.5f, 0f);
            tabBarRect.sizeDelta = new Vector2(0f, TabBarHeight);
            tabBarRect.anchoredPosition = Vector2.zero;

            MainTabView mainTabView = MainTabView.Create(contentAreaObject.transform, regularFont, boldFont);
            PlaceholderTabView dailyTabView = PlaceholderTabView.Create(contentAreaObject.transform, "Daily", regularFont, boldFont);
            PlaceholderTabView journeyTabView = PlaceholderTabView.Create(contentAreaObject.transform, "Journey", regularFont, boldFont);
            PlaceholderTabView meTabView = PlaceholderTabView.Create(contentAreaObject.transform, "Me", regularFont, boldFont);
            TabBarView tabBarView = TabBarView.Create(
                tabBarObject.transform,
                regularFont,
                mainIconTexture,
                dailyIconTexture,
                journeyIconTexture,
                meIconTexture);

            var overlayRootObject = new GameObject("OverlayRoot", typeof(RectTransform));
            overlayRootObject.transform.SetParent(screenObject.transform, false);

            var overlayRect = (RectTransform)overlayRootObject.transform;
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            var appTabsView = screenObject.GetComponent<AppTabsView>();
            appTabsView.Setup(
                safeAreaRect,
                contentAreaRect,
                tabBarRect,
                mainTabView,
                dailyTabView,
                journeyTabView,
                meTabView,
                tabBarView);

            return appTabsView;
        }

        public void SetBestScore(int bestScore)
        {
            _mainTabView?.SetBestScore(bestScore);
        }

        public void SetContinueVisible(bool isVisible)
        {
            _mainTabView?.SetContinueVisible(isVisible);
        }

        public void SelectTab(AppTabId tabId)
        {
            _selectedTab = tabId;

            if (_mainTabView != null)
            {
                _mainTabView.gameObject.SetActive(tabId == AppTabId.Main);
            }

            if (_dailyTabView != null)
            {
                _dailyTabView.gameObject.SetActive(tabId == AppTabId.Daily);
            }

            if (_journeyTabView != null)
            {
                _journeyTabView.gameObject.SetActive(tabId == AppTabId.Journey);
            }

            if (_meTabView != null)
            {
                _meTabView.gameObject.SetActive(tabId == AppTabId.Me);
            }

            _tabBarView?.SetSelectedTab(tabId);
        }

        private void OnEnable()
        {
            ApplyResponsiveLayout(force: true);
        }

        private void LateUpdate()
        {
            ApplyResponsiveLayout(force: false);
        }

        private void OnDestroy()
        {
            if (_mainTabView != null)
            {
                _mainTabView.ContinueClicked -= HandleContinueClicked;
                _mainTabView.NewGameClicked -= HandleNewGameClicked;
            }

            if (_tabBarView != null)
            {
                _tabBarView.TabSelected -= HandleTabSelected;
            }
        }

        private void Setup(
            RectTransform safeAreaRect,
            RectTransform contentAreaRect,
            RectTransform tabBarRect,
            MainTabView mainTabView,
            PlaceholderTabView dailyTabView,
            PlaceholderTabView journeyTabView,
            PlaceholderTabView meTabView,
            TabBarView tabBarView)
        {
            _safeAreaRect = safeAreaRect;
            _contentAreaRect = contentAreaRect;
            _tabBarRect = tabBarRect;
            _mainTabView = mainTabView;
            _dailyTabView = dailyTabView;
            _journeyTabView = journeyTabView;
            _meTabView = meTabView;
            _tabBarView = tabBarView;

            _mainTabView.ContinueClicked += HandleContinueClicked;
            _mainTabView.NewGameClicked += HandleNewGameClicked;
            _tabBarView.TabSelected += HandleTabSelected;

            SelectTab(_selectedTab);
            ApplyResponsiveLayout(force: true);
        }

        private void HandleContinueClicked()
        {
            ContinueClicked?.Invoke();
        }

        private void HandleNewGameClicked()
        {
            NewGameClicked?.Invoke();
        }

        private void HandleTabSelected(AppTabId tabId)
        {
            SelectTab(tabId);
        }

        private void ApplyResponsiveLayout(bool force)
        {
            if (_safeAreaRect == null)
            {
                return;
            }

            Rect safeRect = _safeAreaRect.rect;
            Vector2 safeSize = new Vector2(safeRect.width, safeRect.height);
            if (safeSize.x <= 0f || safeSize.y <= 0f)
            {
                return;
            }

            if (!force && Vector2.Distance(_lastSafeAreaSize, safeSize) < 0.5f)
            {
                return;
            }

            _lastSafeAreaSize = safeSize;

            float scale = MobileLayout.GetScale(safeRect.width, safeRect.height);
            float tabBarHeight = MobileLayout.ClampScaled(TabBarHeight, MinTabBarHeight, MaxTabBarHeight, scale);

            if (_tabBarRect != null)
            {
                _tabBarRect.sizeDelta = new Vector2(0f, tabBarHeight);
            }

            if (_contentAreaRect != null)
            {
                _contentAreaRect.offsetMin = new Vector2(0f, tabBarHeight);
            }
        }
    }
}
