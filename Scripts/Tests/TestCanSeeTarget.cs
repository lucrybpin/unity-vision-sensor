using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCanSeeTarget : MonoBehaviour
{
    [SerializeField] CharacterSensor sensor;
    [SerializeField] Collider target;

    private void LateUpdate ()
    {
        if (sensor.CanSeeTarget( target ))
        {
            Debug.Log("Can See");
        }
    }
}
