using EnhancedStreamChat.Interfaces;
using EnhancedStreamChat.Utilities;
using HMUI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace EnhancedStreamChat.Graphics
{
    public class EnhancedTextMeshProUGUIWithBackground : MonoBehaviour, ILatePreRenderRebuildReciver
    {
        public EnhancedTextMeshProUGUI Text { get; internal set; }
        public EnhancedTextMeshProUGUI SubText { get; internal set; }

        public DateTime ReceivedDate { get; internal set; }

        private bool _rebuiled = false;
        private readonly LazyCopyHashSet<ILatePreRenderRebuildReciver> _recivers = new LazyCopyHashSet<ILatePreRenderRebuildReciver>();
        public ILazyCopyHashSet<ILatePreRenderRebuildReciver> LazyCopyHashSet => this._recivers;

        private ImageView _highlight;
        private ImageView _accent;
        private VerticalLayoutGroup _verticalLayoutGroup;
        private EnhancedTextMeshProUGUI.Factory _factory;
        private WaitWhile _waitNoGlowUINull;

        public Vector2 Size
        {
            get => (this.transform as RectTransform).sizeDelta;
            set => (this.transform as RectTransform).sizeDelta = value;
        }

        public Color AccentColor
        {
            get => this._accent.color;
            set => this._accent.color = value;
        }

        public Color HighlightColor
        {
            get => this._highlight.color;
            set => this._highlight.color = value;
        }

        public bool HighlightEnabled
        {
            get => this._highlight.enabled;
            set
            {
                this._highlight.enabled = value;
                if (value) {
                    this._verticalLayoutGroup.padding = new RectOffset(5, 5, 2, 2);
                }
                else {
                    this._verticalLayoutGroup.padding = new RectOffset(5, 5, 1, 1);
                }
            }
        }

        public bool AccentEnabled
        {
            get => this._accent.enabled;
            set => this._accent.enabled = value;
        }

        public bool SubTextEnabled
        {
            get => this.SubText.enabled;
            set
            {
                if (this.SubText == null) {
                    return;
                }
                this.SubText.enabled = value;
                if (value) {
                    this.SubText.rectTransform.SetParent(this.gameObject.transform, false);
                }
                else {
                    this.SubText.rectTransform.SetParent(null, false);
                }
            }
        }

        private void Awake()
        {
            this._waitNoGlowUINull = new WaitWhile(() => BeatSaberUtils.UINoGlowMaterial == null || this._highlight == null || this._accent == null);
        }

        private IEnumerator Start()
        {
            yield return this._waitNoGlowUINull;
            this._highlight.material = BeatSaberUtils.UINoGlowMaterial;
            this._accent.material = BeatSaberUtils.UINoGlowMaterial;
        }

        [Inject]
        public void Constract(EnhancedTextMeshProUGUI.Factory factory)
        {
            this._factory = factory;
        }

        protected void OnDestroy()
        {
            try {
                this.Text.RemoveReciver(this);
                this.SubText.RemoveReciver(this);
                Destroy(this.Text);
                Destroy(this.SubText);
            }
            catch (Exception) {
            }
        }

        public void AddReciver(ILatePreRenderRebuildReciver reciver)
        {
            this.LazyCopyHashSet.Add(reciver);
        }

        public void RemoveReciver(ILatePreRenderRebuildReciver reciver)
        {
            this.LazyCopyHashSet.Remove(reciver);
        }

        public void LatePreRenderRebuildHandler(object sender, EventArgs e)
        {
            (this._accent.gameObject.transform as RectTransform).sizeDelta = new Vector2(1, (this.transform as RectTransform).sizeDelta.y);
            this._rebuiled = true;
        }

        protected void Update()
        {
            if (this._rebuiled) {
                foreach (var reciver in this._recivers.items) {
                    reciver?.LatePreRenderRebuildHandler(this, EventArgs.Empty);
                }
                this._rebuiled = false;
            }
        }

        public class Pool : MonoMemoryPool<EnhancedTextMeshProUGUIWithBackground>
        {
            protected override void OnCreated(EnhancedTextMeshProUGUIWithBackground item)
            {
                base.OnCreated(item);
                item._highlight = item.gameObject.GetComponent<ImageView>();
                item._highlight.raycastTarget = false;

                item.Text = item._factory.Create();
                item.Text.AddReciver(item);
                item.SubText = item._factory.Create();
                item.SubText.AddReciver(item);

                item._accent = new GameObject().AddComponent<ImageView>();
                item._accent.raycastTarget = false;
                item._accent.color = Color.yellow;

                item._verticalLayoutGroup = item.gameObject.GetComponent<VerticalLayoutGroup>();
                item._verticalLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
                item._verticalLayoutGroup.spacing = 1;

                var highlightFitter = item._accent.gameObject.AddComponent<LayoutElement>();
                highlightFitter.ignoreLayout = true;
                var textFitter = item.Text.gameObject.AddComponent<ContentSizeFitter>();
                textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                var backgroundFitter = item.gameObject.GetComponent<ContentSizeFitter>();
                backgroundFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                item.SubTextEnabled = false;
                item.HighlightEnabled = false;
                item.AccentEnabled = false;
                item._accent.gameObject.transform.SetParent(item.gameObject.transform, false);
                (item._accent.gameObject.transform as RectTransform).anchorMin = new Vector2(0, 0.5f);
                (item._accent.gameObject.transform as RectTransform).anchorMax = new Vector2(0, 0.5f);
                (item._accent.gameObject.transform as RectTransform).sizeDelta = new Vector2(1, 10);
                (item._accent.gameObject.transform as RectTransform).pivot = new Vector2(0, 0.5f);
                item.Text.rectTransform.SetParent(item.gameObject.transform, false);
            }
            protected override void OnSpawned(EnhancedTextMeshProUGUIWithBackground item)
            {
                base.OnSpawned(item);
                if (item._highlight.material != BeatSaberUtils.UINoGlowMaterial) {
                    item._highlight.material = BeatSaberUtils.UINoGlowMaterial;
                }
                if (item._accent.material != BeatSaberUtils.UINoGlowMaterial) {
                    item._accent.material = BeatSaberUtils.UINoGlowMaterial;
                }
            }
            protected override void Reinitialize(EnhancedTextMeshProUGUIWithBackground msg)
            {
                base.Reinitialize(msg);
                msg.Text.autoSizeTextContainer = false;
                msg.SubText.enableWordWrapping = true;
                msg.SubText.autoSizeTextContainer = false;
                (msg.transform as RectTransform).pivot = new Vector2(0.5f, 0);
            }

            protected override void OnDespawned(EnhancedTextMeshProUGUIWithBackground msg)
            {
                if (msg == null || msg.gameObject == null) {
                    return;
                }
                base.OnDespawned(msg);
                msg.HighlightEnabled = false;
                msg.AccentEnabled = false;
                msg.SubTextEnabled = false;
                msg.Text.text = "";
                msg.Text.ChatMessage = null;
                msg.Text.SetAllDirty();
                msg.SubText.text = "";
                msg.SubText.ChatMessage = null;
                msg.SubText.SetAllDirty();
            }
        }
    }
}
