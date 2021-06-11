using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    public Transform m_target;
    public bool m_lookAt = true;
    // Update is called once per frame
    void FixedUpdate()
    {
        if(m_lookAt)
        {
            transform.LookAt(m_target);
        }
    }
}
