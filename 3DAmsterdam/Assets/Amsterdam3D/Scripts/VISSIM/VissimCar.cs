using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VissimCar : MonoBehaviour
{
    public VissimData vehicleCommandData;
    public RaycastHit hit;
    private Vector3 startLastRecordedHeight = default;
    private Vector3 endLastRecordedHeight = default;
    
    public Vector3 futurePosition = default;

    private Animation anim = default;


    AnimationCurve curveX = default;
    AnimationCurve curveY = default;
    AnimationCurve curveZ = default;

    float animationStartTime;
    float currentAnimationTime = 0;


    private void Awake()
    {
        anim = GetComponent<Animation>();
    }

    private void Update()
    {
        
    }

    /// <summary>
    /// Parses the VISSIM data to the vehicle and executes it
    /// </summary>
    /// <param name="commandData"></param>
    public void ExecuteVISSIM(VissimData commandData)
    {
        vehicleCommandData = commandData;
        
        Vector3 temp = transform.position;
        temp.y = 50f;
        
        if (Physics.Raycast(temp, -Vector3.up, out hit, Mathf.Infinity))
        {
            startLastRecordedHeight = hit.point; // stores the last height position
            // puts the car on the road
            MoveObject(hit.point);
        }
        else
        {
            // puts the car on the road
            MoveObject(startLastRecordedHeight);
        }
        
    }

    /// <summary>/
    /// Moves the VISSIM Object
    /// </summary>
    public void MoveObject(Vector3 vectorCorrection)
    {
        // Corrects the vehicles Y position to the height of the map (Maaiveld)
        vehicleCommandData.coordRear.y = vectorCorrection.y;
        vehicleCommandData.coordFront.y = vectorCorrection.y; // misschien hier 2e raycast waarbij de auto naar dit punt kijkt? Alleen bij een weg die naar beneden gaat en hij nog boven staat krijg je dan rare artefacts
        
        // Moves the vehicle to the designated position
        transform.position = vehicleCommandData.coordRear;
        transform.LookAt(vehicleCommandData.coordFront);

        //Laat de autos lerpen via een animation curve
        // https://docs.unity3d.com/ScriptReference/AnimationClip.SetCurve.html
    }

    /// <summary>
    /// Moves the vehicle through animation
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <param name="animationTime"></param>
    public void MoveAnimation(Vector3 startPos, Vector3 endPos, float animationTime)
    {
        // checks for the height and places the car on the correct position on the map (maaiveld)
        Vector3 temp = transform.position;
        temp.y = 50f;

        if (Physics.Raycast(temp, -Vector3.up, out hit, Mathf.Infinity))
        {
            startLastRecordedHeight = hit.point; 
        }
        // checks the positional height of there the car will be in the future/next simulation second
        temp = endPos;
        temp.y = 50f;
        if (Physics.Raycast(temp, -Vector3.up, out hit, Mathf.Infinity))
        {
            endLastRecordedHeight = hit.point; 
        }

        vehicleCommandData.coordFront.y = startLastRecordedHeight.y;
        transform.LookAt(vehicleCommandData.coordFront);

        // Sets the animation curve for each vector element
        curveX = new AnimationCurve(new Keyframe[2] { new Keyframe(0.0f, startPos.x), new Keyframe(animationTime, endPos.x) });
        curveY = new AnimationCurve(new Keyframe[2] { new Keyframe(0.0f, startLastRecordedHeight.y), new Keyframe(animationTime, endLastRecordedHeight.y) });
        curveZ = new AnimationCurve(new Keyframe[2] { new Keyframe(0.0f, startPos.z), new Keyframe(animationTime, endPos.z) });


        // create a new AnimationClip
        AnimationClip clip = new AnimationClip();
        clip.legacy = true;

        // create a curve to move the GameObject and assign to the clip
        clip.SetCurve("", typeof(Transform), "localPosition.x", curveX);
        clip.SetCurve("", typeof(Transform), "localPosition.y", curveY);
        clip.SetCurve("", typeof(Transform), "localPosition.z", curveZ);

        // now animate the GameObject
        anim.AddClip(clip, clip.name);
        anim.Play(clip.name);

        //transform.position = new Vector3(curveX.Evaluate(animationTime), startPoint.point.y, curveZ.Evaluate(animationTime));

    }
}
