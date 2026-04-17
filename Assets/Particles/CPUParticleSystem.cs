using UnityEngine;

using Random = UnityEngine.Random;

public class Particle2D
{
    public Vector2 position;
    public Vector2 previousPosition;

    public Particle2D(Vector2 position, Vector2 velocity)
    {
        this.position = position;
        this.previousPosition = position - velocity;
    }
}

public class CPUParticleSystem : MonoBehaviour
{
    [SerializeField, Range(0, 100000)] int _particleCount;
    [SerializeField, Range(0.0f, 2.0f)] float _particleDrawRadius = 0.5f;
    [SerializeField] Rect _boundingRect;
    [SerializeField, Range(0.0f, 1.0f)] float _dampening = 0.1f; 

    [Header("Debug Flags")]
    [SerializeField] bool _drawParticlePreviousPosition = true;

    [Header("Colors")]
    [SerializeField] Color _boundingRectColor = Color.grey;
    [SerializeField] Color _particleColor = Color.white;
    [SerializeField] Color _previousPositionColor = Color.white;

    Particle2D[] particles;

    protected void Start()
    {
        SpawnParticles(_boundingRect, _particleCount);
    }

    protected void Update()
    {
        
    }

    protected void FixedUpdate()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            VerletIntegrate(particles[i]);
        }
    }

    protected void OnDrawGizmos()
    {
        DrawBoundingRect();
        DrawParticles();
    }


    void SpawnParticles(Rect boundingRect, int count)
    {
        particles = new Particle2D[count];

        for (int i = 0; i < count; i++)
        {
            float posX = Random.Range(boundingRect.xMin, boundingRect.xMax);
            float posY = Random.Range(boundingRect.yMin, boundingRect.yMax);
            Vector2 pos = new Vector2(posX, posY);

            float dirX = Random.Range(-1.0f, 1.0f);
            float dirY = Random.Range(-1.0f, 1.0f);
            Vector2 dir = new Vector2(dirX, dirY);

            float mag = Random.Range(0.1f, 1.0f);

            Vector2 vel = dir.normalized * mag;

            particles[i] = new Particle2D(pos, vel);
        }
    }

    void VerletIntegrate(Particle2D particle)
    {
        Vector2 current = particle.position;
        Vector2 previous = particle.previousPosition;

        Vector2 vel = (current - previous) * (1 - _dampening);

        Vector2 next = current + vel;

        particle.previousPosition = particle.position;
        particle.position = next;
    }

    void DrawBoundingRect()
    {
        DebugUtils.DrawRect(_boundingRect, _boundingRectColor);
    }

    void DrawParticles()
    {
        if (particles != null)
        {

            for (int i = 0; i < particles.Length; i++)
            {
                Gizmos.color = _particleColor;

                Particle2D p = particles[i];

                Vector2 position = p.position;
                Vector2 previousPosition = p.previousPosition;

                Gizmos.DrawSphere(position, _particleDrawRadius);

                if (_drawParticlePreviousPosition)
                {
                    Gizmos.color = _previousPositionColor;
                    Gizmos.DrawSphere(previousPosition, _particleDrawRadius * 0.5f);
                    Gizmos.color = Color.darkGray;
                    Gizmos.DrawLine(position, previousPosition);
                }
            }
        }
    }
}
