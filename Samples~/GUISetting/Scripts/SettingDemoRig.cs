using NonsensicalKit.Core.Service.Setting;
using NonsensicalKit.Setting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 场景示例：把项目设置映到方块、标记、附加光和屏幕文字上。
/// </summary>
public class SettingDemoRig : SettingListenerBase
{
    [SerializeField] private Transform m_subject;
    [SerializeField] private Renderer m_subjectRenderer;
    [SerializeField] private Transform m_marker;
    [SerializeField] private GameObject m_extraLights;
    [SerializeField] private Light[] m_pointLights;
    [SerializeField] private Text m_caption;
    [SerializeField] private Text m_status;
    [SerializeField] private Renderer m_groundRenderer;
    [SerializeField] private AudioSource m_audio;

    private float m_spin = 45f;
    private float m_baseScale = 1f;
    private float m_pulseUntil;
    private string m_colorName = "white";
    private bool m_showMarker = true;
    private float m_markerHeight = 2.2f;
    private bool m_extraOn = true;
    private float m_lightIntensity = 1.5f;
    private string m_captionText = "设置演示";

    protected override void Awake()
    {
        base.Awake();
        if ((m_pointLights == null || m_pointLights.Length == 0) && m_extraLights != null)
        {
            m_pointLights = m_extraLights.GetComponentsInChildren<Light>(true);
        }

        PrepareGround();
        PrepareTone();
        ApplyVisuals();
    }

    private void Update()
    {
        if (m_audio != null && !m_audio.isPlaying && (Input.GetMouseButtonDown(0) || Input.anyKeyDown))
        {
            m_audio.Play();
        }

        if (m_subject == null)
        {
            return;
        }

        float scale = m_baseScale;
        if (Time.time < m_pulseUntil)
        {
            float k = (m_pulseUntil - Time.time) / 0.35f;
            scale *= 1f + 0.28f * k;
        }

        m_subject.localScale = Vector3.one * scale;
        m_subject.Rotate(0f, m_spin * Time.deltaTime, 0f, Space.World);
    }

    protected override void OnSettingValueChanged(string key, SettingValue value)
    {
        switch (key)
        {
            case "project.color":
                m_colorName = value.AsString();
                break;
            case "project.spinSpeed":
                m_spin = value.AsFloat();
                break;
            case "project.scale":
                m_baseScale = Mathf.Max(0.05f, value.AsFloat());
                break;
            case "project.caption":
                m_captionText = value.AsString();
                break;
            case "project.showMarker":
                m_showMarker = value.AsBool();
                break;
            case "project.markerHeight":
                m_markerHeight = value.AsFloat();
                break;
            case "project.extraLights":
                m_extraOn = value.AsBool();
                break;
            case "project.lightIntensity":
                m_lightIntensity = value.AsFloat();
                break;
        }

        ApplyVisuals();
    }

    protected override void OnButtonClick(string actionKey)
    {
        if (actionKey == "pulseSubject")
        {
            m_pulseUntil = Time.time + 0.35f;
        }
    }

    private void ApplyVisuals()
    {
        SetRendererColor(m_subjectRenderer, ColorOf(m_colorName));
        if (m_marker != null)
        {
            m_marker.gameObject.SetActive(m_showMarker);
            Vector3 p = m_marker.position;
            p.y = m_markerHeight;
            m_marker.position = p;
        }

        if (m_extraLights != null)
        {
            m_extraLights.SetActive(m_extraOn);
        }

        if (m_pointLights != null)
        {
            for (int i = 0; i < m_pointLights.Length; i++)
            {
                if (m_pointLights[i] != null)
                {
                    m_pointLights[i].intensity = m_lightIntensity;
                }
            }
        }

        if (m_caption != null)
        {
            m_caption.text = m_captionText;
        }

        if (m_status != null)
        {
            m_status.text = "颜色 " + m_colorName
                            + "    转速 " + m_spin.ToString("0")
                            + "    缩放 " + m_baseScale.ToString("0.00")
                            + "    标记 " + (m_showMarker ? "开 " + m_markerHeight.ToString("0.0") + "m" : "关")
                            + "    附加光 " + (m_extraOn ? m_lightIntensity.ToString("0.0") : "关");
        }
    }

    private void PrepareGround()
    {
        if (m_groundRenderer == null)
        {
            return;
        }

        const int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGB24, true);
        Color dark = new Color(0.18f, 0.2f, 0.22f);
        Color light = new Color(0.82f, 0.84f, 0.86f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool on = ((x / 8) + (y / 8)) % 2 == 0;
                tex.SetPixel(x, y, on ? light : dark);
            }
        }

        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Trilinear;
        tex.anisoLevel = 16;
        tex.Apply(true);

        Material mat = m_groundRenderer.material;
        mat.mainTexture = tex;
        if (mat.HasProperty("_BaseMap"))
        {
            mat.SetTexture("_BaseMap", tex);
            mat.SetTextureScale("_BaseMap", new Vector2(48f, 48f));
        }

        if (mat.HasProperty("_MainTex"))
        {
            mat.SetTextureScale("_MainTex", new Vector2(48f, 48f));
        }
    }

    private void PrepareTone()
    {
        if (m_audio == null)
        {
            m_audio = gameObject.AddComponent<AudioSource>();
        }

        const int rate = 44100;
        int length = rate * 2;
        float[] data = new float[length];
        for (int i = 0; i < length; i++)
        {
            float t = i / (float)rate;
            float gate = (t % 0.9f) < 0.1f ? 0.22f : 0f;
            data[i] = Mathf.Sin(2f * Mathf.PI * 523.25f * t) * gate;
        }

        AudioClip clip = AudioClip.Create("SettingDemoTone", length, 1, rate, false);
        clip.SetData(data, 0);
        m_audio.clip = clip;
        m_audio.loop = true;
        m_audio.playOnAwake = true;
        m_audio.spatialBlend = 0f;
        m_audio.volume = 1f;
        m_audio.Play();
    }

    private static void SetRendererColor(Renderer renderer, Color color)
    {
        if (renderer == null)
        {
            return;
        }

        Material mat = renderer.material;
        mat.color = color;
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", color);
        }
    }

    private static Color ColorOf(string name)
    {
        switch (name)
        {
            case "red":
                return new Color(0.86f, 0.22f, 0.18f);
            case "green":
                return new Color(0.2f, 0.72f, 0.32f);
            case "blue":
                return new Color(0.2f, 0.45f, 0.9f);
            case "yellow":
                return new Color(0.95f, 0.78f, 0.15f);
            default:
                return new Color(0.92f, 0.92f, 0.92f);
        }
    }
}
