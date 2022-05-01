using EnhancedStreamChat.Utilities;
using HMUI;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace EnhancedStreamChat.Graphics
{
    public class EnhancedTextMeshProUGUIWithBackground : MonoBehaviour, IInitializable
    {
        public EnhancedTextMeshProUGUI Text { get; internal set; }
        public EnhancedTextMeshProUGUI SubText { get; internal set; }

        public DateTime ReceivedDate { get; internal set; }

        public event Action OnLatePreRenderRebuildComplete;

        private ImageView _highlight;
        private ImageView _accent;
        private VerticalLayoutGroup _verticalLayoutGroup;
        private MemoryPoolContainer<EnhancedTextMeshProUGUI> _textContainer;

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
                this.SubText.enabled = value;
                if (value) {
                    this.SubText.rectTransform.SetParent(this.gameObject.transform, false);
                }
                else {
                    this.SubText.rectTransform.SetParent(null, false);
                }
            }
        }

        [Inject]
        public void Constact(EnhancedTextMeshProUGUI.Pool pool)
        {
            this._textContainer = new MemoryPoolContainer<EnhancedTextMeshProUGUI>(pool);
        }

        private void OnDestroy()
        {
            try {
                this.Text.OnLatePreRenderRebuildComplete -= this.Text_OnLatePreRenderRebuildComplete;
                this.SubText.OnLatePreRenderRebuildComplete -= this.Text_OnLatePreRenderRebuildComplete;
            }
            catch (Exception) {
            }
        }

        private void Text_OnLatePreRenderRebuildComplete()
        {
            (this._accent.gameObject.transform as RectTransform).sizeDelta = new Vector2(1, (this.transform as RectTransform).sizeDelta.y);
            OnLatePreRenderRebuildComplete?.Invoke();
        }

        public void Initialize()
        {
            this._highlight = this.gameObject.GetComponent<ImageView>();
            this._highlight.raycastTarget = false;
            this._highlight.material = BeatSaberUtils.UINoGlowMaterial;
            
            this.Text = this._textContainer.Spawn();
            this.Text.OnLatePreRenderRebuildComplete += this.Text_OnLatePreRenderRebuildComplete;

            this.SubText = this._textContainer.Spawn();
            this.SubText.OnLatePreRenderRebuildComplete += this.Text_OnLatePreRenderRebuildComplete;

            this._accent = new GameObject().AddComponent<ImageView>();
            this._accent.raycastTarget = false;
            DontDestroyOnLoad(this._accent.gameObject);
            this._accent.material = BeatSaberUtils.UINoGlowMaterial;
            this._accent.color = Color.yellow;

            this._verticalLayoutGroup = this.gameObject.GetComponent<VerticalLayoutGroup>();
            this._verticalLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
            this._verticalLayoutGroup.spacing = 1;

            var highlightFitter = this._accent.gameObject.AddComponent<LayoutElement>();
            highlightFitter.ignoreLayout = true;
            var textFitter = this.Text.gameObject.AddComponent<ContentSizeFitter>();
            textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var backgroundFitter = this.gameObject.GetComponent<ContentSizeFitter>();
            backgroundFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            this.SubTextEnabled = false;
            this.HighlightEnabled = false;
            this.AccentEnabled = false;
            this._accent.gameObject.transform.SetParent(this.gameObject.transform, false);
            (this._accent.gameObject.transform as RectTransform).anchorMin = new Vector2(0, 0.5f);
            (this._accent.gameObject.transform as RectTransform).anchorMax = new Vector2(0, 0.5f);
            (this._accent.gameObject.transform as RectTransform).sizeDelta = new Vector2(1, 10);
            (this._accent.gameObject.transform as RectTransform).pivot = new Vector2(0, 0.5f);
            this.Text.rectTransform.SetParent(this.gameObject.transform, false);
        }

        public class Pool : MonoMemoryPool<EnhancedTextMeshProUGUIWithBackground>
        {
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
                base.OnDespawned(msg);
                msg.gameObject.SetActive(false);
                (msg.transform as RectTransform).localPosition = Vector3.zero;
                msg.HighlightEnabled = false;
                msg.AccentEnabled = false;
                msg.SubTextEnabled = false;
                msg.Text.text = "";
                msg.Text.ChatMessage = null;
                msg.SubText.text = "";
                msg.SubText.ChatMessage = null;
                msg.Text.ClearImages();
                msg.SubText.ClearImages();
            }
        }
    }
}
