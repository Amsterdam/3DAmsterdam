using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireworkAnimationScript : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform player;

    private Animator animator;

    [SerializeField]
    private float distance = 8;

    private bool active = false;

    [SerializeField]
    private GameObject Text;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void EnableScript(Transform player) 
    {
        active = true;
        this.player = player;
        Text.SetActive(true);

    }

    // Update is called once per frame
    void Update()
    {
        if (active) 
        {
            Text.transform.LookAt(player.transform);
            Text.transform.Rotate(new Vector3(0, 180, 0));
            float dist = Vector3.Distance(transform.position, player.transform.position);
            // verander text op basis van afstand
            if (dist > distance) 
            {
                active = false;
                Text.SetActive(false);
                animator.SetBool("MovedBack", true);
            }
        }
    }
}
