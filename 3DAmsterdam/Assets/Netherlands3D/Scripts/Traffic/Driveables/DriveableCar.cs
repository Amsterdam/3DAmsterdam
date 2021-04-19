using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriveableCar : MonoBehaviour
{
    [SerializeField]
    private HingeJoint wheelHingeFrontLeft;
    [SerializeField]
    private HingeJoint wheelHingeFrontRight;

    [SerializeField]
    private HingeJoint wheelSteerHingeFrontLeft;
    [SerializeField]
    private HingeJoint wheelSteerHingeFrontRight;

    private Quaternion startRotationFrontLeft;
    private Quaternion startRotationFrontRight;

    private float wheelMaxSteerAngle = 30.0f;

    private JointMotor leftWheelMotor;
    private JointMotor rightWheelMotor;

    private JointMotor leftWheelSteerMotor;
    private JointMotor rightWheelSteerMotor;

    [SerializeField]
    private float throttleForce = 10.0f;

    [SerializeField]
    private float steeringForce = 10.0f;

    [SerializeField]
    private float maxSpeed = 10.0f;

    [SerializeField]
    private float steeringSpeed = 10.0f;

    private void Start()
	{
        startRotationFrontLeft = wheelHingeFrontLeft.transform.localRotation;
        startRotationFrontRight = wheelHingeFrontRight.transform.localRotation;

        leftWheelMotor = wheelHingeFrontLeft.motor;
        rightWheelMotor = wheelHingeFrontRight.motor;

        leftWheelSteerMotor = wheelSteerHingeFrontLeft.motor;
        rightWheelSteerMotor = wheelSteerHingeFrontRight.motor;
    }

	void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        //Throttle
        leftWheelMotor.targetVelocity = -vertical * maxSpeed;
        rightWheelMotor.targetVelocity = vertical * maxSpeed;
        leftWheelMotor.force = throttleForce;
        rightWheelMotor.force = throttleForce;
        wheelHingeFrontLeft.motor = leftWheelMotor;
        wheelHingeFrontRight.motor = rightWheelMotor;

        //Steering
        leftWheelSteerMotor.targetVelocity = wheelMaxSteerAngle * horizontal * steeringSpeed;
        rightWheelSteerMotor.targetVelocity = wheelMaxSteerAngle * horizontal * steeringSpeed;
        leftWheelSteerMotor.force = steeringForce;
        rightWheelSteerMotor.force = steeringForce;
        wheelSteerHingeFrontLeft.motor = leftWheelSteerMotor;
        wheelSteerHingeFrontRight.motor = rightWheelSteerMotor;
    }
}
