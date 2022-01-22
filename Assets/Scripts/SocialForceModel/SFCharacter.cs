using UnityEngine;
using UnityEngine.AI;

public enum PersonalityType
{
    standard,
    agressive,
    cautious,
    distracted,
    reckless,
}

// This type is a class instead of a struct because a class is a reference type
// Hundreds of agents each having an instance of a large data container like a CharacterParameters object causes a significant performance loss
public class CharacterParameters
{
    // Type
    private PersonalityType m_Type;
    public PersonalityType Type { get { return m_Type; } set { m_Type = value; } }

    // Obstacle repulsive
    private float m_ObstacleRepulsiveWeight;
    public float ObstacleRepulsiveWeight { get { return m_ObstacleRepulsiveWeight; } set { m_ObstacleRepulsiveWeight = value; } }
    private float m_ObstacleRepulsiveStrength;
    public float ObstacleRepulsiveStrength { get { return m_ObstacleRepulsiveStrength; } set { m_ObstacleRepulsiveStrength = value; } }
    private float m_ObstacleRepulsiveRange;
    public float ObstacleRepulsiveRange { get { return m_ObstacleRepulsiveRange; } set { m_ObstacleRepulsiveRange = value; } }

    // Agent repulsive
    private float m_AgentRepulsiveWeight;
    public float AgentRepulsiveWeight { get { return m_AgentRepulsiveWeight; } set { m_AgentRepulsiveWeight = value; } }
    private float m_AgentRepulsiveStrength;
    public float AgentRepulsiveStrength { get { return m_AgentRepulsiveStrength; } set { m_AgentRepulsiveStrength = value; } }
    private float m_AgentRepulsiveRange;
    public float AgentRepulsiveRange { get { return m_AgentRepulsiveRange; } set { m_AgentRepulsiveRange = value; } }

    // General repulsive
    private float m_DirectionWeight;
    public float DirectionWeight { get { return m_DirectionWeight; } set { m_DirectionWeight = value; } }
    private float m_RangeDirFactor;
    public float RangeDirFactor { get { return m_RangeDirFactor; } set { m_RangeDirFactor = value; } }
    private float m_AngInteractRange;
    public float AngInteractRange { get { return m_AngInteractRange; } set { m_AngInteractRange = value; } }
    private float m_AngInteractRangeLarge;
    public float AngInteractRangeLarge { get { return m_AngInteractRangeLarge; } set { m_AngInteractRangeLarge = value; } }

    // Wall repulsive
    private float m_WallRepulsiveWeight;
    public float WallRepulsiveWeight { get { return m_WallRepulsiveWeight; } set { m_WallRepulsiveWeight = value; } }
    private float m_WallRepulsiveStrength;
    public float WallRepulsiveStrength { get { return m_WallRepulsiveStrength; } set { m_WallRepulsiveStrength = value; } }
    private float m_WallRepulsiveRange;
    public float WallRepulsiveRange { get { return m_WallRepulsiveRange; } set { m_WallRepulsiveRange = value; } }

    // Driving force
    private float m_DrivingWeight;
    public float DrivingWeight { get { return m_DrivingWeight; } set { m_DrivingWeight = value; } }
    private float m_DesiredSpeed;
    public float DesiredSpeed { get { return m_DesiredSpeed; } set { m_DesiredSpeed = value; } }

    //Attractive force
    private float m_AttractiveWeight;
    public float AttractiveWeight { get { return m_AttractiveWeight; } set { m_AttractiveWeight = value; } }
    private float m_AttractiveStrength;
    public float AttractiveStrength { get { return m_AttractiveStrength; } set { m_AttractiveStrength = value; } }
    private float m_AttractiveRange;
    public float AttractiveRange { get { return m_AttractiveRange; } set { m_AttractiveRange = value; } }
}

public class SFCharacter : MonoBehaviour
{
    [SerializeField] private Transform m_Destination;
    private Vector3 m_Velocity = new Vector3();
    public Vector3 Velocity { get { return m_Velocity; } }
    private NavMeshAgent m_CharacterAgent;

    private float m_Radius = 0.5f;
    public float Radius { get { return m_Radius; } }
    private SFManager m_SFManager;
    private CharacterParameters m_Params;
    public CharacterParameters Parameters
    { 
        get 
        { 
            return m_Params;
        } 
        set 
        { 
            m_Params = value;
            // speed requires a separate assignment because it is used by the NavmeshAgent component
            m_CharacterAgent.speed = value.DesiredSpeed;

            // Find the renderer component instead of using a serialize field to save on memory usage
            // memory usage is important to consider due to the large amount of agents in the scene
            Renderer mainRenderer = transform.GetChild(0).GetComponent<Renderer>();
            if (mainRenderer != null)
            {
                // Also get the renderer of the front object
                Renderer frontRenderer = mainRenderer.gameObject.transform.GetChild(0).GetComponent<Renderer>();
                switch (m_Params.Type)
                {
                    case PersonalityType.agressive:
                        Color agressiveColor = new Color(1.0f, 0.4f, 0.4f);
                        mainRenderer.material.color = agressiveColor;
                        frontRenderer.material.color = agressiveColor;
                        break;
                    case PersonalityType.cautious:
                        Color cautiousColor = new Color(0.8f, 0.8f, 0.1f);
                        mainRenderer.material.color = cautiousColor;
                        frontRenderer.material.color = cautiousColor;
                        break;
                    case PersonalityType.distracted:
                        Color distractedColor = new Color(0.7f, 0.2f, 0.7f);
                        mainRenderer.material.color = distractedColor;
                        frontRenderer.material.color = distractedColor;
                        break;
                    case PersonalityType.reckless:
                        Color recklessColor = new Color(0.1f, 0.9f, 0.1f);
                        mainRenderer.material.color = recklessColor;
                        frontRenderer.material.color = recklessColor;
                        break;
                    default:
                        // Standard types do not get their color changed
                        break;
                }
            }
        } 
    }

    //TEMP
    private bool m_HasRecorded = false;
    private bool m_IsRecording = false;
    private float m_LastTimePoint = 0.0f;

    void Awake()
    {
        m_CharacterAgent = GetComponent<NavMeshAgent>();      
    
        m_SFManager = GameObject.Find("SceneScripts").GetComponent<SFManager>();
        m_SFManager.AddAgent(this);

        if(m_Destination == null)
        {
            SetNextOrderedDestination();
            m_CharacterAgent.Warp(m_Destination.position);
        }

        m_CharacterAgent.stoppingDistance = 1.5f;      
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (m_CharacterAgent.remainingDistance < m_CharacterAgent.stoppingDistance)
        {
            SetNewDestination();
        }
                
        Vector3 drivingForce = m_Params.DrivingWeight * DrivingForce();
        Vector3 obstacleRepulsiveForce = m_Params.ObstacleRepulsiveWeight * m_SFManager.CalculateObstacleRepulsiveForce(this);
        Vector3 agentRepulsiveForce = m_Params.AgentRepulsiveWeight * m_SFManager.CalculateAgentRepulsiveForce(this);
        Vector3 wallRepulsiveForce = m_Params.WallRepulsiveWeight * m_SFManager.CalculateWallRepulsiveForce(transform.position, m_Radius, m_Params.WallRepulsiveRange, m_Params.WallRepulsiveStrength);
        Vector3 attractiveForce = m_Params.AttractiveWeight * m_SFManager.CalculateAttractive(transform.position, m_Radius, m_Params.AttractiveRange, m_Params.AttractiveStrength);
        Vector3 acceleration = drivingForce + obstacleRepulsiveForce + agentRepulsiveForce + wallRepulsiveForce + attractiveForce;
        m_Velocity = acceleration /** Time.deltaTime*/;

        // Limit maximum velocity
        if (Vector3.SqrMagnitude(m_Velocity) > m_Params.DesiredSpeed * m_Params.DesiredSpeed)
        {
            m_Velocity.Normalize();
            m_Velocity *= m_Params.DesiredSpeed;
        }

        m_Velocity *= Time.deltaTime;
        
        transform.position += m_Velocity;
        RotateToVelocity(m_Velocity);
    }

    Vector3 DrivingForce()
    {
        const float relaxationT = 0.54f;

        //Vector3 desiredDirection = destination.transform.position - this.transform.position;
        Vector3 desiredDirection = m_CharacterAgent.steeringTarget - this.transform.position;
        desiredDirection.Normalize();

        Vector3 drivingForce = (m_Params.DesiredSpeed * desiredDirection - m_Velocity) / relaxationT;

        return drivingForce;
    }

    private void RotateToVelocity(Vector3 velocity)
    {
        // Implementation taken from ThirdPersonCharacter.cs from UnityStandardAssets
        // This was present in the starting framework of this project, but I've moved it out of that script since it was the only useful part from it
        Vector3 move = velocity.normalized;
        move = transform.InverseTransformDirection(move);
        move = Vector3.ProjectOnPlane(move, Vector3.up);
        float stationaryTurnSpeed = 180;
        float movingTurnSpeed = 360;
        float forwardAmount = move.z;
        float turnAmount = Mathf.Atan2(move.x, move.z);
        float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
        transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
    }

    private void SetNewDestination()
    {
        m_Destination = m_SFManager.GetRandomDestination().transform;
        m_CharacterAgent.SetDestination(m_Destination.position);
        // Stop the navmesh agent from performing any movement - we want full manual control of agent movement
        m_CharacterAgent.isStopped = true;

        if(m_SFManager.IsRecording())
        {
            if(!m_IsRecording)
            {
                m_LastTimePoint = Time.time;
                m_IsRecording = true;
            }
            else if(!m_HasRecorded)
            {
                float timeTaken = Time.time - m_LastTimePoint;
                m_SFManager.RecordTime((int)m_Params.Type, timeTaken);
                m_IsRecording = false;
                m_HasRecorded = true;
            }
        }
    }

    private void SetNextOrderedDestination()
    {
        m_Destination = m_SFManager.GetNextOrderedDestination().transform;
        m_CharacterAgent.SetDestination(m_Destination.position);
        // Stop the navmesh agent from performing any movement - we want full manual control of agent movement
        m_CharacterAgent.isStopped = true;
    }

}
