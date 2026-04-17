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

public struct Edge
{
    public Vector2 start;
    public Vector2 end;
    public Vector2 vector;

    public Edge(Vector2 startVertex, Vector2 endVertex)
    {
        this.start = startVertex;
        this.end = endVertex;
        this.vector = endVertex - startVertex;  
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

    Particle2D[] _particles;
    Edge[] _rectEdges = new Edge[4];

    protected void Start()
    {
        SetupRectEdges();
        SpawnParticles(_boundingRect, _particleCount);
    }

    protected void FixedUpdate()
    {
        for (int i = 0; i < _particles.Length; i++)
        {
            VerletIntegrate(_particles[i]);
        }
    }

    protected void OnDrawGizmos()
    {
        DrawBoundingRect();
        DrawParticles();
    }

    // Sets up the bounding rect edges in a clockwise order 
    void SetupRectEdges()
    {
        Rect b = _boundingRect;
        _rectEdges[0] = new Edge(new Vector2(b.xMin, b.yMin), new Vector2(b.xMin, b.yMax)); // left edge
        _rectEdges[1] = new Edge(new Vector2(b.xMin, b.yMax), new Vector2(b.xMax, b.yMax)); // top edge
        _rectEdges[2] = new Edge(new Vector2(b.xMax, b.yMax), new Vector2(b.xMax, b.yMin)); // right edge
        _rectEdges[3] = new Edge(new Vector2(b.xMax, b.yMin), new Vector2(b.xMin, b.yMin)); // bottom edge
    }

    void SpawnParticles(Rect boundingRect, int count)
    {
        _particles = new Particle2D[count];

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

            _particles[i] = new Particle2D(pos, vel);
        }
    }

    void VerletIntegrate(Particle2D particle)
    {
        Vector2 current = particle.position;
        Vector2 previous = particle.previousPosition;

        Vector2 vel = (current - previous) * (1 - _dampening);

        Vector2 next = current + vel;


        // do collision detection (vector reflection)
        for (int i = 0; i < _rectEdges.Length; i++)
        {
            Edge e = _rectEdges[i];

            Line2D line1 = new Line2D(current, next, false);
            Line2D line2 = new Line2D(e.start, e.end, true);

            Vector2 inter = GeometryUtils.IntersectionPointTwoLines2D(line1, line2, out bool succ);

            if (succ)
            {
                Vector2 edge = e.vector;
                Vector2 edgeNormal = new Vector2(edge.y, -edge.x);
                edgeNormal = edgeNormal.normalized;

                Vector2 reflected = Vector2.Reflect(next - inter, edgeNormal);

                next = inter + reflected;

                particle.previousPosition = inter;
                particle.position = next;

                return;
            }
        }

        particle.previousPosition = particle.position;
        particle.position = next;
    }

    void DrawBoundingRect()
    {
        DebugUtils.DrawRect(_boundingRect, _boundingRectColor);
    }

    void DrawParticles()
    {
        if (_particles != null)
        {

            for (int i = 0; i < _particles.Length; i++)
            {
                Gizmos.color = _particleColor;

                Particle2D p = _particles[i];

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
