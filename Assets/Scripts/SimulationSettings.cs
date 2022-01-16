using UnityEngine;

public class SimulationSettings : MonoBehaviour
{
    [Header("Startup settings")]
    [SerializeField] private Transform m_AgentParentTransform;
    [SerializeField] private GameObject m_AgentPrefab;
    [SerializeField] private int m_NrStandardAgents = 10;
    [SerializeField] private int m_NrAgressiveAgents = 10;
    [SerializeField] private int m_NrCautiousAgents = 10;
    [SerializeField] private int m_NrDistractedAgents = 10;
    [SerializeField] private float m_SpawnDelay = 0.5f;

    private float m_ElapsedTime;
    private int m_SpawnedStandardAgentCount = 0;
    private int m_SpawnedAgressiveAgentCount = 0;
    private int m_SpawnedCautiousAgentCount = 0;
    private int m_SpawnedDistractedAgentCount = 0;
    private bool m_CanSpawn = true;
    private PersonalityType m_NextTypeToSpawn = PersonalityType.standard;

    private CharacterParameters m_StandardParams;
    private CharacterParameters m_AgressiveParams;
    private CharacterParameters m_CautiousParams;
    private CharacterParameters m_DistractedParams;

    // Start is called before the first frame update
    private void Start()
    {
        Debug.Log("Started spawning agents");
        m_StandardParams = CreateStandardParameters();
        m_AgressiveParams = CreateAgressiveParameters();
        m_CautiousParams = CreateCautiousParameters();
        m_DistractedParams = CreateDistractedParameters();
    }

    private void Update()
    {
        if(!m_CanSpawn)
        {
            return;
        }

        m_ElapsedTime += Time.deltaTime;

        if(m_ElapsedTime >= m_SpawnDelay)
        {
            CharacterParameters newParams = null;
           
            //Give the newly spawned agent the correct parameters according to personality type
            switch(m_NextTypeToSpawn)
            {
                case PersonalityType.standard:
                    if (m_SpawnedStandardAgentCount < m_NrStandardAgents)
                    {
                        newParams = m_StandardParams;
                        ++m_SpawnedStandardAgentCount;
                        
                    }
                    else
                    {
                        ++m_NextTypeToSpawn;
                    }     
                    
                    break;
                case PersonalityType.agressive:
                    if (m_SpawnedAgressiveAgentCount < m_NrAgressiveAgents)
                    {
                        newParams = m_AgressiveParams;
                        ++m_SpawnedAgressiveAgentCount;
                        
                    }
                    else
                    {
                        ++m_NextTypeToSpawn;
                    }
                    
                    break;
                case PersonalityType.cautious:
                    if (m_SpawnedCautiousAgentCount < m_NrCautiousAgents)
                    {
                        newParams = m_CautiousParams;
                        ++m_SpawnedCautiousAgentCount;
                        
                    }
                    else
                    {
                        ++m_NextTypeToSpawn;
                    }
                    
                    break;
               default:
                    // No more known types to spawn, therefore stop spawning
                    m_ElapsedTime = 0.0f;
                    Debug.Log("Finished spawning agents");
                    m_CanSpawn = false;
                    // Spawning has stopped, exit this function to avoid spawning an extra agent
                    return;
            }

            if(newParams != null)
            {
                // Spawn agent and assign parameters
                GameObject spawnedAgent = Instantiate(m_AgentPrefab, m_AgentParentTransform);
                spawnedAgent.GetComponent<SFCharacter>().Parameters = newParams;
            }
                                
            m_ElapsedTime = 0.0f;          
        }        
    }

    private CharacterParameters CreateStandardParameters()
    {
        CharacterParameters charParams = new CharacterParameters();
        charParams.Type = PersonalityType.standard;

        //Obstacle repulsive
        charParams.ObstacleRepulsiveWeight = 1.0f;
        charParams.ObstacleRepulsiveStrength = 47.0f;
        charParams.ObstacleRepulsiveRange = 10.0f;

        //Agent repulsive
        charParams.AgentRepulsiveWeight = 1.0f;
        charParams.AgentRepulsiveStrength = 47.0f;
        charParams.AgentRepulsiveRange = 10.0f;

        //General repulsive
        charParams.DirectionWeight = 2.0f;
        charParams.RangeDirFactor = 0.4f;
        charParams.AngInteractRange = 2.0f;
        charParams.AngInteractRangeLarge = 3.0f;

        //Wall repulsive
        charParams.WallRepulsiveWeight = 1.0f;
        charParams.WallRepulsiveStrength = 2.0f;
        charParams.WallRepulsiveRange = 0.4f;

        //Driving force
        charParams.DrivingWeight = 1.0f;
        charParams.DesiredSpeed = 0.5f;

        //Attractice force
        charParams.AttractiveWeight = 0.1f;
        charParams.AttractiveStrength = 2.2f;
        charParams.AttractiveRange = 10.0f;

        return charParams;
    }

    private CharacterParameters CreateAgressiveParameters()
    {
        CharacterParameters charParams = new CharacterParameters();
        charParams.Type = PersonalityType.agressive;

        //Obstacle repulsive
        charParams.ObstacleRepulsiveWeight = 1.0f;
        charParams.ObstacleRepulsiveStrength = 38.0f;
        charParams.ObstacleRepulsiveRange = 4.0f;

        //Agent repulsive
        charParams.AgentRepulsiveWeight = 0.9f;
        charParams.AgentRepulsiveStrength = 25.0f;
        charParams.AgentRepulsiveRange = 8.0f;

        //General repulsive
        charParams.DirectionWeight = 2.2f;
        charParams.RangeDirFactor = 0.3f;
        charParams.AngInteractRange = 0.5f;
        charParams.AngInteractRangeLarge = 0.9f;

        //Wall repulsive
        charParams.WallRepulsiveWeight = 1.0f;
        charParams.WallRepulsiveStrength = 2.0f;
        charParams.WallRepulsiveRange = 0.4f;

        //Driving force
        charParams.DrivingWeight = 1.1f;
        charParams.DesiredSpeed = 0.7f;

        //Attractice force
        charParams.AttractiveWeight = 0.2f;
        charParams.AttractiveStrength = 5.0f;
        charParams.AttractiveRange = 8.0f;

        //Attractice force
        charParams.AttractiveWeight = 0.1f;
        charParams.AttractiveStrength = 2.2f;
        charParams.AttractiveRange = 10.0f;

        return charParams;
    }

    private CharacterParameters CreateCautiousParameters()
    {
        CharacterParameters charParams = new CharacterParameters();
        charParams.Type = PersonalityType.cautious;

        //Obstacle repulsive
        charParams.ObstacleRepulsiveWeight = 1.0f;
        charParams.ObstacleRepulsiveStrength = 60.0f;
        charParams.ObstacleRepulsiveRange = 10.0f;

        //Agent repulsive
        charParams.AgentRepulsiveWeight = 1.0f;
        charParams.AgentRepulsiveStrength = 60.0f;
        charParams.AgentRepulsiveRange = 10.0f;

        //General repulsive
        charParams.DirectionWeight = 2.0f;
        charParams.RangeDirFactor = 0.7f;
        charParams.AngInteractRange = 2.0f;
        charParams.AngInteractRangeLarge = 3.0f;

        //Wall repulsive
        charParams.WallRepulsiveWeight = 1.0f;
        charParams.WallRepulsiveStrength = 2.8f;
        charParams.WallRepulsiveRange = 0.8f;

        //Driving force
        charParams.DrivingWeight = 1.0f;
        charParams.DesiredSpeed = 0.3f;

        //Attractice force
        charParams.AttractiveWeight = 0.1f;
        charParams.AttractiveStrength = 2.0f;
        charParams.AttractiveRange = 10.0f;

        return charParams;
    }

    private CharacterParameters CreateDistractedParameters()
    {
        CharacterParameters charParams = new CharacterParameters();
        charParams.Type = PersonalityType.standard;

        //Obstacle repulsive
        charParams.ObstacleRepulsiveWeight = 0.8f;
        charParams.ObstacleRepulsiveStrength = 47.0f;
        charParams.ObstacleRepulsiveRange = 10.0f;

        //Agent repulsive
        charParams.AgentRepulsiveWeight = 1.0f;
        charParams.AgentRepulsiveStrength = 47.0f;
        charParams.AgentRepulsiveRange = 10.0f;

        //General repulsive
        charParams.DirectionWeight = 2.0f;
        charParams.RangeDirFactor = 0.4f;
        charParams.AngInteractRange = 2.0f;
        charParams.AngInteractRangeLarge = 3.0f;

        //Wall repulsive
        charParams.WallRepulsiveWeight = 0.8f;
        charParams.WallRepulsiveStrength = 2.0f;
        charParams.WallRepulsiveRange = 0.4f;

        //Driving force
        charParams.DrivingWeight = 1.0f;
        charParams.DesiredSpeed = 0.5f;

        //Attractice force
        charParams.AttractiveWeight = 0.1f;
        charParams.AttractiveStrength = 2.2f;
        charParams.AttractiveRange = 10.0f;

        return charParams;
    }
}
