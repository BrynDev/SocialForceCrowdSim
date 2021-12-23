﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class SFCharacter : MonoBehaviour
{
    Vector3 velocity = new Vector3();

    AICharacterControl characterControl;
    NavMeshAgent characterAgent;
    float radius = 0.5f;

    [SerializeField] private Transform destination;
    [SerializeField] private float desiredSpeed = 0.5f;

    private float m_ObstacleRepulsiveStrength = 5.0f;
    private float m_ObstacleRepulsiveRange = 1.5f;
    private float m_AgentRepulsiveStrength = 45.0f;
    private float m_AgentRepulsiveRange = 5.0f;
    private float m_Radius = 0.5f;
    private SFManager m_SFManager;

    public float ObstacleRepulsiveStrength { get { return m_ObstacleRepulsiveStrength; } }
    public float ObstacleRepulsiveRange { get { return m_ObstacleRepulsiveRange; } }
    public float AgentRepulsiveStrength { get { return m_AgentRepulsiveStrength; } }
    public float AgentRepulsiveRange { get { return m_AgentRepulsiveRange; } }
    public float Radius { get { return m_Radius; } }
    public Vector3 Velocity { get { return velocity; } }

    // Start is called before the first frame update
    void Start()
    {
        characterControl = this.GetComponent<AICharacterControl>();
        characterAgent = this.GetComponent<NavMeshAgent>();
        if (this.destination == null)
        {
            this.destination = this.transform;
        }
        else if (characterControl)
        {
            characterControl.target.localPosition = velocity;
        }          
        if (characterAgent)
        {
            characterAgent.speed = desiredSpeed;
            characterAgent.SetDestination(destination.position);
            // Stop the navmesh agent from performing any movement - we want full manual control of agent movement
            characterAgent.isStopped = true;
        }

        //characterAgent.radius = radius;

        m_SFManager = GameObject.Find("SceneScripts").GetComponent<SFManager>();
        m_SFManager.AddAgent(this);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (characterAgent.remainingDistance < characterAgent.stoppingDistance)
                return;

        Vector3 acceleration = new Vector3();

        acceleration = DrivingForce() + m_SFManager.CalculateRepulsiveForce(this);
        velocity = acceleration * Time.deltaTime;

        // Limit maximum velocity
        if (Vector3.SqrMagnitude(velocity) > desiredSpeed * desiredSpeed)
        {
            velocity.Normalize();
            velocity *= desiredSpeed;
        }

       // Prevent inanimate objects that are marked as agents from moving
      
       transform.position += velocity;

        RotateToVelocity(velocity);
    }

    Vector3 DrivingForce()
    {
        const float relaxationT = 0.54f;

        //Vector3 desiredDirection = destination.transform.position - this.transform.position;
        Vector3 desiredDirection = characterAgent.steeringTarget - this.transform.position;
        desiredDirection.Normalize();

        Vector3 drivingForce = (desiredSpeed * desiredDirection - velocity) / relaxationT;

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

}
