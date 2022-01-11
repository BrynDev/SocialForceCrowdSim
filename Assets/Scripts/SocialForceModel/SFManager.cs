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
        foreach (SFCharacter agent in m_Agents)
        {
            if(agent == currentAgent)
            {
                continue;
            }

            /*float directionWeight = dirWeight;
            float rangeDirectionFactor = rangeDirFactor;
            float angularInteractionRange = angInterRange;
            float angularInteractionRangeLarge = angInterRangeLarge;
            float repulsiveStrength = repStrength;*/

            agentRepulsiveForce += CalculateRepulsive(currentAgent, agent.transform.position, agent.Velocity);
        }
        return agentRepulsiveForce;
    }

    public Vector3 CalculateObstacleRepulsiveForce(SFCharacter currentAgent)
    {
        Vector3 obstacleRepulsiveForce = new Vector3();
        foreach (SFObstacle obstacle in m_Obstacles)
        {
            obstacleRepulsiveForce += CalculateRepulsive(currentAgent, obstacle.transform.position, Vector3.zero); //Obstacles do not move, they have a velocity of 0
        }
        return obstacleRepulsiveForce;
    }

    private Vector3 CalculateRepulsive(SFCharacter currentAgent, Vector3 otherPosition, Vector3 otherVelocity/*, float dirWeight, float rangeDirFactor, float angInterRange, float angInterRangeLarge, float repStrength*/)
    {
        const float directionWeight = 2.0f;
        const float rangeDirectionFactor = 0.40f;
        const float angularInteractionRangeLarge = 3.0f;
        const float angularInteractionRange = 2.0f;
        const float repulsiveStrength = 47f;

        /*float directionWeight = dirWeight;
        float rangeDirectionFactor = rangeDirFactor;
        float angularInteractionRange = angInterRange;
        float angularInteractionRangeLarge = angInterRangeLarge;     
        float repulsiveStrength = repStrength;*/

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
        Vector3 interactionVector = directionWeight * (currentAgent.Velocity - otherVelocity) + directionToAgent;

        float interactionRange = rangeDirectionFactor * Vector3.Magnitude(interactionVector);

        Vector3 interactionDir = interactionVector.normalized;

        float interactionAngle = Mathf.Deg2Rad * Vector3.Angle(interactionDir, directionToAgent);
        int angleSign;
        if (interactionAngle == 0) angleSign = 0;
        else if (interactionAngle > 0) angleSign = 1;
        else angleSign = -1;

        float distanceToAgent = Vector3.Magnitude(vectorToAgent);

        float deceleration = -repulsiveStrength * Mathf.Exp(-distanceToAgent / interactionRange - (angularInteractionRangeLarge * interactionRange * interactionAngle) * (angularInteractionRangeLarge * interactionRange * interactionAngle));
        float directionalChange = -repulsiveStrength * angleSign * Mathf.Exp(-distanceToAgent / interactionRange - (angularInteractionRange * interactionRange * interactionAngle) * (angularInteractionRange * interactionRange * interactionAngle));
        Vector3 normalInteractionVector = new Vector3(-interactionDir.z, interactionDir.y, interactionDir.x);
        //Vector3 normalInteractionVector = new Vector3(-interactionDir.y, interactionDir.x, 0);

        interactionForce += deceleration * interactionDir + directionalChange * normalInteractionVector;
        return interactionForce;
    }

    public Vector3 CalculateWallRepulsiveForce(SFCharacter currentAgent)
    {
        float repulsiveStrength = currentAgent.Parameters.WallRepulsiveStrength;
        float repulsiveRange = currentAgent.Parameters.WallRepulsiveRange;

        float squaredDistToObject = Mathf.Infinity;
        
        float minSquaredDist = Mathf.Infinity;
        Vector3 minDistVector = new Vector3();

        // Find distance to nearest wall
        foreach (Wall wall in m_Walls)
        {
            Vector3 vectorNearestPointToAgent = currentAgent.transform.position - wall.GetNearestPoint(currentAgent.transform.position);
            float squaredDist = Vector3.SqrMagnitude(vectorNearestPointToAgent);

            if (squaredDist < minSquaredDist)
            {
                minSquaredDist = squaredDist;
                minDistVector = vectorNearestPointToAgent;
                squaredDistToObject = squaredDist;
            }
        }

        
        float distToNearestObs = Mathf.Sqrt(squaredDistToObject) - currentAgent.Radius;

        float interactionForce = repulsiveStrength * Mathf.Exp(-distToNearestObs / repulsiveRange);

        minDistVector.Normalize();
        minDistVector.y = 0;
        Vector3 obsInteractForce = interactionForce * minDistVector.normalized;

        return obsInteractForce;
    }
}
