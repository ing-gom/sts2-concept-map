using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace Sts2ConceptMap;

/// <summary>
/// A draggable banner on the map screen naming the current map's concept, tinted by difficulty tier.
/// Added as a child of NMapScreen so it lives with the screen. Position is user-draggable and saved to
/// user://. Visibility is suppressed when something is layered over the map (pause/settings modal, deck
/// /relic/potion views, submenus) so it only shows on the actual map-node screen — mirroring
/// Sts2PotionDropChance's overlay watcher / Sts2WinratePreview's MapPreviewPanel.
/// </summary>
internal sealed partial class ConceptBanner : PanelContainer
{
    public const string NodeName = "Sts2ConceptMap_Banner";
    private const string ConfigPath = "user://sts2_concept_map.cfg";

    private static readonly Color[] TierColor =
    {
        new(0.45f, 0.80f, 0.45f), // Tier 1 — green
        new(0.95f, 0.78f, 0.30f), // Tier 2 — amber
        new(0.92f, 0.40f, 0.36f), // Tier 3 — red
    };

    private Label _label = null!;
    private bool _dragging;
    private bool _hasConcept;
    private bool _suppressed;
    private readonly HashSet<NSubmenu> _submenus = new();

    public override void _Ready()
    {
        Name = NodeName;
        // Ignore: dragging is handled in _Input (not GUI routing), so the banner never needs to block
        // mouse events from the map underneath.
        MouseFilter = MouseFilterEnum.Ignore;
        ZIndex = 10;

        // Absolute positioning (top-left anchors); default to top-centre, user drag overrides + saves.
        AnchorLeft = AnchorTop = AnchorRight = AnchorBottom = 0f;
        GrowHorizontal = GrowDirection.End;
        GrowVertical = GrowDirection.End;
        var vp = GetViewportRect().Size;
        OffsetLeft = vp.X * 0.5f - 90f;
        OffsetTop = 16f;

        AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0.08f, 0.09f, 0.13f, 0.92f),
            BorderColor = new Color(0.35f, 0.38f, 0.45f, 0.85f),
            BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderWidthBottom = 1,
            ContentMarginLeft = 16, ContentMarginRight = 16, ContentMarginTop = 6, ContentMarginBottom = 6,
            CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6, CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
        });

        _label = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        _label.AddThemeFontSizeOverride("font_size", 20);
        AddChild(_label);

        LoadPosition();

        // Track modal submenus that appear after us.
        var tree = GetTree();
        if (tree != null)
        {
            tree.NodeAdded += OnNodeAdded;
            ScanForSubmenus(tree.Root);
        }

        Refresh();
    }

    public override void _ExitTree()
    {
        var tree = GetTree();
        if (tree != null) tree.NodeAdded -= OnNodeAdded;
    }

    /// <summary>Update the banner text/colour for whatever concept governs the current map.</summary>
    public void Refresh()
    {
        var c = ConceptMapService.CurrentConcept();
        _hasConcept = c != null;
        if (c != null && _label != null)
        {
            _label.Text = $"{ConceptMapService.LabelOf(c)} {ConceptMapService.RomanLevel(ConceptMapService.CurrentLevel())}";
            _label.AddThemeColorOverride("font_color", TierColor[Math.Clamp(c.Tier - 1, 0, 2)]);
        }
    }

    public override void _Process(double delta)
    {
        try { _suppressed = ShouldSuppress(); }
        catch { _suppressed = false; }

        bool show = _hasConcept && !_suppressed;
        if (Visible != show) Visible = show;
    }

    // ---- drag to reposition (left-drag anywhere on the banner; saved to user://) ----
    // Handled in _Input (not _GuiInput): the banner sits near the top HUD, whose higher canvas layer
    // would otherwise eat the click before GUI routing reaches us. _Input sees the event first; we only
    // act (and consume it) when the press lands inside the banner's rect, so the map is unaffected.

    public override void _Input(InputEvent @event)
    {
        if (!Visible) return;

        switch (@event)
        {
            case InputEventMouseButton { ButtonIndex: MouseButton.Left } mb:
                if (mb.Pressed)
                {
                    if (GetGlobalRect().HasPoint(mb.Position))
                    {
                        _dragging = true;
                        GetViewport().SetInputAsHandled();
                    }
                }
                else if (_dragging)
                {
                    _dragging = false;
                    SavePosition();
                    GetViewport().SetInputAsHandled();
                }
                break;

            case InputEventMouseMotion mm when _dragging:
                OffsetLeft += mm.Relative.X;
                OffsetTop += mm.Relative.Y;
                GetViewport().SetInputAsHandled();
                break;
        }
    }

    private void SavePosition()
    {
        try
        {
            var cf = new ConfigFile();
            cf.SetValue("banner", "offset_x", OffsetLeft);
            cf.SetValue("banner", "offset_y", OffsetTop);
            cf.Save(ConfigPath);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[{MainFile.ModId}] banner position save failed: {ex.Message}");
        }
    }

    private void LoadPosition()
    {
        try
        {
            var cf = new ConfigFile();
            if (cf.Load(ConfigPath) != Error.Ok) return;
            OffsetLeft = (float)cf.GetValue("banner", "offset_x", OffsetLeft).AsDouble();
            OffsetTop = (float)cf.GetValue("banner", "offset_y", OffsetTop).AsDouble();
        }
        catch
        {
            // corrupt/missing config — keep defaults.
        }
    }

    // ---- modal suppression (show only on the map-node screen) ----

    private void OnNodeAdded(Node n)
    {
        if (n is NSubmenu sm) _submenus.Add(sm);
    }

    private void ScanForSubmenus(Node n)
    {
        if (n is NSubmenu sm) _submenus.Add(sm);
        int count = n.GetChildCount();
        for (int i = 0; i < count; i++)
        {
            var c = n.GetChild(i);
            if (c != null) ScanForSubmenus(c);
        }
    }

    private bool ShouldSuppress()
    {
        var modal = NModalContainer.Instance;
        if (modal != null && modal.OpenModal != null) return true;

        // Deck / relic / potion views open over the map without closing it; "map is not the current
        // screen context" is the single general signal that something is drawn on top.
        if (GetParent() is NMapScreen map)
        {
            var asc = ActiveScreenContext.Instance;
            if (asc != null && !asc.IsCurrent(map)) return true;
        }

        _submenus.RemoveWhere(s => !IsInstanceValid(s));
        foreach (var sm in _submenus)
            if (sm.IsVisibleInTree()) return true;
        return false;
    }
}
