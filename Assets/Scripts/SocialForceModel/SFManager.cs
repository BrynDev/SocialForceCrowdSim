using System.Collections.Generic;
using UnityEngine;

public class SFManager : MonoBehaviour
{
    private List<SFCharacter> m_Agents = new List<SFCharacter>();
    private List<SFObstacle> m_Obstacles = new List<SFObstacle>();
    private List<Wall> m_Walls = new List<Wall>();
    private List<GameObject> m_Destinations = new List<GameObject>();
    private List<GameObject> m_Attractors = new List<GameObject>();
    private int m_NextDestIndex = 0;

    //TEMP
    private bool m_CanRecord = false;
    private List<float> m_RecordedTimes = new List<float>();
    private int m_RecordedCount;

    private void Awake()
    {       
        GameObject[] obstacleArray = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject obstacle in obstacleArray)
        {
            m_Obstacles.Add(obstacle.GetComponent<SFObstacle>());
        }

        GameObject[] wallArray = GameObject.FindGameObjectsWithTag("Wall");
        foreach (GameObject wall in wallArray)
        {
            m_Walls.Add(wall.GetComponent<Wall>());
        }

        GameObject[] destArray = GameObject.FindGameObjectsWithTag("Destination");
        foreach (GameObject destination in destArray)
        {
            m_Destinations.Add(destination);
        }

        GameObject[] attractArray = GameObject.FindGameObjectsWithTag("Attractor");
        foreach (GameObject attractor in attractArray)
        {
            m_Attractors.Add(attractor);
        }

        for (int i = 0; i < m_Destinations.Count; ++i)
        {
            m_RecordedTimes.Add(0);
        }
        m_RecordedTimes.TrimExcess();
    }

    public void AddAgent(SFCharacter agentToAdd)
    {
        m_Agents.Add(agentToAdd);
    }

    public GameObject GetRandomDestination()
    {
        int randomIdx = Random.Range(0, m_Destinations.Count);
        return m_Destinations[randomIdx];
    }

    public GameObject GetNextOrderedDestination()
    {
        GameObject destToReturn = m_Destinations[m_NextDestIndex];
        m_NextDestIndex = (++m_NextDestIndex % m_Destinations.Count);
        return destToReturn;
    }

    public Vector3 CalculateAttractiveForce()
    {
        return Vector3.zero;
    }

    public Vector3 CalculateAgentRepulsiveForce(SFCharacter currentAgent)
    {
        // Basic implementation - Not realistic enough (directional turning issues)

        /*Vector3 repulsiveForce = new Vector3();
        foreach (SFCharacter agent in m_Agents)
        {
            Vector3 vectorToAgent = currentAgent.transform.position - agent.transform.position;
            float distanceToObstacle = vectorToAgent.magnitude;
            float angleBetween = Vector3.Angle(vectorToAgent, currentAgent.transform.forward);
            const float compareEpsilon = 0.1f;
            if (Mathf.Abs(angleBetween) < compareEpsilon)
            {
                repulsiveForce.z += 0.2f;
            }

            vectorToAgent.Normalize();
            repulsiveForce += (currentAgent.ObstacleRepulsiveStrength) * Mathf.Exp((currentAgent.Radius - distanceToObstacle) / currentAgent.ObstacleRepulsiveRange) * vectorToAgent;
        }
        repulsiveForce.y = 0.0f;
        return repulsiveForce;*/

        Vector3 agentRepulsiveForce = new Vector3();
        foreach (SFCharacter otherAgent in m_Agents)
        {
            if(otherAgent == currentAgent)
            {
                continue;
            }

            float range = currentAgent.Parameters.AgentRepulsiveRange;
            float directionWeight = currentAgent.Parameters.DirectionWeight;
            float rangeDirectionFactor = currentAgent.Parameters.RangeDirFactor;
            float angularInteractionRange = currentAgent.Parameters.AngInteractRange;
            float angularInteractionRangeLarge = currentAgent.Parameters.AngInteractRangeLarge;
            float repulsiveStrength = currentAgent.Parameters.AgentRepulsiveStrength;

            agentRepulsiveForce += CalculateRepulsive(currentAgent.transform.position, currentAgent.Velocity, otherAgent.transform.position, otherAgent.Velocity, range, directionWeight, rangeDirectionFactor, angularInteractionRange, angularInteractionRangeLarge, repulsiveStrength);
        }
        return agentRepulsiveForce;
    }

    public Vector3 CalculateObstacleRepulsiveForce(SFCharacter currentAgent)
    {
        Vector3 obstacleRepulsiveForce = new Vector3();
        foreach (SFObstacle obstacle in m_Obstacles)
        {
            float range = currentAgent.Parameters.ObstacleRepulsiveRange;
            float directionWeight = currentAgent.Parameters.DirectionWeight;
            float rangeDirectionFactor = currentAgent.Parameters.RangeDirFactor;
            float angularInteractionRange = currentAgent.Parameters.AngInteractRange;
            float angularInteractionRangeLarge = currentAgent.Parameters.AngInteractRangeLarge;
            float repulsiveStrength = currentAgent.Parameters.ObstacleRepulsiveStrength;

            obstacleRepulsiveForce += CalculateRepulsive(currentAgent.transform.position, currentAgent.Velocity, obstacle.transform.position, Vector3.zero, range, directionWeight, rangeDirectionFactor, angularInteractionRange, angularInteractionRangeLarge, repulsiveStrength); //Obstacles do not move, they have a velocity of 0
        }
        return obstacleRepulsiveForce;
    }

    private Vector3 CalculateRepulsive(Vector3 agentPosition, Vector3 agentVelocity, Vector3 otherPosition, Vector3 otherVelocity, float range, float dirWeight, float rangeDirFactor, float angInterRange, float angInterRangeLarge, float repStrength)
    {
        /*const float directionWeight = 2.0f;
        const float rangeDirectionFactor = 0.40f;
        const float angularInteractionRangeLarge = 3.0f;
        const float angularInteractionRange = 2.0f;
        const float repulsiveStrength = 47f;*/

        Vector3 interactionForce = new Vector3(0f, 0f, 0f);

        Vector3 vectorToAgent = new Vector3();
        vectorToAgent = otherPosition - agentPosition;

        // Skip if agent is too far
        if (Vector3.SqrMagnitude(vectorToAgent) > range * range)
        {
            return Vector3.zero;
        }

        Vector3 directionToAgent = vectorToAgent.normalized;
        Vector3 interactionVector = dirWeight * (agentVelocity - otherVelocity) + directionToAgent;

        float interactionRange = rangeDirFactor * Vector3.Magnitude(interactionVector);

        Vector3 interactionDir = interactionVector.normalized;

        float interactionAngle = Mathf.Deg2Rad * Vector3.Angle(interactionDir, directionToAgent);
        int angleSign;
        if (interactionAngle == 0) angleSign = 0;
        else if (interactionAngle > 0) angleSign = 1;
        else angleSign = -1;

        float distanceToAgent = Vector3.Magnitude(vectorToAgent);

        float deceleration = -repStrength * Mathf.Exp(-distanceToAgent / interactionRange - (angInterRangeLarge * interactionRange * interactionAngle) * (angInterRangeLarge * interactionRange * interactionAngle));
        float directionalChange = -repStrength * angleSign * Mathf.Exp(-distanceToAgent / interactionRange - (angInterRange * interactionRange * interactionAngle) * (angInterRange * interactionRange * interactionAngle));
        Vector3 normalInteractionVector = new Vector3(-interactionDir.z, interactionDir.y, interactionDir.x);
        //Vector3 normalInteractionVector = new Vector3(-interactionDir.y, interactionDir.x, 0);

        interactionForce += deceleration * interactionDir + directionalChange * normalInteractionVector;
        return interactionForce;
    }

    public Vector3 CalculateWallRepulsiveForce(Vector3 agentPosition, float agentRadius, float forceRange, float forceStrength)
    {       
        float squaredDistToObject = Mathf.Infinity;
        
        float minSquaredDist = Mathf.Infinity;
        Vector3 minDistVector = new Vector3();

        // Find distance to nearest wall
        foreach (Wall wall in m_Walls)
        {
            Vector3 vectorNearestPointToAgent = agentPosition - wall.GetNearestPoint(agentPosition);
            float squaredDist = Vector3.SqrMagnitude(vectorNearestPointToAgent);

            if (squaredDist < minSquaredDist)
            {
                minSquaredDist = squaredDist;
                minDistVector = vectorNearestPointToAgent;
                squaredDistToObject = squaredDist;
            }
        }

        
        float distToNearestObs = Mathf.Sqrt(squaredDistToObject) - agentRadius;

        float interactionForce = forceStrength * Mathf.Exp(-distToNearestObs / forceRange);

        minDistVector.Normalize();
        minDistVector.y = 0;
        Vector3 obsInteractForce = interactionForce * minDistVector.normalized;

        return obsInteractForce;
    }

    public Vector3 CalculateAttractive(Vector3 agentPosition, float agentRadius, float forceRange, float forceStrength)
    {
        Vector3 repulsiveForce = new Vector3();
        foreach (GameObject attractor in m_Attractors)
        {
            Vector3 vectorToAgent = attractor.transform.position - agentPosition;
            if(vectorToAgent.sqrMagnitude > forceRange * forceRange)
            {
                continue;
            }
            float distanceToObstacle = vectorToAgent.magnitude;
            vectorToAgent.Normalize();
            repulsiveForce += forceStrength * Mathf.Exp((distanceToObstacle - agentRadius) / forceRange) * vectorToAgent;
        }
        repulsiveForce.y = 0.0f;
        return repulsiveForce;
    }

    public void StartRecording()
    {
        m_CanRecord = true;
        m_NextDestIndex = 0;
    }

    public bool IsRecording()
    {
        return m_CanRecord;
    }

    public void RecordTime(int destIndex, float time)
    {
        m_RecordedTimes[destIndex] = time;
        ++m_RecordedCount;
        if(m_RecordedCount == m_Destinations.Count + 1)
        {
            Debug.Log("Recording complete");
            m_CanRecord = false;
        }
    }
}
