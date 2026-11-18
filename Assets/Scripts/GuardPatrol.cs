using UnityEngine;

/// <summary>
/// Hace que el guardia vaya de un punto a otro en bucle: A -> B -> A -> B...
/// Va en el objeto Guard, que necesita un Rigidbody2D (Body Type = Kinematic).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class GuardPatrol : MonoBehaviour
{
    [Header("Ruta de patrulla")]
    [Tooltip("Puntos por los que pasa el guardia, en orden. Arrastra aquí PatrolPointA y PatrolPointB.")]
    [SerializeField] private Transform[] patrolPoints;

    [Header("Movimiento")]
    [Tooltip("Velocidad del guardia (unidades por segundo).")]
    [SerializeField] private float speed = 2f;

    [Tooltip("Segundos que se queda quieto en cada punto antes de seguir.")]
    [SerializeField] private float waitTime = 1f;

    [Tooltip("A qué distancia del punto consideramos que ya ha llegado.")]
    [SerializeField] private float arriveDistance = 0.05f;

    private Rigidbody2D rb;
    private int currentPoint;
    private float waitTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Las físicas se mueven en FixedUpdate (no en Update).
    private void FixedUpdate()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return;
        }

        // 1. Si está esperando en un punto, descuenta tiempo y no se mueve.
        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        // 2. Si alguien dejó un hueco vacío en la lista, lo saltamos.
        Transform point = patrolPoints[currentPoint];
        if (point == null)
        {
            GoToNextPoint();
            return;
        }

        // 3. Da un pasito hacia el punto actual.
        Vector2 target = point.position;
        Vector2 newPosition = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        // 4. Si ya ha llegado, espera un poco y pasa al siguiente punto.
        if (Vector2.Distance(newPosition, target) <= arriveDistance)
        {
            GoToNextPoint();
            waitTimer = waitTime;
        }
    }

    private void GoToNextPoint()
    {
        // El % hace que después del último punto vuelva al primero (bucle).
        currentPoint = (currentPoint + 1) % patrolPoints.Length;
    }

    // Dibuja la ruta en amarillo en la ventana Scene (no se ve en el juego).
    private void OnDrawGizmos()
    {
        if (patrolPoints == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Transform from = patrolPoints[i];
            Transform to = patrolPoints[(i + 1) % patrolPoints.Length];
            if (from == null)
            {
                continue;
            }

            Gizmos.DrawWireSphere(from.position, 0.3f);
            if (to != null)
            {
                Gizmos.DrawLine(from.position, to.position);
            }
        }
    }
}
