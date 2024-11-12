using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform[] patrolPoints;   // Lista de puntos para rondas (waypoints)
    public float chaseDistance = 10f;  // Distancia de persecuci�n
    public float attackDistance = 2f;  // Distancia de ataque
    public float reactionTime = 2f;   // Tiempo de reacci�n despu�s de ser atacado
    public Transform hidePosition;     // Posici�n de escondite cuando la vida es baja

    public float maxHealth = 100f;     // Vida m�xima
    public float currentHealth;       // Vida actual

    public enum EnemyState { Patrolling, Chasing, Attacked, Resting, Hiding }
    public EnemyState currentState;

    public Transform player;
    public int patrolIndex = 0; // �ndice para las rondas

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

        // Cambia al estado de persecuci�n si el jugador est� dentro de la distancia
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
            // L�gica de ataque aqu� (puedes agregar animaciones o efectos de da�o)
            Debug.Log("Enemigo atacando al jugador");
        }

        // Si el jugador se aleja m�s all� de la distancia de persecuci�n, vuelve a patrullar
        if (Vector3.Distance(transform.position, player.position) > chaseDistance)
        {
            currentState = EnemyState.Patrolling;
        }
    }

    private void ReactToDamage()
    {
        // L�gica de reacci�n al da�o (por ejemplo, retroceder)
        Vector3 damageDirection = (transform.position - player.position).normalized;
        agent.destination = transform.position + damageDirection * 5f; // Alejarse del da�o

        // Despu�s de un tiempo de reacci�n, vuelve a patrullar
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
                // Aqu� puedes a�adir alguna animaci�n de escondite
            }
        }
    }

    private void ResetState()
    {
        currentState = EnemyState.Patrolling;
    }

    // M�todo para que el enemigo reciba da�o
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

        // Reaccionar hacia la direcci�n del impacto
        agent.destination = transform.position + attackDirection.normalized * 5f;  // Alejarse un poco
    }

    private void Die()
    {
        Debug.Log("Enemigo muerto");
        // Aqu� puedes a�adir la l�gica para eliminar al enemigo o hacer que se destruya
        Destroy(gameObject);
    }
}
