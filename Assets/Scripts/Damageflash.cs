using UnityEngine;
using UnityEngine.UI;

// Cria sozinho um overlay de tela cheia com vinheta vermelha. Não precisa de Canvas na cena.
public class DamageFlash : MonoBehaviour
{
    public Color color = new Color(1f, 0f, 0f, 1f);
    [Range(0f, 1f)] public float maxAlpha = 0.85f;
    public float fadeDuration = 0.6f;
    [Tooltip("Onde o vermelho começa, do centro (0) até o canto (1). Maior = centro da tela mais limpo")]
    [Range(0f, 0.95f)] public float innerRadius = 0.35f;

    PlayerHealth health;
    GameObject canvasObject;
    Image image;
    float alpha;

    void Start()
    {
        BuildOverlay();

        health = GetComponent<PlayerHealth>();
        if (health != null) health.Damaged += OnDamaged;
    }

    void OnDestroy()
    {
        if (health != null) health.Damaged -= OnDamaged;
        if (canvasObject != null) Destroy(canvasObject);
    }

    void OnDamaged(int damage, int currentHealth)
    {
        alpha = maxAlpha;
    }

    void Update()
    {
        if (image == null) return;

        if (alpha > 0f)
        {
            alpha = Mathf.MoveTowards(alpha, 0f, maxAlpha / Mathf.Max(0.01f, fadeDuration) * Time.deltaTime);
        }

        Color c = color;
        c.a = alpha;
        image.color = c;
        image.enabled = alpha > 0.001f;
    }

    void BuildOverlay()
    {
        canvasObject = new GameObject("DamageFlashCanvas", typeof(Canvas));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        GameObject imageObject = new GameObject("Flash", typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        image = imageObject.GetComponent<Image>();
        image.sprite = CreateVignetteSprite();
        image.raycastTarget = false;
        image.enabled = false;
    }

    Sprite CreateVignetteSprite()
    {
        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x / (size - 1f) - 0.5f;
                float py = y / (size - 1f) - 0.5f;
                float dist = Mathf.Sqrt(px * px + py * py) * 1.4142f; // 0 no centro, 1 no canto

                float a = Mathf.Clamp01((dist - innerRadius) / (1f - innerRadius));
                a = a * a * (3f - 2f * a); // smoothstep

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}