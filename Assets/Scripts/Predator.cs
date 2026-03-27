using UnityEngine;

public class Predator : MonoBehaviour
{
    [Header("Predator Settings")]
    public float energy = 10;
    public float age = 0;
    public float maxAge = 20;
    public float speed = 1f;
    public float visionRange = 5f;

    [Header("Predator States")]
    public bool isAlive = true;
    public PredatorState currentState = PredatorState.Exploring;

    private Vector3 destination;
    private float h;

    private void Start()
    {
        destination = transform.position;
    }

    public void Simulate(float h)
    {
        if (!isAlive) return;

        this.h = h;

        switch (currentState)
        {
            case PredatorState.Exploring:
                Explore();
                break;
            case PredatorState.SearchingFood:
                SearchFood();
                break;
            case PredatorState.Eating:
                Eat();
                break;
        }

        Move();
        Age();
        CheckState();
    }

    void Explore()
    {
        // Si hay comida a la vista, cambiar de estado
        Bunny nearestBunny = FindNearestBunny();
        if (nearestBunny != null)
        {
            currentState = PredatorState.SearchingFood;
            destination = nearestBunny.transform.position;
            return;
        }

        // Si ya llegó al destino, elegir uno nuevo
        if (Vector3.Distance(transform.position, destination) < 0.1f)
        {
            SelectNewDestination();
        }
    }

    void SearchFood()
    {
        Bunny nearestBunny = FindNearestBunny();
        if (nearestBunny == null)
        {
            // Si no hay comida, volver a explorar
            currentState = PredatorState.Exploring;
            return;
        }

        destination = nearestBunny.transform.position;

        // Si está suficientemente cerca, pasar a comer
        if (Vector3.Distance(transform.position, nearestBunny.transform.position) < 0.2f)
        {
            currentState = PredatorState.Eating;
        }
    }

    void Eat()
    {
        Collider2D foodHit = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Bunnies"));
        if (foodHit != null)
        {
            Bunny food = foodHit.GetComponent<Bunny>();
            if (food != null)
            {
                energy += food.age;
                Destroy(food.gameObject);
            }
        }

        // Después de comer vuelve a explorar
        currentState = PredatorState.Exploring;
    }

    void Flee()
    {
        SelectNewDestination();
        currentState = PredatorState.Exploring;
    }

    void SelectNewDestination()
    {
        Vector3 direction = new Vector3(
            Random.Range(-visionRange, visionRange),
            Random.Range(-visionRange, visionRange),
            0
        );

        Vector3 targetPoint = transform.position + direction;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, visionRange, LayerMask.GetMask("Obstacles"));

        if (hit.collider != null)
        {
            float offset = transform.localScale.magnitude * 0.5f;
            destination = hit.point - (Vector2)direction.normalized * offset;
        }
        else
        {
            destination = targetPoint;
        }
    }

    void Move()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            speed * h
        );

        energy -= speed * h;
    }

    void Age()
    {
        age += h;
    }

    void CheckState()
    {
        if (energy <= 0 || age > maxAge)
        {
            isAlive = false;
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(destination, 0.2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, destination);
    }

    Bunny FindNearestBunny() // Busca al conejo más cercano dentro del rango de visión, considerando obstáculos
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRange, LayerMask.GetMask("Bunnies")); // Busca todos los collider dentro del rango de visión
        Debug.Log($"Predator {name} encontró {hits.Length} colliders en su rango");
        Bunny nearest = null; // Inicializa la variable
        float minDist = Mathf.Infinity; // Inicializa la distancia mínima a infinito

        foreach (Collider2D hit in hits)// Se ejecuta para cada collider encontrado
        {
            Bunny food = hit.GetComponent<Bunny>();
            if (food != null)
            {
                Vector2 direction = food.transform.position - transform.position; //Acá se calcula la dirección hacia el conejo que va a cazar
                float dist = direction.magnitude; //Acá se calcula la distancia hacia el conejo que va a cazar
                RaycastHit2D rayHit = Physics2D.Raycast(transform.position, direction.normalized, dist, LayerMask.GetMask("Obstacles")); //Acá se lanza un rayo desde el depredador hacia el conejo para verificar si hay obstáculos en el camino
                if (rayHit.collider == null) //Si el rayo no choca con ningún obstáculo, entonces el depredador tiene un camino directo hacia el conejo. Si el rayo choca con un obstáculo, entonces el depredador no puede ver al conejo por el obstáculo que hay en medio
                {
                    if (dist < minDist) //Si la distancia hacia el conejo es menor que la distancia mínima
                    {
                        minDist = dist; //Entonces se actualiza la distancia mínima
                        nearest = food; //Y se actualiza el conejo más cercano
                    }
                }
            }
        }
        return nearest; //Retornando al conejo más cercano que el depredador puede ver, si no hay niguno entonces manda un null
    }
}
