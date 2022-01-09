using UnityEngine;

public class SimulationSettings : MonoBehaviour
{
    [SerializeField] private Transform m_AgentParentTransform;
    [SerializeField] private GameObject m_AgentPrefab;
    [SerializeField] private int m_NrAgents = 10;
    [SerializeField] private float m_SpawnDelay = 0.5f;
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
            Instantiate(m_AgentPrefab, m_AgentParentTransform);
            m_ElapsedTime = 0.0f;
            ++m_SpawnedAgentCount;
        }        
    }
}
