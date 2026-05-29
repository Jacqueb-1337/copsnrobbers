// CNRDebugInspector.cs
// Tap to inspect nearby GOs. Info panel has a HIDE/SHOW CHILDREN button
// to toggle all direct children of the selected GO, so you can confirm
// which panel owns which UI.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CNRDebugInspector
{
    public static class InspectorEntry
    {
        public static void Load()
        {
            try
            {
                var go = new GameObject("CNRDebugInspector");
                go.AddComponent<InspectorBehaviour>();
                GameObject.DontDestroyOnLoad(go);
                Debug.Log("[CNRDbg] loaded");
            }
            catch (Exception ex) { Debug.Log("[CNRDbg] Load error: " + ex); }
        }
    }

    public class InspectorBehaviour : MonoBehaviour
    {
        bool            _btnMode       = false;  // false=hierarchy mode, true=buttons mode

        List<Transform> _candidates    = null;
        int             _candidateIdx  = 0;
        Vector2         _lastTapScreen = Vector2.zero;
        bool            _hasTap        = false;

        string    _info        = "";
        Rect      _hlRect      = new Rect(0, 0, 0, 0);
        bool      _hasHL       = false;
        Vector2   _tapDot      = Vector2.zero;
        Vector2   _elemDot     = Vector2.zero;
        bool      _hasElemDot  = false;

        // Children-toggle state
        GameObject              _toggleTarget    = null;  // GO whose children we toggled
        bool                    _childrenHidden  = false;
        List<GameObject>        _hiddenChildren  = new List<GameObject>(); // those we hid

        // Self-toggle state
        GameObject _selfHiddenGO = null;

        void ToggleSelf(GameObject go)
        {
            if (_selfHiddenGO == go)
            {
                go.SetActive(true);
                _selfHiddenGO = null;
                Debug.Log("[CNRDbg] Restored " + go.name);
            }
            else
            {
                if (_selfHiddenGO != null) { _selfHiddenGO.SetActive(true); }
                go.SetActive(false);
                _selfHiddenGO = go;
                Debug.Log("[CNRDbg] Hidden self: " + go.name);
            }
        }

        // Rect of the hide/show button in GUI space — used to swallow taps on it
        Rect _btnRect      = new Rect(0, 0, 0, 0);
        Rect _btnSelfRect  = new Rect(0, 0, 0, 0);
        Rect _modeRect     = new Rect(0, 0, 0, 0);

        GUIStyle  _infoStyle;
        GUIStyle  _btnStyle;
        bool      _styleBuilt;
        Texture2D _whiteTex;

        static PropertyInfo _piWidgetW;
        static PropertyInfo _piWidgetH;
        static bool         _widgetReflDone;

        const float SCAN_RADIUS_PX = 150f;
        const float SAME_TAP_PX    = 80f;

        void Awake()
        {
            _whiteTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _whiteTex.SetPixel(0, 0, Color.white);
            _whiteTex.Apply();
        }

        static void EnsureWidgetRefl()
        {
            if (_widgetReflDone) return;
            _widgetReflDone = true;
            try
            {
                Type wt = typeof(UIWidget);
                _piWidgetW = wt.GetProperty("width",  BindingFlags.Public | BindingFlags.Instance);
                _piWidgetH = wt.GetProperty("height", BindingFlags.Public | BindingFlags.Instance);
            }
            catch { }
        }

        bool TryGetScreenRect(GameObject go, Camera cam, out Rect rect)
        {
            rect = new Rect(0, 0, 0, 0);

            BoxCollider box = go.GetComponent<BoxCollider>();
            if (box != null)
            {
                Vector3 ctr = box.bounds.center, ext = box.bounds.extents;
                return ProjectRect(cam,
                    new Vector3(ctr.x - ext.x, ctr.y - ext.y, ctr.z),
                    new Vector3(ctr.x + ext.x, ctr.y + ext.y, ctr.z),
                    out rect);
            }

            EnsureWidgetRefl();
            UIWidget widget = go.GetComponent<UIWidget>();
            if (widget != null && _piWidgetW != null && _piWidgetH != null)
            {
                try
                {
                    int w = (int)_piWidgetW.GetValue(widget, null);
                    int h = (int)_piWidgetH.GetValue(widget, null);
                    if (w > 0 && h > 0)
                    {
                        Transform tr = widget.transform;
                        float hw = w * 0.5f, hh = h * 0.5f;
                        return ProjectRect(cam,
                            tr.TransformPoint(-hw, -hh, 0f),
                            tr.TransformPoint( hw,  hh, 0f),
                            out rect);
                    }
                }
                catch { }
            }

            UIPanel panel = go.GetComponent<UIPanel>();
            if (panel != null)
            {
                Vector4 cr = panel.clipRange;
                if (cr.z > 0f && cr.w > 0f)
                {
                    Transform tr = panel.cachedTransform;
                    float hw = cr.z * 0.5f, hh = cr.w * 0.5f;
                    return ProjectRect(cam,
                        tr.TransformPoint(cr.x - hw, cr.y - hh, 0f),
                        tr.TransformPoint(cr.x + hw, cr.y + hh, 0f),
                        out rect);
                }
            }

            return false;
        }

        bool ProjectRect(Camera cam, Vector3 worldBL, Vector3 worldTR, out Rect rect)
        {
            rect = new Rect(0, 0, 0, 0);
            Vector3 s0 = cam.WorldToScreenPoint(worldBL);
            Vector3 s1 = cam.WorldToScreenPoint(worldTR);
            float minX = Mathf.Min(s0.x, s1.x), maxX = Mathf.Max(s0.x, s1.x);
            float minY = Mathf.Min(s0.y, s1.y), maxY = Mathf.Max(s0.y, s1.y);
            float guiH = maxY - minY;
            if (guiH < 1f || (maxX - minX) < 1f) return false;
            rect = new Rect(minX, Screen.height - maxY, maxX - minX, guiH);
            return true;
        }

        // ── Toggle all direct children of a GO ────────────────────────────────
        void ToggleChildren(GameObject go)
        {
            if (_toggleTarget == go && _childrenHidden)
            {
                // Restore
                foreach (GameObject child in _hiddenChildren)
                    if (child != null) child.SetActive(true);
                _hiddenChildren.Clear();
                _childrenHidden = false;
                _toggleTarget   = null;
                Debug.Log("[CNRDbg] Restored children of " + go.name);
            }
            else
            {
                // Restore any previous toggle first
                if (_toggleTarget != null && _childrenHidden)
                {
                    foreach (GameObject child in _hiddenChildren)
                        if (child != null) child.SetActive(true);
                    _hiddenChildren.Clear();
                }
                // Hide active children
                _hiddenChildren.Clear();
                for (int i = 0; i < go.transform.childCount; i++)
                {
                    GameObject child = go.transform.GetChild(i).gameObject;
                    if (child.activeSelf)
                    {
                        child.SetActive(false);
                        _hiddenChildren.Add(child);
                    }
                }
                _childrenHidden = true;
                _toggleTarget   = go;
                Debug.Log("[CNRDbg] Hid " + _hiddenChildren.Count + " children of " + go.name);
            }
        }

        void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;

            Camera cam = UICamera.mainCamera;
            if (cam == null) { Debug.Log("[CNRDbg] UICamera.mainCamera is null"); return; }

            Vector2 tapScreen = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            _tapDot = new Vector2(tapScreen.x, Screen.height - tapScreen.y);

            // Ignore taps on the UI buttons
            if (_btnRect.width > 0f && _btnRect.Contains(_tapDot)) return;
            if (_btnSelfRect.width > 0f && _btnSelfRect.Contains(_tapDot)) return;
            if (_modeRect.width > 0f && _modeRect.Contains(_tapDot)) return;

            if (_btnMode)
                FindButtonCandidates(cam, tapScreen);
            else
                FindHierarchyCandidates(cam, tapScreen);
        }

        void FindButtonCandidates(Camera cam, Vector2 tapScreen)
        {
            // Use FindObjectsOfTypeAll to include inactive GOs.
            // Find every BoxCollider whose projected screen rect contains the tap point.
            var allColliders = (BoxCollider[])Resources.FindObjectsOfTypeAll(typeof(BoxCollider));
            var hits = new List<BoxCollider>();
            Vector2 tapGui = new Vector2(tapScreen.x, Screen.height - tapScreen.y);
            foreach (BoxCollider box in allColliders)
            {
                Rect r;
                if (!TryGetScreenRect(box.gameObject, cam, out r)) continue;
                if (r.Contains(tapGui)) hits.Add(box);
            }

            // Sort smallest area first — most specific button wins
            hits.Sort(delegate(BoxCollider a, BoxCollider b)
            {
                Rect ra, rb;
                TryGetScreenRect(a.gameObject, cam, out ra);
                TryGetScreenRect(b.gameObject, cam, out rb);
                float areA = ra.width * ra.height;
                float areB = rb.width * rb.height;
                return areA.CompareTo(areB);
            });

            bool sameTap = _hasTap && Vector2.Distance(tapScreen, _lastTapScreen) < SAME_TAP_PX;
            if (sameTap && hits.Count > 0 && _candidates != null && _candidates.Count > 0)
            {
                _candidateIdx = (_candidateIdx + 1) % hits.Count;
            }
            else
            {
                _hasTap        = true;
                _lastTapScreen = tapScreen;
                _candidateIdx  = 0;
            }

            _candidates = new List<Transform>();
            foreach (var box in hits) _candidates.Add(box.transform);

            if (_candidates.Count == 0) { _hasHL = false; _hasElemDot = false; _info = "No BoxColliders at tap point."; return; }
            ShowButtonCandidate(cam, tapScreen);
        }

        void ShowButtonCandidate(Camera cam, Vector2 tapScreen)
        {
            if (_candidates == null || _candidates.Count == 0) return;
            if (_candidateIdx >= _candidates.Count) _candidateIdx = 0;
            Transform  t  = _candidates[_candidateIdx];
            if (t == null) return;
            GameObject go = t.gameObject;

            Vector3 wsp = cam.WorldToScreenPoint(t.position);
            _elemDot    = new Vector2(wsp.x, Screen.height - wsp.y);
            _hasElemDot = wsp.z > 0f;

            Rect r; _hasHL = TryGetScreenRect(go, cam, out r); _hlRect = r;

            var parts = new List<string>();
            for (Transform c = t; c != null; c = c.parent) parts.Add(c.name);
            parts.Reverse();
            string pathStr = string.Join("/", parts.ToArray());

            // Collect interesting button components
            var compSb = new StringBuilder();
            foreach (Component comp in go.GetComponents<Component>())
            {
                if (comp == null) continue;
                string typeName = comp.GetType().Name;
                compSb.Append("\n  ").Append(typeName);
                // UIStoreBtnEvent.buttonName via reflection (avoid hard reference)
                var fi = comp.GetType().GetField("buttonName", BindingFlags.Public | BindingFlags.Instance);
                if (fi != null) compSb.Append(": buttonName=").Append(fi.GetValue(comp));
                UILabel lbl = comp as UILabel; if (lbl != null) compSb.Append(": \"").Append(lbl.text ?? "").Append('"');
                UIImageButton ib = comp as UIImageButton; if (ib != null) compSb.Append(" [UIImageButton]");
            }

            string rectStr = _hasHL
                ? string.Format("x={0:F0} y={1:F0} w={2:F0} h={3:F0}", _hlRect.x, _hlRect.y, _hlRect.width, _hlRect.height)
                : "(no bounds)";

            _info = string.Format(
                "BUTTONS MODE [{0}/{1}]  {2}\n" +
                "PATH:  {3}\n" +
                "WORLD: ({4:F2},{5:F2},{6:F2})  screenPt=({7:F0},{8:F0})\n" +
                "RECT:  {9}\n" +
                "COMPS:{10}\n" +
                "[tap same=next]",
                _candidateIdx + 1, _candidates.Count, go.name,
                pathStr,
                t.position.x, t.position.y, t.position.z, wsp.x, Screen.height - wsp.y,
                rectStr, compSb.ToString());

            Debug.Log("[CNRDbg] " + _info.Replace('\n', ' '));
        }

        void FindHierarchyCandidates(Camera cam, Vector2 tapScreen)
        {
            bool sameTap = _hasTap && Vector2.Distance(tapScreen, _lastTapScreen) < SAME_TAP_PX;
            if (sameTap && _candidates != null && _candidates.Count > 1)
            {
                _candidateIdx = (_candidateIdx + 1) % _candidates.Count;
                ShowCandidate(cam, tapScreen);
                return;
            }

            _hasTap        = true;
            _lastTapScreen = tapScreen;
            _candidateIdx  = 0;

            var directHits = new List<Transform>();
            object[] allObjs = FindObjectsOfType(typeof(Transform));
            foreach (object obj in allObjs)
            {
                Transform t = obj as Transform;
                if (t == null) continue;
                Vector3 sp = cam.WorldToScreenPoint(t.position);
                if (sp.z < 0f) continue;
                float dx = sp.x - tapScreen.x, dy = sp.y - tapScreen.y;
                if (dx * dx + dy * dy < SCAN_RADIUS_PX * SCAN_RADIUS_PX)
                    directHits.Add(t);
            }

            directHits.Sort(delegate(Transform a, Transform b)
            {
                Vector3 sa = cam.WorldToScreenPoint(a.position);
                Vector3 sb = cam.WorldToScreenPoint(b.position);
                float da = (sa.x - tapScreen.x) * (sa.x - tapScreen.x)
                         + (sa.y - tapScreen.y) * (sa.y - tapScreen.y);
                float db = (sb.x - tapScreen.x) * (sb.x - tapScreen.x)
                         + (sb.y - tapScreen.y) * (sb.y - tapScreen.y);
                return da.CompareTo(db);
            });

            var seen = new HashSet<int>(); var combined = new List<Transform>();
            foreach (Transform t in directHits) if (seen.Add(t.gameObject.GetInstanceID())) combined.Add(t);
            foreach (Transform t in directHits)
            {
                Transform p = t.parent;
                while (p != null) { if (seen.Add(p.gameObject.GetInstanceID())) combined.Add(p); p = p.parent; }
            }
            _candidates = combined;

            if (_candidates.Count == 0)
            {
                _hasHL = false; _hasElemDot = false;
                _info  = "No GOs within " + (int)SCAN_RADIUS_PX + "px.";
                return;
            }
            ShowCandidate(cam, tapScreen);
        }

        void ShowCandidate(Camera cam, Vector2 tapScreen)
        {
            if (_candidates == null || _candidates.Count == 0) return;
            while (_candidateIdx < _candidates.Count && _candidates[_candidateIdx] == null) _candidateIdx++;
            if (_candidateIdx >= _candidates.Count) { _candidateIdx = 0; return; }

            Transform  t  = _candidates[_candidateIdx];
            GameObject go = t.gameObject;

            Vector3 wsp = cam.WorldToScreenPoint(t.position);
            _elemDot    = new Vector2(wsp.x, Screen.height - wsp.y);
            _hasElemDot = wsp.z > 0f;

            Rect r;
            _hasHL  = TryGetScreenRect(go, cam, out r);
            _hlRect = r;

            var parts = new List<string>();
            for (Transform c = t; c != null; c = c.parent) parts.Add(c.name);
            parts.Reverse();
            string pathStr = string.Join("/", parts.ToArray());

            var compSb = new StringBuilder();
            foreach (Component comp in go.GetComponents<Component>())
            {
                if (comp == null) continue;
                compSb.Append("\n  ").Append(comp.GetType().Name);
                UILabel  lbl = comp as UILabel;  if (lbl != null) compSb.Append(": \"").Append(lbl.text ?? "").Append('"');
                UISprite spr = comp as UISprite; if (spr != null) compSb.Append(": sprite=\"").Append(spr.spriteName ?? "").Append('"');
                UIPanel  pan = comp as UIPanel;
                if (pan != null) { Vector4 cr = pan.clipRange; compSb.Append(string.Format(": clip=({0:F0},{1:F0},{2:F0},{3:F0})", cr.x, cr.y, cr.z, cr.w)); }
                UIWidget wid = comp as UIWidget;
                if (wid != null && _piWidgetW != null && _piWidgetH != null)
                    try { compSb.Append(string.Format(": size={0}x{1}", (int)_piWidgetW.GetValue(wid,null), (int)_piWidgetH.GetValue(wid,null))); } catch { }
            }

            int di = 0;
            foreach (Transform d in _candidates)
            {
                Vector3 sp = cam.WorldToScreenPoint(d.position);
                float dx = sp.x - tapScreen.x, dy = sp.y - tapScreen.y;
                if (dx * dx + dy * dy < SCAN_RADIUS_PX * SCAN_RADIUS_PX) di++; else break;
            }
            string depthTag = _candidateIdx < di ? "[direct]" : "[ancestor +" + (_candidateIdx - di + 1) + "]";

            string rectStr = _hasHL
                ? string.Format("x={0:F0} y={1:F0} w={2:F0} h={3:F0}", _hlRect.x, _hlRect.y, _hlRect.width, _hlRect.height)
                : "(no bounds)";

            string childToggleLabel = (_toggleTarget == go && _childrenHidden)
                ? "SHOW " + _hiddenChildren.Count + " CHILDREN"
                : "HIDE " + go.transform.childCount + " CHILDREN";

            _info = string.Format(
                "[{0}/{1}] {2}  {3}\n" +
                "PATH:  {4}\n" +
                "WORLD: ({5:F2},{6:F2},{7:F2})  screenPt=({8:F0},{9:F0})\n" +
                "RECT:  {10}\n" +
                "COMPS:{11}\n" +
                "[tap same area=next | tap elsewhere=new]  [{12}]",
                _candidateIdx + 1, _candidates.Count, go.name, depthTag,
                pathStr,
                t.position.x, t.position.y, t.position.z, wsp.x, Screen.height - wsp.y,
                rectStr, compSb.ToString(), childToggleLabel);

            Debug.Log("[CNRDbg] " + _info.Replace('\n', ' '));
        }

        void OnGUI()
        {
            if (!_styleBuilt)
            {
                _infoStyle = new GUIStyle(GUI.skin.label);
                _infoStyle.alignment = TextAnchor.UpperLeft;
                _infoStyle.wordWrap  = true;
                _infoStyle.fontSize  = Mathf.Max(13, Screen.height / 55);
                _infoStyle.normal.textColor = Color.white;

                _btnStyle = new GUIStyle(GUI.skin.button);
                _btnStyle.fontSize  = Mathf.Max(14, Screen.height / 48);
                _btnStyle.fontStyle = FontStyle.Bold;

                _styleBuilt = true;
            }
            if (_whiteTex == null) return;

            // Mode-toggle button — top-right corner
            float mbH = Screen.height * 0.065f;
            float mbW = Screen.width  * 0.38f;
            _modeRect = new Rect(Screen.width - mbW - 8f, 8f, mbW, mbH);
            Color prev0 = GUI.color;
            GUI.color = _btnMode ? new Color(0.2f, 0.8f, 1f) : new Color(1f, 0.75f, 0.2f);
            if (GUI.Button(_modeRect, _btnMode ? "MODE: BUTTONS" : "MODE: HIERARCHY", _btnStyle))
            {
                _btnMode = !_btnMode;
                _candidates = null; _info = ""; _hasHL = false; _hasElemDot = false;
            }
            GUI.color = prev0;

            // Yellow crosshair — tap point
            if (_hasTap)
            {
                Color prev = GUI.color; GUI.color = Color.yellow;
                float d = 20f;
                GUI.DrawTexture(new Rect(_tapDot.x - d * 0.5f, _tapDot.y - 2f, d, 4f), _whiteTex);
                GUI.DrawTexture(new Rect(_tapDot.x - 2f, _tapDot.y - d * 0.5f, 4f, d), _whiteTex);
                GUI.color = prev;
            }

            // Cyan dot — selected element position
            if (_hasElemDot)
            {
                Color prev = GUI.color; GUI.color = Color.cyan;
                float r = 10f;
                GUI.DrawTexture(new Rect(_elemDot.x - r, _elemDot.y - r, r * 2f, r * 2f), _whiteTex);
                GUI.color = prev;
            }

            // Red border — element bounds
            if (_hasHL)
            {
                Color prev = GUI.color; GUI.color = new Color(1f, 0.15f, 0.15f, 0.95f);
                Rect rl = _hlRect; float b = 4f;
                GUI.DrawTexture(new Rect(rl.xMin,     rl.yMin,     rl.width, b),        _whiteTex);
                GUI.DrawTexture(new Rect(rl.xMin,     rl.yMax - b, rl.width, b),        _whiteTex);
                GUI.DrawTexture(new Rect(rl.xMin,     rl.yMin,     b,       rl.height), _whiteTex);
                GUI.DrawTexture(new Rect(rl.xMax - b, rl.yMin,     b,       rl.height), _whiteTex);
                GUI.color = prev;
            }

            // Info panel + buttons — anchored from top so buttons never overflow
            if (_info.Length > 0)
            {
                float btnH = Screen.height * 0.07f;
                float pw   = Screen.width  * 0.96f;
                float ph   = Screen.height * 0.40f;  // text area
                float px   = Screen.width  * 0.02f;
                float py   = Screen.height * 0.02f;  // start near top

                Color prev = GUI.color;
                GUI.color = new Color(0f, 0f, 0f, 0.85f);
                GUI.DrawTexture(new Rect(px - 8f, py - 8f, pw + 16f, ph + btnH + 24f), _whiteTex);

                GUI.color = Color.white;
                GUI.Label(new Rect(px + 8f, py + 4f, pw - 16f, ph - 8f), _info, _infoStyle);

                if (_candidates != null && _candidateIdx < _candidates.Count && _candidates[_candidateIdx] != null)
                {
                    GameObject cur = _candidates[_candidateIdx].gameObject;

                    // Left button: HIDE/SHOW CHILDREN
                    bool childrenHidden = (_toggleTarget == cur && _childrenHidden);
                    string childBtnText = childrenHidden
                        ? "SHOW " + _hiddenChildren.Count + " CHILDREN"
                        : "HIDE " + cur.transform.childCount + " CHILDREN";
                    float halfW = (pw - 16f - 8f) * 0.5f;
                    _btnRect = new Rect(px + 8f, py + ph + 4f, halfW, btnH);
                    GUI.color = childrenHidden ? Color.green : new Color(1f, 0.4f, 0.4f);
                    if (GUI.Button(_btnRect, childBtnText, _btnStyle))
                        ToggleChildren(cur);

                    // Right button: HIDE/SHOW SELF
                    bool selfHidden = (_selfHiddenGO == cur);
                    string selfBtnText = selfHidden ? "SHOW SELF" : "HIDE SELF";
                    _btnSelfRect = new Rect(px + 8f + halfW + 8f, py + ph + 4f, halfW, btnH);
                    GUI.color = selfHidden ? new Color(0.4f, 1f, 0.4f) : new Color(0.4f, 0.6f, 1f);
                    if (GUI.Button(_btnSelfRect, selfBtnText, _btnStyle))
                        ToggleSelf(cur);
                }
                GUI.color = prev;
            }
        }
    }
}
