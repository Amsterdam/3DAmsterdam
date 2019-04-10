using UnityEngine;
using System.Collections;
namespace EnviroSamples
{
public class FPSController : MonoBehaviour {

	public float speed = 2f;
	public float sensitivity = 2f;
	CharacterController player;

	public GameObject eyes;

	float moveFB;
	float moveLR;

	float rotX;
	float rotY;

	// Use this for initialization
	void Start () {

		player = GetComponent<CharacterController> ();

	}

	// Update is called once per frame
	void Update () {

		moveFB = Input.GetAxis ("Vertical") * speed;
		moveLR = Input.GetAxis ("Horizontal") * speed;

		rotX = Input.GetAxis ("Mouse X") * sensitivity;
		rotY -= Input.GetAxis ("Mouse Y") * sensitivity;

		rotY = Mathf.Clamp (rotY, -60f, 60f);

		Vector3 movement = new Vector3 (moveLR, 0, moveFB);
		transform.Rotate (0, rotX, 0);
		eyes.transform.localRotation = Quaternion.Euler(rotY, 0, 0);
		//eyes.transform.Rotate (-rotY, 0, 0);

		movement = transform.rotation * movement;
        movement.y -= 4000f * Time.deltaTime;
        player.Move (movement * Time.deltaTime);

	}
	}
}