using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTest : MonoBehaviour
{
    public float time;
    Animation anim;
    // Use this for initialization
    void Start()
    {
        anim = GetComponent<Animation>();
        MoveAnimation(new Vector3(0f, 0f, 0f), new Vector3(5f, 0f, 15f), 15f);
    }

    public void MoveAnimation(Vector3 startPos, Vector3 endPos, float animationTime)
    {
        AnimationCurve curveX = new AnimationCurve(new Keyframe[2] { new Keyframe(0.0f, startPos.x), new Keyframe(animationTime, endPos.x) });
        AnimationCurve curveY = new AnimationCurve(new Keyframe[2] { new Keyframe(0.0f, startPos.y), new Keyframe(animationTime, endPos.y) });
        AnimationCurve curveZ = new AnimationCurve(new Keyframe[2] { new Keyframe(0.0f, startPos.z), new Keyframe(animationTime, endPos.z) });

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
    }
}
