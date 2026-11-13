using UnityEngine;
using UnityEngine.UI;

// Adicione ao Player. Cria sozinha o overlay da mira (não precisa de Canvas na cena).
// Os tiros saem retos, na direção do trilho, então a mira fica exatamente nessa linha.
[DefaultExecutionOrder(100)] // roda depois da câmera, para não ficar um frame atrasada
public class Crosshair : MonoBehaviour
{
    [Tooltip("Distância à frente em que a mira é desenhada. Use a distância típica dos inimigos")]
    public float aimDistance = 85f;
    public float size = 56f;
    public Color normalColor = new Color(0.3f, 1f, 0.4f, 0.9f);
    public Color enemyColor = new Color(1f, 0.25f, 0.25f, 1f);
    public float colorSmooth = 15f;

    [Tooltip("Raio da checagem de inimigo na linha do tiro")]
    public float detectRadius = 1.5f;
    public float detectDistance = 200f;

    PlayerShip ship;
    Camera cam;
    GameObject canvasObject;
    Image image;
    RectTransform rect;

    static readonly RaycastHit[] hitBuffer = new RaycastHit[16];

    void Start()
    {
        ship = GetComponent<PlayerShip>();
        cam = Camera.main;
        BuildOverlay();
    }

    void OnDestroy()
    {
        if (canvasObject != null) Destroy(canvasObject);
    }

    void LateUpdate()
    {
        if (ship == null || image == null) return;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 origin = ship.firePoint != null ? ship.firePoint.position : ship.transform.position;
        Vector3 direction = ship.RailForward;

        // Posição da mira na tela: o ponto da linha do tiro a 'aimDistance'.
        Vector3 screen = cam.WorldToScreenPoint(origin + direction * aimDistance);
        bool visible = screen.z > 0f;
        image.enabled = visible;
        if (!visible) return;

        rect.position = new Vector3(screen.x, screen.y, 0f);

        // Fica vermelha quando há um inimigo na linha do tiro.
        bool overEnemy = IsEnemyInLine(origin, direction);
        Color target = overEnemy ? enemyColor : normalColor;
        image.color = Color.Lerp(image.color, target, colorSmooth * Time.deltaTime);
    }

    bool IsEnemyInLine(Vector3 origin, Vector3 direction)
    {
        int count = Physics.SphereCastNonAlloc(origin, detectRadius, direction, hitBuffer,
                                               detectDistance, ~0, QueryTriggerInteraction.Collide);

        for (int i = 0; i < count; i++)
        {
            if (hitBuffer[i].collider.GetComponentInParent<EnemyShip>() != null) return true;
        }
        return false;
    }

    void BuildOverlay()
    {
        canvasObject = new GameObject("CrosshairCanvas", typeof(Canvas));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        GameObject imageObject = new GameObject("Crosshair", typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(canvasObject.transform, false);

        rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(size, size);

        image = imageObject.GetComponent<Image>();
        image.sprite = CreateCrosshairSprite();
        image.color = normalColor;
        image.raycastTarget = false;
    }

    // Anel + pontinho central + quatro traços, desenhados em uma textura.
    Sprite CreateCrosshairSprite()
    {
        const int s = 128;
        Texture2D tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        float c = (s - 1) * 0.5f;

        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float dx = x - c;
                float dy = y - c;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                float a = 0f;

                // Anel
                a = Mathf.Max(a, 1f - Mathf.Clamp01(Mathf.Abs(dist - 36f) / 2.5f));
                // Ponto central
                a = Mathf.Max(a, 1f - Mathf.Clamp01((dist - 3f) / 1.5f));
                // Traços nas quatro direções (entre o anel e a borda)
                bool onHorizontal = Mathf.Abs(dy) < 1.8f && Mathf.Abs(dx) > 30f && Mathf.Abs(dx) < 52f;
                bool onVertical = Mathf.Abs(dx) < 1.8f && Mathf.Abs(dy) > 30f && Mathf.Abs(dy) < 52f;
                if (onHorizontal || onVertical) a = 1f;

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f));
    }
}