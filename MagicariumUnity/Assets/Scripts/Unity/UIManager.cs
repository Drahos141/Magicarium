using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Magicarium.Buildings;
using Magicarium.Entities;
using Magicarium.Map;
using Magicarium.Players;
using Magicarium.Resources;

namespace Magicarium.Unity
{
    /// <summary>
    /// Creates and manages the entire UGUI canvas hierarchy at runtime.
    ///
    /// Layout (1280 × 720 reference):
    ///   Top bar (60px)  : resource counters  |  turn label  |  End Turn button
    ///   Main area       : map scroll area (left ~58%) | info/action panel (right ~42%)
    ///   Bottom bar (55px): message log
    ///
    /// Map is a 20×20 grid of clickable tile buttons, each showing terrain colour
    /// and a short label for any unit/building present on that tile.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        // ── Canvas / panels ──────────────────────────────────────────
        private Canvas _canvas;

        // Top bar
        private Text _goldText;
        private Text _woodText;
        private Text _oreText;
        private Text _turnText;

        // Map grid
        private Image[,] _tileBg;
        private Text[,] _tileLbl;

        // Info / action panel
        private Text _selectedNameText;
        private Text _selectedHpText;
        private Text _selectedDetailText;

        private Button _btnMove;
        private Button _btnAttack;
        private Button _btnGather;

        private GameObject _buildPanel;
        private GameObject _trainPanel;

        // Bottom log
        private Text _messageText;

        // Game-over overlay
        private GameObject _gameOverOverlay;
        private Text _gameOverText;

        // Font
        private Font _font;

        // ── Colours ───────────────────────────────────────────────────
        private static readonly Color ColGrass    = new Color(0.35f, 0.55f, 0.25f);
        private static readonly Color ColForest   = new Color(0.15f, 0.38f, 0.15f);
        private static readonly Color ColMountain = new Color(0.55f, 0.55f, 0.55f);
        private static readonly Color ColWater    = new Color(0.18f, 0.45f, 0.75f);
        private static readonly Color ColMine     = new Color(0.70f, 0.60f, 0.15f);
        private static readonly Color ColRoad     = new Color(0.50f, 0.38f, 0.18f);

        private static readonly Color ColHuman  = new Color(0.20f, 0.70f, 1.00f);   // blue-ish
        private static readonly Color ColEnemy  = new Color(1.00f, 0.30f, 0.25f);   // red
        private static readonly Color ColSelect = new Color(1.00f, 0.95f, 0.20f);   // yellow

        private static readonly Color ColPanel       = new Color(0.10f, 0.10f, 0.12f, 0.95f);
        private static readonly Color ColPanelLight  = new Color(0.15f, 0.15f, 0.18f, 0.95f);
        private static readonly Color ColBtn         = new Color(0.22f, 0.22f, 0.28f, 1.00f);
        private static readonly Color ColBtnHover    = new Color(0.30f, 0.30f, 0.40f, 1.00f);
        private static readonly Color ColBtnDisabled = new Color(0.15f, 0.15f, 0.18f, 0.60f);

        // ────────────────────────────────────────────────────────────
        // Initialisation
        // ────────────────────────────────────────────────────────────

        public void Initialise()
        {
            _font = LoadFont();
            CreateEventSystem();
            CreateCanvas();
        }

        private static Font LoadFont()
        {
            // Unity 2022+ built-in font name
            Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (f == null) f = Font.CreateDynamicFontFromOSFont("Arial", 14);
            return f;
        }

        private static void CreateEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        // ────────────────────────────────────────────────────────────
        // Canvas & main layout
        // ────────────────────────────────────────────────────────────

        private void CreateCanvas()
        {
            var cGo = new GameObject("Canvas");
            _canvas = cGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 0;

            var scaler = cGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;

            cGo.AddComponent<GraphicRaycaster>();

            var root = cGo.GetComponent<RectTransform>();

            // ── Top bar ──────────────────────────────────────────────
            BuildTopBar(root);

            // ── Main area ─────────────────────────────────────────────
            // map: x=0, y=55 (bottom), w=740, h=605   right panel: x=740, w=540, h=605
            BuildMapArea(root);
            BuildInfoPanel(root);

            // ── Bottom log ───────────────────────────────────────────
            BuildBottomLog(root);

            // ── Game-over overlay ────────────────────────────────────
            BuildGameOverOverlay(root);

            // Initial display
            Refresh();
        }

        // ────────────────────────────────────────────────────────────
        // Top bar
        // ────────────────────────────────────────────────────────────

        private void BuildTopBar(RectTransform root)
        {
            var bar = MakePanel(root, "TopBar", 0, 660, 1280, 60, ColPanel);

            // Title
            MakeLabel(bar, "Title", 10, 5, 160, 50, "MAGICARIUM", 18, TextAnchor.MiddleLeft, Color.yellow);

            // Resources
            _goldText = MakeLabel(bar, "Gold",    180, 5, 160, 50, "Gold: 0",    14, TextAnchor.MiddleLeft, new Color(1f, 0.9f, 0.3f));
            _woodText = MakeLabel(bar, "Wood",    350, 5, 160, 50, "Wood: 0",    14, TextAnchor.MiddleLeft, new Color(0.6f, 0.9f, 0.4f));
            _oreText  = MakeLabel(bar, "Ore",     520, 5, 200, 50, "Magic Ore: 0", 14, TextAnchor.MiddleLeft, new Color(0.7f, 0.5f, 1.0f));

            // Turn label
            _turnText = MakeLabel(bar, "Turn", 750, 5, 200, 50, "Turn: 0", 14, TextAnchor.MiddleCenter, Color.white);

            // End Turn button
            MakeButton(bar, "EndTurn", 1050, 8, 200, 44, "End Turn",
                Color.white, new Color(0.2f, 0.5f, 0.2f), () => GameManager.Instance.EndTurn());
        }

        // ────────────────────────────────────────────────────────────
        // Map area
        // ────────────────────────────────────────────────────────────

        private void BuildMapArea(RectTransform root)
        {
            // Viewport panel
            var mapPanel = MakePanel(root, "MapPanel", 0, 55, 740, 605, ColPanelLight);

            // Scrollable viewport
            var viewportGo = MakePanel(mapPanel, "Viewport", 5, 5, 730, 595, Color.clear);
            viewportGo.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            // Give it an Image so the Mask works
            var maskImg = viewportGo.gameObject.GetComponent<Image>();
            if (maskImg == null) maskImg = viewportGo.gameObject.AddComponent<Image>();
            maskImg.color = Color.clear;

            // Content area (same size as viewport for a 20×20 grid at 35px per tile = 700×700)
            const float tileSize = 35f;
            const int   mapSize  = GameManager.MapSize;
            float gridW = tileSize * mapSize;
            float gridH = tileSize * mapSize;

            var contentGo = new GameObject("MapContent");
            contentGo.transform.SetParent(viewportGo, false);
            var contentRT = contentGo.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 0);
            contentRT.anchorMax = new Vector2(0, 0);
            contentRT.pivot     = new Vector2(0, 0);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta = new Vector2(gridW, gridH);

            // ScrollRect
            var sr = mapPanel.gameObject.AddComponent<ScrollRect>();
            sr.content    = contentRT;
            sr.viewport   = viewportGo;
            sr.horizontal = true;
            sr.vertical   = true;
            sr.scrollSensitivity = 20f;

            // Tile buttons
            _tileBg  = new Image[mapSize, mapSize];
            _tileLbl = new Text[mapSize, mapSize];

            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    float px = x * tileSize;
                    float py = y * tileSize;

                    var tileGo = new GameObject($"Tile_{x}_{y}");
                    tileGo.transform.SetParent(contentGo.transform, false);

                    var rt = tileGo.AddComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0, 0);
                    rt.anchorMax = new Vector2(0, 0);
                    rt.pivot     = new Vector2(0, 0);
                    rt.anchoredPosition = new Vector2(px, py);
                    rt.sizeDelta = new Vector2(tileSize - 1, tileSize - 1);

                    _tileBg[x, y] = tileGo.AddComponent<Image>();
                    _tileBg[x, y].color = ColGrass;

                    // Label child
                    var lblGo = new GameObject("Lbl");
                    lblGo.transform.SetParent(tileGo.transform, false);
                    var lblRT = lblGo.AddComponent<RectTransform>();
                    lblRT.anchorMin = Vector2.zero;
                    lblRT.anchorMax = Vector2.one;
                    lblRT.offsetMin = Vector2.zero;
                    lblRT.offsetMax = Vector2.zero;

                    _tileLbl[x, y] = lblGo.AddComponent<Text>();
                    _tileLbl[x, y].font      = _font;
                    _tileLbl[x, y].fontSize  = 11;
                    _tileLbl[x, y].alignment = TextAnchor.MiddleCenter;
                    _tileLbl[x, y].color     = Color.white;

                    // Button
                    var btn = tileGo.AddComponent<Button>();
                    var handler = tileGo.AddComponent<TileClickHandler>();
                    handler.TileX = x;
                    handler.TileY = y;
                    btn.onClick.AddListener(handler.OnClick);

                    // Button colours
                    var cb = btn.colors;
                    cb.normalColor      = Color.white;
                    cb.highlightedColor = new Color(1f, 1f, 0.6f, 1f);
                    cb.pressedColor     = new Color(0.8f, 0.8f, 0.8f, 1f);
                    btn.colors = cb;
                }
            }
        }

        // ────────────────────────────────────────────────────────────
        // Info / action panel (right side)
        // ────────────────────────────────────────────────────────────

        private void BuildInfoPanel(RectTransform root)
        {
            var panel = MakePanel(root, "InfoPanel", 742, 55, 538, 605, ColPanel);

            float y = 575;

            MakeLabel(panel, "SelHeader", 10, y, 518, 24, "Selected", 14, TextAnchor.MiddleLeft, Color.cyan);
            y -= 28;

            _selectedNameText   = MakeLabel(panel, "SelName",   10, y, 518, 22, "Nothing selected", 12, TextAnchor.MiddleLeft, Color.white);
            y -= 24;
            _selectedHpText     = MakeLabel(panel, "SelHp",     10, y, 518, 22, "", 12, TextAnchor.MiddleLeft, new Color(0.8f, 1f, 0.8f));
            y -= 24;
            _selectedDetailText = MakeLabel(panel, "SelDetail", 10, y, 518, 22, "", 12, TextAnchor.MiddleLeft, new Color(0.9f, 0.9f, 0.7f));
            y -= 30;

            // ── Unit action buttons ──────────────────────────────────
            MakeLabel(panel, "ActHeader", 10, y, 518, 22, "── Unit Actions ──", 12, TextAnchor.MiddleLeft, Color.grey);
            y -= 28;

            _btnMove   = MakeButton(panel, "BtnMove",   10,  y, 155, 32, "Move",    Color.white, ColBtn, () => GameManager.Instance.StartMove());
            _btnAttack = MakeButton(panel, "BtnAttack", 175, y, 155, 32, "Attack",  Color.white, ColBtn, () => GameManager.Instance.StartAttack());
            _btnGather = MakeButton(panel, "BtnGather", 340, y, 180, 32, "Gather/Deposit", Color.white, ColBtn, () => GameManager.Instance.WorkerGatherOrDeposit());
            y -= 40;

            // ── Build panel ──────────────────────────────────────────
            MakeLabel(panel, "BuildHeader", 10, y, 518, 22, "── Build ──", 12, TextAnchor.MiddleLeft, Color.grey);
            y -= 28;

            _buildPanel = new GameObject("BuildPanel");
            _buildPanel.transform.SetParent(panel, false);
            var bpRT = _buildPanel.AddComponent<RectTransform>();
            bpRT.anchorMin = new Vector2(0, 0);
            bpRT.anchorMax = new Vector2(0, 0);
            bpRT.pivot     = new Vector2(0, 0);
            bpRT.anchoredPosition = new Vector2(10, y - 80);
            bpRT.sizeDelta = new Vector2(518, 90);

            // Row 1 of build buttons
            float bx = 0;
            foreach (var entry in BuildButtons1())
            {
                var bt = entry;
                MakeButton(_buildPanel.GetComponent<RectTransform>(), $"Btn{bt.type}", bx, 44, 120, 32,
                    bt.label, Color.white, ColBtn, () => GameManager.Instance.StartBuild(bt.type));
                bx += 126;
            }
            // Row 2 of build buttons
            bx = 0;
            foreach (var entry in BuildButtons2())
            {
                var bt = entry;
                MakeButton(_buildPanel.GetComponent<RectTransform>(), $"Btn{bt.type}2", bx, 6, 120, 32,
                    bt.label, Color.white, ColBtn, () => GameManager.Instance.StartBuild(bt.type));
                bx += 126;
            }

            y -= 96;

            // ── Train panel ──────────────────────────────────────────
            MakeLabel(panel, "TrainHeader", 10, y, 518, 22, "── Train ──", 12, TextAnchor.MiddleLeft, Color.grey);
            y -= 28;

            _trainPanel = new GameObject("TrainPanel");
            _trainPanel.transform.SetParent(panel, false);
            var tpRT = _trainPanel.AddComponent<RectTransform>();
            tpRT.anchorMin = new Vector2(0, 0);
            tpRT.anchorMax = new Vector2(0, 0);
            tpRT.pivot     = new Vector2(0, 0);
            tpRT.anchoredPosition = new Vector2(10, y - 40);
            tpRT.sizeDelta = new Vector2(518, 44);

            float tx = 0;
            foreach (var entry in TrainButtons())
            {
                var tb = entry;
                MakeButton(_trainPanel.GetComponent<RectTransform>(), $"Train{tb.type}", tx, 6, 120, 32,
                    tb.label, Color.white, ColBtn, () => GameManager.Instance.TrainUnit(tb.type));
                tx += 126;
            }
        }

        // ────────────────────────────────────────────────────────────
        // Bottom message log
        // ────────────────────────────────────────────────────────────

        private void BuildBottomLog(RectTransform root)
        {
            var bar = MakePanel(root, "LogBar", 0, 0, 1280, 55, ColPanel);
            _messageText = MakeLabel(bar, "Msg", 10, 5, 1260, 45, "Welcome to Magicarium! Select a unit to begin.", 13, TextAnchor.MiddleLeft, new Color(0.9f, 0.9f, 0.7f));
        }

        // ────────────────────────────────────────────────────────────
        // Game-over overlay
        // ────────────────────────────────────────────────────────────

        private void BuildGameOverOverlay(RectTransform root)
        {
            _gameOverOverlay = MakePanel(root, "GameOver", 340, 235, 600, 250, new Color(0, 0, 0, 0.88f)).gameObject;
            MakeLabel(_gameOverOverlay.GetComponent<RectTransform>(), "GOTitle", 0, 150, 600, 60, "GAME OVER", 36, TextAnchor.MiddleCenter, Color.yellow);
            _gameOverText = MakeLabel(_gameOverOverlay.GetComponent<RectTransform>(), "GOMsg", 0, 80, 600, 50, "", 22, TextAnchor.MiddleCenter, Color.white);
            MakeButton(_gameOverOverlay.GetComponent<RectTransform>(), "GORestart", 200, 15, 200, 45, "Restart", Color.white, new Color(0.2f, 0.4f, 0.2f),
                () => UnityEngine.SceneManagement.SceneManager.LoadScene(0));
            _gameOverOverlay.SetActive(false);
        }

        // ────────────────────────────────────────────────────────────
        // Public refresh methods
        // ────────────────────────────────────────────────────────────

        /// <summary>
        /// Full refresh: map tiles + HUD. Called by GameManager after any state change.
        /// </summary>
        public void Refresh()
        {
            var gm  = GameManager.Instance;
            var gs  = gm.GameState;
            var hp  = gm.HumanPlayer;

            // Resources & turn
            _goldText.text = $"Gold: {hp.Resources.Get(ResourceType.Gold)}";
            _woodText.text = $"Wood: {hp.Resources.Get(ResourceType.Wood)}";
            _oreText.text  = $"Magic Ore: {hp.Resources.Get(ResourceType.MagicOre)}";
            _turnText.text = $"Turn: {gs.CurrentTurn}";

            // Map
            RefreshMap(gs, hp);

            // Info panel
            RefreshInfoPanel(gm, hp);
        }

        private void RefreshMap(GameState gs, Player human)
        {
            int size = GameManager.MapSize;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var tile = gs.Map.GetTile(x, y);

                    // Terrain colour
                    Color bg = TerrainColour(tile);

                    // Entity on tile?
                    string lbl  = TerrainLabel(tile);
                    Color  lblC = new Color(0.85f, 0.85f, 0.85f);

                    bool hasHuman = false, hasEnemy = false;

                    foreach (var p in gs.Players)
                    {
                        bool isHuman = p.Id == human.Id;

                        foreach (var u in p.Units)
                        {
                            if (!u.IsAlive || u.X != x || u.Y != y) continue;
                            lbl  = UnitAbbrev(u.Type, isHuman);
                            lblC = isHuman ? ColHuman : ColEnemy;
                            if (isHuman) hasHuman = true; else hasEnemy = true;
                        }
                        foreach (var b in p.Buildings)
                        {
                            if (!b.IsAlive || b.X != x || b.Y != y) continue;
                            lbl  = BuildingAbbrev(b.Type, isHuman);
                            lblC = isHuman ? ColHuman : ColEnemy;
                            if (isHuman) hasHuman = true; else hasEnemy = true;
                        }
                    }

                    // Selected tile highlight
                    var sel = GameManager.Instance.SelectedEntity;
                    bool isSelected = sel != null && sel.X == x && sel.Y == y && sel.IsAlive;
                    if (isSelected) bg = Color.Lerp(bg, ColSelect, 0.55f);

                    // Slight tint for owned vs enemy tiles
                    if (hasHuman && !isSelected) bg = Color.Lerp(bg, ColHuman, 0.15f);
                    if (hasEnemy && !isSelected) bg = Color.Lerp(bg, ColEnemy, 0.15f);

                    _tileBg[x, y].color    = bg;
                    _tileLbl[x, y].text    = lbl;
                    _tileLbl[x, y].color   = lblC;
                    _tileLbl[x, y].fontStyle = FontStyle.Bold;
                }
            }
        }

        private void RefreshInfoPanel(GameManager gm, Player human)
        {
            var sel = gm.SelectedEntity;
            bool isOwned = sel != null && sel.OwnerId == human.Id;

            if (sel == null)
            {
                _selectedNameText.text   = "Nothing selected";
                _selectedHpText.text     = "";
                _selectedDetailText.text = "Click any tile with a unit or building.";
            }
            else
            {
                string owner = isOwned ? "Your" : "Enemy";
                string name;
                if (sel is Unit u)            name = $"{owner} {u.Type}";
                else if (sel is Building b)   name = $"{owner} {b.Type}";
                else                          name = "Unknown";
                _selectedNameText.text   = name;
                _selectedHpText.text     = $"HP: {sel.Health} / {sel.MaxHealth}";

                // Extra detail
                if (sel is Worker w)
                    _selectedDetailText.text = $"Carrying: {w.CarriedAmount}/{w.CarryCapacity}" +
                        (w.CarriedResource.HasValue ? $" {w.CarriedResource.Value}" : "");
                else if (sel is Building bd)
                    _selectedDetailText.text = bd.BenefitDescription;
                else
                    _selectedDetailText.text = "";
            }

            // Mode indicator appended to selected name
            if (gm.CurrentMode != GameManager.ActionMode.None)
                _selectedNameText.text += $"  [{gm.CurrentMode}]";

            // Enable/disable action buttons
            bool unitOwned = sel is Unit && isOwned;
            bool workerOwned = sel is Worker && isOwned;
            SetButtonInteractable(_btnMove,   unitOwned);
            SetButtonInteractable(_btnAttack, unitOwned && !(sel is Worker));
            SetButtonInteractable(_btnGather, workerOwned);
        }

        // ────────────────────────────────────────────────────────────
        // Public helpers called from GameManager
        // ────────────────────────────────────────────────────────────

        public void ShowMessage(string msg)
        {
            if (_messageText != null) _messageText.text = msg;
        }

        public void ShowGameOver(bool humanWon, string winnerName)
        {
            _gameOverOverlay.SetActive(true);
            _gameOverText.text = humanWon
                ? "Victory! You have conquered Magicarium!"
                : $"{winnerName} wins. Better luck next time.";
        }

        // ────────────────────────────────────────────────────────────
        // Data tables
        // ────────────────────────────────────────────────────────────

        private static Color TerrainColour(MapTile t)
        {
            switch (t.Terrain)
            {
                case TerrainType.Grass:    return ColGrass;
                case TerrainType.Forest:   return ColForest;
                case TerrainType.Mountain: return ColMountain;
                case TerrainType.Water:    return ColWater;
                case TerrainType.Mine:     return ColMine;
                case TerrainType.Road:     return ColRoad;
                default:                   return ColGrass;
            }
        }

        private static string TerrainLabel(MapTile t)
        {
            switch (t.Terrain)
            {
                case TerrainType.Mountain: return "^^";
                case TerrainType.Water:    return "~~";
                case TerrainType.Mine:
                    if (t.ResourceYield == ResourceType.Gold)    return "Au";
                    if (t.ResourceYield == ResourceType.MagicOre) return "Mo";
                    return "Mi";
                case TerrainType.Forest:   return t.ResourceAmount > 0 ? "Fo" : "";
                default:                   return "";
            }
        }

        private static string UnitAbbrev(UnitType t, bool human)
        {
            string s;
            switch (t)
            {
                case UnitType.Worker:  s = "W"; break;
                case UnitType.Soldier: s = "S"; break;
                case UnitType.Archer:  s = "A"; break;
                case UnitType.Knight:  s = "K"; break;
                default:               s = "?"; break;
            }
            return human ? s : s.ToLower();
        }

        private static string BuildingAbbrev(BuildingType t, bool human)
        {
            string s;
            switch (t)
            {
                case BuildingType.MainBase:       s = "HQ"; break;
                case BuildingType.Barracks:       s = "Br"; break;
                case BuildingType.BlacksmithHall: s = "Bs"; break;
                case BuildingType.Farm:           s = "Fm"; break;
                case BuildingType.Mill:           s = "Mi"; break;
                case BuildingType.Church:         s = "Ch"; break;
                case BuildingType.Well:           s = "We"; break;
                case BuildingType.Shrine:         s = "Sh"; break;
                case BuildingType.Field:          s = "Fi"; break;
                case BuildingType.Road:           s = "Rd"; break;
                default:                          s = "Bd"; break;
            }
            return human ? s : s.ToLower();
        }

        private static IEnumerable<(BuildingType type, string label)> BuildButtons1()
        {
            yield return (BuildingType.Farm,           "Farm\n50G 30W");
            yield return (BuildingType.Barracks,       "Barracks\n150G 100W");
            yield return (BuildingType.BlacksmithHall, "Blacksmith\n200G 150W 50M");
            yield return (BuildingType.Mill,           "Mill\n75G 50W");
        }

        private static IEnumerable<(BuildingType type, string label)> BuildButtons2()
        {
            yield return (BuildingType.Church, "Church\n100G 80W 20M");
            yield return (BuildingType.Well,   "Well\n40G 20W");
            yield return (BuildingType.Shrine, "Shrine\n80G 30M");
            yield return (BuildingType.Field,  "Field\n30G 10W");
            // Road handled separately if needed
        }

        private static IEnumerable<(UnitType type, string label)> TrainButtons()
        {
            yield return (UnitType.Worker,  "Worker\n50G");
            yield return (UnitType.Soldier, "Soldier\n100G");
            yield return (UnitType.Archer,  "Archer\n80G 20M");
            yield return (UnitType.Knight,  "Knight\n200G 50M");
        }

        // ────────────────────────────────────────────────────────────
        // UGUI helper utilities
        // ────────────────────────────────────────────────────────────

        /// <summary>Creates a plain rectangle panel with an optional background colour.</summary>
        private RectTransform MakePanel(RectTransform parent, string name,
            float x, float y, float w, float h, Color bg)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.pivot     = new Vector2(0, 0);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);

            var img = go.AddComponent<Image>();
            img.color = bg;
            return rt;
        }

        /// <summary>Creates a Text label.</summary>
        private Text MakeLabel(RectTransform parent, string name,
            float x, float y, float w, float h,
            string text, int fontSize,
            TextAnchor anchor, Color colour)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.pivot     = new Vector2(0, 0);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);

            var lbl = go.AddComponent<Text>();
            lbl.font      = _font;
            lbl.text      = text;
            lbl.fontSize  = fontSize;
            lbl.alignment = anchor;
            lbl.color     = colour;
            lbl.supportRichText = false;
            return lbl;
        }

        /// <summary>Creates a clickable Button with a coloured background and label.</summary>
        private Button MakeButton(RectTransform parent, string name,
            float x, float y, float w, float h,
            string label, Color textColour, Color bgColour,
            UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.pivot     = new Vector2(0, 0);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);

            var img = go.AddComponent<Image>();
            img.color = bgColour;

            // Text child
            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(go.transform, false);
            var txtRT = txtGo.AddComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = new Vector2(2, 2);
            txtRT.offsetMax = new Vector2(-2, -2);

            var txt = txtGo.AddComponent<Text>();
            txt.font      = _font;
            txt.text      = label;
            txt.fontSize  = 11;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color     = textColour;
            txt.supportRichText = false;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var cb = btn.colors;
            cb.normalColor      = bgColour;
            cb.highlightedColor = ColBtnHover;
            cb.pressedColor     = new Color(bgColour.r * 0.7f, bgColour.g * 0.7f, bgColour.b * 0.7f);
            cb.disabledColor    = ColBtnDisabled;
            btn.colors = cb;
            btn.onClick.AddListener(onClick);

            return btn;
        }

        private static void SetButtonInteractable(Button btn, bool interactable)
        {
            if (btn != null) btn.interactable = interactable;
        }
    }
}
