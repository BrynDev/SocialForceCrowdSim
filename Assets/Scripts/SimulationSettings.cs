using UnityEngine;

public class SimulationSettings : MonoBehaviour
{
    [Header("Startup settings")]
    [SerializeField] private Transform m_AgentParentTransform;
    [SerializeField] private GameObject m_AgentPrefab;
    [SerializeField] private int m_NrAgents = 10;
    [SerializeField] private float m_SpawnDelay = 0.5f;

    [Header("Obstacle repulsive force")]
    [SerializeField] private float m_ObstacleRepulsiveWeight = 1.0f;
    [SerializeField] private float m_ObstacleRepulsiveStrength;
    [SerializeField] private float m_ObstacleRepulsiveRange;

    [Header("Agent repulsive force")]
    [SerializeField] private float m_AgentRepulsiveWeight = 1.0f;
    [SerializeField] private float m_AgentRepulsiveStrength;
    [SerializeField] private float m_AgentRepulsiveRange;

    [Header("Wall repulsive force")]
    [SerializeField] private float m_WallRepulsiveWeight = 1.0f;
    [SerializeField] private float m_WallRepulsiveStrength = 2.0f;
    [SerializeField] private float m_WallRepulsiveRange = 0.4f;

    [Header("Driving force")]
    [SerializeField] private float m_DrivingWeight = 1.0f;
    [SerializeField] private float m_AgentDesiredSpeed = 0.5f;


    private float m_ElapsedTime;
    private int m_SpawnedAgentCount = 0;
    private bool m_CanSpawn = true;
    
    // Start is called before the first frame update
    private void Start()
    {
        Debug.Log("Started spawning agents");
    }

    private void Update()
    {
        if(!m_CanSpawn)
        {
            return;
        }

        if (m_SpawnedAgentCount >= m_NrAgents)
        {
            // If enough agents were spawned, disable further spawning
            m_CanSpawn = false;
            m_ElapsedTime = 0.0f;
            Debug.Log("Finished spawning agents");
        }

        m_ElapsedTime += Time.deltaTime;

        if(m_ElapsedTime >= m_SpawnDelay)
        {
            GameObject spawnedAgent = Instantiate(m_AgentPrefab, m_AgentParentTransform);
            spawnedAgent.GetComponent<SFCharacter>().Parameters = CreateParameters();
            m_ElapsedTime = 0.0f;
            ++m_SpawnedAgentCount;
        }        
    }

    private CharacterParameters CreateParameters()
    {
        CharacterParameters charParams = new CharacterParameters();
        charParams.ObstacleRepulsiveWeight = m_ObstacleRepulsiveWeight;
        charParams.ObstacleRepulsiveStrength = m_ObstacleRepulsiveStrength;
        charParams.ObstacleRepulsiveRange = m_ObstacleRepulsiveRange;

        charParams.AgentRepulsiveWeight = m_AgentRepulsiveWeight;
        charParams.AgentRepulsiveStrength = m_AgentRepulsiveStrength;
        charParams.AgentRepulsiveRange = m_AgentRepulsiveRange;

        charParams.WallRepulsiveWeight = m_WallRepulsiveWeight;
        charParams.WallRepulsiveStrength = m_WallRepulsiveStrength;
        charParams.WallRepulsiveRange = m_WallRepulsiveRange;

        charParams.DrivingWeight = m_DrivingWeight;
        charParams.DesiredSpeed = m_AgentDesiredSpeed;

        return charParams;
    }
}
