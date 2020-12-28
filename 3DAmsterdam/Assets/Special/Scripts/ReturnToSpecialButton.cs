using Amsterdam3D.CameraMotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReturnToSpecialButton : MonoBehaviour
{
    [SerializeField]
    private Button backToDamButton;

    [SerializeField]
    private Transform distanceReference;

    [SerializeField]
    private float triggerDistance = 300.0f;

    [SerializeField]
    private BillboardText billboardText;

    void Update()
    {
        //Activate button if distance is too far, and countdown is about to start
        backToDamButton.gameObject.SetActive(billboardText.countDownAboutToStart && Vector3.Distance(CameraModeChanger.Instance.ActiveCamera.transform.position, distanceReference.transform.position) > triggerDistance);
    }
}
