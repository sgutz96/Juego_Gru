using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform[] patrolPoints;   // Lista de puntos para rondas (waypoints)
    public float chaseDistance = 10f;  // Distancia de persecución
    public float attackDistance = 2f;  // Distancia de ataque
    public float reactionTime = 2f;   // Tiempo de reacción después de ser atacado
    public Transform hidePosition;     // Posición de escondite cuando la vida es baja

    public float maxHealth = 100f;     // Vida máxima
    public float currentHealth;       // Vida actual

    public enum EnemyState { Patrolling, Chasing, Attacked, Resting, Hiding }
    public EnemyState currentState;

    public Transform player;
    public int patrolIndex = 0; // Índice para las rondas

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform; // Asume que el jugador tiene la etiqueta "Player"
        currentHealth = maxHealth;
        currentState = EnemyState.Patrolling;
    }

    private void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                Patrol();
                break;
            case EnemyState.Chasing:
                ChasePlayer();
                break;
            case EnemyState.Attacked:
                ReactToDamage();
                break;
            case EnemyState.Resting:
                Rest();
                break;
            case EnemyState.Hiding:
                Hide();
                break;
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        // Dirige al enemigo al siguiente punto de patrullaje
        agent.destination = patrolPoints[patrolIndex].position;

        // Si ha llegado al punto, pasa al siguiente
        if (Vector3.Distance(transform.position, patrolPoints[patrolIndex].position) < 1f)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        }

        // Cambia al estado de persecución si el jugador está dentro de la distancia
        if (Vector3.Distance(transform.position, player.position) <= chaseDistance)
        {
            currentState = EnemyState.Chasing;
        }
    }

    private void ChasePlayer()
    {
        agent.destination = player.position;

        // Si el enemigo llega lo suficientemente cerca del jugador, cambia a atacar
        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            // Lógica de ataque aquí (puedes agregar animaciones o efectos de daño)
            Debug.Log("Enemigo atacando al jugador");
        }

        // Si el jugador se aleja más allá de la distancia de persecución, vuelve a patrullar
        if (Vector3.Distance(transform.position, player.position) > chaseDistance)
        {
            currentState = EnemyState.Patrolling;
        }
    }

    private void ReactToDamage()
    {
        // Lógica de reacción al daño (por ejemplo, retroceder)
        Vector3 damageDirection = (transform.position - player.position).normalized;
        agent.destination = transform.position + damageDirection * 5f; // Alejarse del daño

        // Después de un tiempo de reacción, vuelve a patrullar
        Invoke(nameof(ResetState), reactionTime);
    }

    private void Rest()
    {
        // Si la vida es menor al 30%, entra en estado de descanso/esconderse
        if (currentHealth < maxHealth * 0.30f)
        {
            currentState = EnemyState.Hiding;
        }
    }

    private void Hide()
    {
        if (currentHealth < maxHealth * 0.10f)
        {
            // Si la vida es menor al 10%, se va al punto de esconderse
            agent.destination = hidePosition.position;
            if (Vector3.Distance(transform.position, hidePosition.position) < 1f)
            {
                Debug.Log("Enemigo escondido");
                // Aquí puedes añadir alguna animación de escondite
            }
        }
    }

    private void ResetState()
    {
        currentState = EnemyState.Patrolling;
    }

    // Método para que el enemigo reciba daño
    public void TakeDamage(float damage, Vector3 attackDirection)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            currentState = EnemyState.Attacked;
        }

        // Reaccionar hacia la dirección del impacto
        agent.destination = transform.position + attackDirection.normalized * 5f;  // Alejarse un poco
    }

    private void Die()
    {
        Debug.Log("Enemigo muerto");
        // Aquí puedes añadir la lógica para eliminar al enemigo o hacer que se destruya
        Destroy(gameObject);
    }
}
