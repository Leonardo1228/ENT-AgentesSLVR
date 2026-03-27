using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))] //agrega los componentes automaticamente
[RequireComponent(typeof(SpriteRenderer))]
public class FrictionZone : MonoBehaviour
{
    public enum ZoneType { Water, Mud, Snow } //aparece como desplegable en el inspector

    [Header("Tipo de zona")]
    public ZoneType zoneType = ZoneType.Mud;

    [Header("Apariencia")]
    public float radius = 2f;
    public Color color = new Color(0.5f, 0.3f, 0f, 0.35f);

    private CircleCollider2D col;
    private SpriteRenderer sr;

    private float GetMultiplier() //define qué tan lento va
    {
        switch (zoneType)
        {
            case ZoneType.Water: return 0.3f;
            case ZoneType.Mud: return 0.5f;
            case ZoneType.Snow: return 0.7f;
            default: return 1f;
        }
    }

    private void OnValidate() //Se puede cambiar sin tener que parar la simulación
    {
        SetDefaultColor();
        if (sr != null) sr.color = color;
    }

    private void Start()
    {
        col = GetComponent<CircleCollider2D>(); //obtiene propiedad de los componentes y aplica color
        sr = GetComponent<SpriteRenderer>();

        SetDefaultColor(); //asigna color según tipo
        UpdateVisuals();
    }

    private void Update()
    {
        UpdateVisuals(); // actualiza tamaño y color
        ResetSpeeds();   // restaura velocidad base de todos
        ApplyToAll();    // aplica fricción a los que están adentro
    }

    void UpdateVisuals()
    {
        // Sincroniza collider y escala visual con el radio
        col.radius = radius; //// tamaño del collider
        transform.localScale = Vector3.one * radius * 2f; // tamaño visual del sprite
        sr.color = color; // color del sprite
    }

    void SetDefaultColor() //asigna color según tipo
    {
        switch (zoneType)
        {
            case ZoneType.Water: color = new Color(0f, 0.4f, 1f, 0.4f); break;
            case ZoneType.Mud: color = new Color(0.4f, 0.2f, 0f, 0.4f); break;
            case ZoneType.Snow: color = new Color(1f, 1f, 1f, 0.4f); break;
        }
    }

    void ResetSpeeds() 
    {
        // Restaura velocidad base antes de aplicar fricción
        foreach (Predator p in FindObjectsByType<Predator>(FindObjectsSortMode.None))
            p.speed = p.baseSpeed;

        foreach (Bunny b in FindObjectsByType<Bunny>(FindObjectsSortMode.None))
            b.speed = b.baseSpeed;
    }

    void ApplyToAll() //aplica la fricción
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D hit in hits)
        {
            Predator predator = hit.GetComponent<Predator>();
            if (predator != null && predator.isAlive)
            {
                float newSpeed = predator.baseSpeed * GetMultiplier();
                if (newSpeed < predator.speed)
                    predator.speed = newSpeed;
                continue;
            }

            Bunny bunny = hit.GetComponent<Bunny>();
            if (bunny != null)
            {
                float newSpeed = bunny.baseSpeed * GetMultiplier();
                if (newSpeed < bunny.speed)
                    bunny.speed = newSpeed;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, radius);

        Color outline = color;
        outline.a = 1f;
        Gizmos.color = outline;
        Gizmos.DrawWireSphere(transform.position, radius);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position, zoneType.ToString());
#endif
    }
}