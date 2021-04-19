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
    private HingeJoint wheelHingeRearLeft;
    [SerializeField]
    private HingeJoint wheelHingeRearRight;

    private Quaternion startRotationFrontLeft;
    private Quaternion startRotationFrontRight;

    private float wheelMaxSteerAngle = 30.0f;

    private JointMotor leftWheelMotor;
    private JointMotor rightWheelMotor;

    private void Start()
	{
        startRotationFrontLeft = wheelHingeFrontLeft.transform.localRotation;
        startRotationFrontRight = wheelHingeFrontRight.transform.localRotation;

        leftWheelMotor = wheelHingeFrontLeft.motor;
        rightWheelMotor = wheelHingeFrontRight.motor;
    }

	void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        wheelHingeFrontLeft.transform.eulerAngles = new Vector3(wheelHingeFrontLeft.transform.localEulerAngles.x, startRotationFrontLeft.eulerAngles.y + (horizontal* wheelMaxSteerAngle), wheelHingeFrontLeft.transform.localEulerAngles.z);
        wheelHingeFrontRight.transform.eulerAngles = new Vector3(wheelHingeFrontLeft.transform.localEulerAngles.x, startRotationFrontRight.eulerAngles.y - (horizontal * wheelMaxSteerAngle), wheelHingeFrontLeft.transform.localEulerAngles.z);

        leftWheelMotor.targetVelocity = vertical;
        rightWheelMotor.targetVelocity = -vertical;

        wheelHingeFrontLeft.motor = leftWheelMotor;
        wheelHingeRearRight.motor = rightWheelMotor;
    }
}
