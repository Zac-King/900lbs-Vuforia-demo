using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceOffset : MonoBehaviour
{
    public Transform m_target;

    // Update is called once per frame
    void FixedUpdate()
    {
        float distance = Vector3.Distance(Camera.main.transform.position, m_target.position);
        transform.localPosition = new Vector3(0, 0, distance);
    }
}
