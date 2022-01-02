using System.Collections.Generic;
using UnityEngine;

public class SFManager : MonoBehaviour
{
    private List<SFCharacter> m_Agents = new List<SFCharacter>();
    private List<SFObstacle> m_Obstacles = new List<SFObstacle>();
    private List<Wall> m_Walls = new List<Wall>();
    private List<GameObject> m_Destinations = new List<GameObject>();
    private int m_NextDestIndex = 0;

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

    public Vector3 CalculateRepulsiveForce(SFCharacter currentAgent)
    {
        Vector3 agentRepulsive = CalculateAgentRepulsiveForce(currentAgent);
        Vector3 obstacleRepulsive = CalculateObstacleRepulsiveForce(currentAgent);
        Vector3 wallRepulsive = CalculateWallRepulsiveForce(currentAgent);

        return (agentRepulsive + obstacleRepulsive + wallRepulsive);
    }

    private Vector3 CalculateAgentRepulsiveForce(SFCharacter currentAgent)
    {
        // Basic implementation - Not realistic enough (directional turning issues)

        //Vector3 repulsiveForce = new Vector3();
        //foreach(SFObstacle obstacle in m_Obstacles)
        //{
        //    Vector3 vectorToAgent = currentAgent.transform.position - obstacle.transform.position;
        //    float distanceToObstacle = vectorToAgent.magnitude;
        //    float angleBetween = Vector3.Angle(vectorToAgent, currentAgent.transform.forward);
        //    const float compareEpsilon = 0.1f;
        //    if(Mathf.Abs(angleBetween) < compareEpsilon)
        //    {
        //        repulsiveForce.z += 0.2f;
        //    }

        //    vectorToAgent.Normalize();
        //    repulsiveForce += (currentAgent.ObstacleRepulsiveStrength) * Mathf.Exp((currentAgent.Radius - distanceToObstacle) / currentAgent.ObstacleRepulsiveRange) * vectorToAgent;
        //}
        //repulsiveForce.y = 0.0f;
        //return repulsiveForce;

        // Improved implementation provided by framework
        // Model parameters derived by Moussaïd et. al as described in "Experimental study of the behavioural mechanisms underlying self-organization in human crowds"
        /*const float directionWeight = 2.0f;
        const float gamma = 0.35f;
        const float nPrime = 3.0f;
        const float n = 2.0f;

        const float repulsiveStrength = 47f;
        float B, interactionAngle;
        int angleSign;
        Vector3 interactionForce = new Vector3(0f, 0f, 0f);

        Vector3 vectorToAgent = new Vector3();

        foreach (SFObstacle obstacle in m_Obstacles)
        {
            // Skip if agent is self
            //if (obstacle == this) continue;

            vectorToAgent = obstacle.transform.position - currentAgent.transform.position;

            // Skip if agent is too far
            if (Vector3.SqrMagnitude(vectorToAgent) > 10f * 10f) continue;

            Vector3 directionToAgent = vectorToAgent.normalized;
            Vector3 interactionVector = directionWeight * (currentAgent.Velocity) + directionToAgent;

            B = gamma * Vector3.Magnitude(interactionVector);

            Vector3 interactionDir = interactionVector.normalized;

            interactionAngle = Mathf.Deg2Rad * Vector3.Angle(interactionDir, directionToAgent);

            if (interactionAngle == 0) angleSign = 0;
            else if (interactionAngle > 0) angleSign = 1;
            else angleSign = -1;

            float distanceToAgent = Vector3.Magnitude(vectorToAgent);

            float deceleration = -repulsiveStrength * Mathf.Exp(-distanceToAgent / B - (nPrime * B * interactionAngle) * (nPrime * B * interactionAngle));
            float directionalChange = -repulsiveStrength * angleSign * Mathf.Exp(-distanceToAgent / B - (n * B * interactionAngle) * (n * B * interactionAngle));
            Vector3 normalInteractionVector = new Vector3(-interactionDir.z, interactionDir.y, interactionDir.x);
            //Vector3 normalInteractionVector = new Vector3(-interactionDir.y, interactionDir.x, 0);

            interactionForce += deceleration * interactionDir + directionalChange * normalInteractionVector;
        }
        return interactionForce;*/

        Vector3 agentRepulsiveForce = new Vector3();
        foreach (SFCharacter agent in m_Agents)
        {
            if(agent == currentAgent)
            {
                continue;
            }

            agentRepulsiveForce += CalculateRepulsive(currentAgent, agent.transform.position);
        }
        return agentRepulsiveForce;
    }

    private Vector3 CalculateObstacleRepulsiveForce(SFCharacter currentAgent)
    {
        Vector3 obstacleRepulsiveForce = new Vector3();
        foreach (SFObstacle obstacle in m_Obstacles)
        {
            obstacleRepulsiveForce += CalculateRepulsive(currentAgent, obstacle.transform.position);
        }
        return obstacleRepulsiveForce;
    }

    private Vector3 CalculateRepulsive(SFCharacter currentAgent, Vector3 otherPosition)
    {
        const float directionWeight = 2.0f;
        //const float gamma = 0.35f;
        const float gamma = 0.80f;
        const float nPrime = 3.0f;
        const float n = 2.0f;

        const float repulsiveStrength = 47f;
        float B, interactionAngle;
        int angleSign;
        Vector3 interactionForce = new Vector3(0f, 0f, 0f);

        Vector3 vectorToAgent = new Vector3();
        vectorToAgent = otherPosition - currentAgent.transform.position;

        // Skip if agent is too far
        const int minDistance = 10;
        if (Vector3.SqrMagnitude(vectorToAgent) > minDistance * minDistance)
        {
            return Vector3.zero;
        }

        Vector3 directionToAgent = vectorToAgent.normalized;
        Vector3 interactionVector = directionWeight * (currentAgent.Velocity) + directionToAgent;

        B = gamma * Vector3.Magnitude(interactionVector);

        Vector3 interactionDir = interactionVector.normalized;

        interactionAngle = Mathf.Deg2Rad * Vector3.Angle(interactionDir, directionToAgent);

        if (interactionAngle == 0) angleSign = 0;
        else if (interactionAngle > 0) angleSign = 1;
        else angleSign = -1;

        float distanceToAgent = Vector3.Magnitude(vectorToAgent);

        float deceleration = -repulsiveStrength * Mathf.Exp(-distanceToAgent / B - (nPrime * B * interactionAngle) * (nPrime * B * interactionAngle));
        float directionalChange = -repulsiveStrength * angleSign * Mathf.Exp(-distanceToAgent / B - (n * B * interactionAngle) * (n * B * interactionAngle));
        Vector3 normalInteractionVector = new Vector3(-interactionDir.z, interactionDir.y, interactionDir.x);
        //Vector3 normalInteractionVector = new Vector3(-interactionDir.y, interactionDir.x, 0);

        interactionForce += deceleration * interactionDir + directionalChange * normalInteractionVector;
        return interactionForce;
    }

    private Vector3 CalculateWallRepulsiveForce(SFCharacter currentAgent)
    {
        const float repulsiveStrength = 1.0f;
        const float range = 0.8f;

        float squaredDistToObject = Mathf.Infinity;
        
        float minSquaredDist = Mathf.Infinity;
        Vector3 minDistVector = new Vector3();

        // Find distance to nearest obstacles
        foreach (Wall wall in m_Walls)
        {
            Vector3 vectorToNearestPoint = currentAgent.transform.position - wall.GetNearestPoint(currentAgent.transform.position);
            float squaredDist = Vector3.SqrMagnitude(vectorToNearestPoint);

            if (squaredDist < minSquaredDist)
            {
                minSquaredDist = squaredDist;
                minDistVector = vectorToNearestPoint;
                squaredDistToObject = squaredDist;
            }
        }

        
        float distToNearestObs = Mathf.Sqrt(squaredDistToObject) - currentAgent.Radius;

        float interactionForce = repulsiveStrength * Mathf.Exp(-distToNearestObs / range);

        minDistVector.Normalize();
        minDistVector.y = 0;
        Vector3 obsInteractForce = interactionForce * minDistVector.normalized;

        return obsInteractForce;
    }
}
