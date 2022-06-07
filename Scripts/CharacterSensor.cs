using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu( "Mulx/AI/CharacterSensor" )]
public class CharacterSensor : MonoBehaviour {

    [System.Serializable]
    public struct DetectedObject {

        [SerializeField] Collider collider;
        [SerializeField] float angle;
        [SerializeField] float distance;
        [SerializeField] float visibility;

        public DetectedObject (Collider collider, float angle, float distance, float visibility = 0f)
        {
            this.collider = collider;
            this.angle = angle;
            this.distance = distance;
            this.visibility = visibility;
        }

        public Collider Collider { get => collider; }
        public float Angle { get => angle; }
        public float Distance { get => distance; }
        public float Visibility { get => visibility; set => visibility = value; }
    }

    [Header( "Sensor Configs" )]
    [SerializeField] LayerMask layerMask;
    [SerializeField] bool ignoreMyself = true;
    [SerializeField] List<Collider> ignoreList = new List<Collider>();
    [SerializeField] float sensorRange = 21f;

    [Header( "Vision" )]
    [SerializeField] Transform eyesTransform;
    [SerializeField] float visionRange = 21f;
    [Range( 0, 180 )]
    [SerializeField] float visionAngle = 21f;
    [Range( 0, 100 )]
    [SerializeField] float minFOVAnlePercentage = 30f;
    [Range( 0, 100 )]
    [SerializeField] float percentageToSee = 30f;
    [Range( 0, 100 )]
    [SerializeField] float minFOVDistancePercentage = 30f;

    [Header( "Detected Objects" )]
    [SerializeField] List<DetectedObject> detectedObjects = new List<DetectedObject>();

#if UNITY_EDITOR
    [Header( "Debug" )]
    public bool debugMode = false;
#endif

    public List<DetectedObject> DetectedObjects { get => detectedObjects; }

    public float SensorRange { get => sensorRange; }

    public float VisionRange { get => visionRange; }

    public float VisionAngle { get => visionAngle; }
    public Transform EyesTransform { get => eyesTransform; }

    [ExecuteInEditMode]
    void OnValidate ()
    {
        if (eyesTransform == null)
            eyesTransform = transform;
        visionRange = Mathf.Clamp( visionRange, 1f, sensorRange );
        DetectInRange();
    }

    private void Start ()
    {
        if (ignoreMyself)
        {
            Collider myCollider = this.GetComponent<Collider>();
            if (!ignoreList.Contains( myCollider ))
                ignoreList.Add( myCollider );
        }

#if UNITY_EDITOR
        StartCoroutine( DetectTargetsCO() );
#endif
    }

    private IEnumerator DetectTargetsCO ()
    {
        while (true)
        {
            yield return new WaitForSeconds( 0.25f );
            DetectInRange();
        }
    }

    [ContextMenu( "Detect In Range" )]
    public void DetectInRange ()
    {
        detectedObjects.Clear();
        Collider [ ] hitColliders = Physics.OverlapSphere( eyesTransform.position, sensorRange, layerMask );

        int collidersCount = hitColliders.Length;
        for (int i = 0; i < collidersCount; i++)
        {
            if (ignoreList.Contains( hitColliders [ i ] )) continue;

            float distanceToTarget = Vector3.Distance( hitColliders [ i ].transform.position, eyesTransform.position );
            Vector3 targetDirection = hitColliders [ i ].transform.position - eyesTransform.transform.position;
            float angleToSeeTarget = Vector3.Angle(
                    targetDirection,
                    eyesTransform.transform.forward );

            detectedObjects.Add( new DetectedObject( hitColliders [ i ], angleToSeeTarget, distanceToTarget ) );
        }

        if (detectedObjects.Count > 0)
            FilterVisibleObjects();
    }

    public void FilterVisibleObjects ()
    {
        int objectsCount = detectedObjects.Count;
        for (int i = 0; i < objectsCount; i++)
        {
            Ray ray = new Ray( eyesTransform.position, detectedObjects [ i ].Collider.transform.position - eyesTransform.position );
            List<RaycastHit> hitsList = new List<RaycastHit>();

            hitsList.AddRange( Physics.RaycastAll( ray, visionRange ) );
            hitsList.Sort( (a, b) => a.distance.CompareTo( b.distance ) );

            if (hitsList.Count == 0)
                continue;

            if (hitsList [ 0 ].collider.transform == this.transform)
                continue;

            if (hitsList [ 0 ].collider == detectedObjects [ i ].Collider)
            {
                if (detectedObjects [ i ].Angle <= visionAngle)
                {
                    if (detectedObjects [ i ].Distance > visionRange)
                        continue;
                    //y = mx + b
                    float angleVisibility = ( ( minFOVAnlePercentage - 100 ) / visionAngle ) * detectedObjects [ i ].Angle + 100;
                    float distanceVisibility = ( ( minFOVDistancePercentage - 100 ) / visionRange ) * detectedObjects [ i ].Distance + 100;
                    float finalVisibility = ( angleVisibility + distanceVisibility ) / 2;

                    detectedObjects [ i ] = new DetectedObject( detectedObjects [ i ].Collider, detectedObjects [ i ].Angle, detectedObjects [ i ].Distance, finalVisibility );
                }
            }
        }
    }

    public List<DetectedObject> GetVisibleObjects ()
    {
        List<DetectedObject> visibleObjects = new List<DetectedObject>();
        for (int i = 0; i < detectedObjects.Count; i++)
            if (detectedObjects [ i ].Visibility >= percentageToSee)
                visibleObjects.Add( detectedObjects [ i ] );

        return visibleObjects;
    }

    public bool CanSeeTarget (Collider target)
    {
        for (int i = 0; i < detectedObjects.Count; i++)
            if (detectedObjects [ i ].Collider == target)
                if (detectedObjects [ i ].Visibility >= percentageToSee)
                    return true;
        return false;
    }

    public bool CanSeeObjectWithTag (string tag, out DetectedObject objectFound)
    {
        objectFound = new DetectedObject();
        for (int i = 0; i < detectedObjects.Count; i++)
        {
            if (detectedObjects [ i ].Collider.gameObject.CompareTag( tag ) == true)
            {
                if (detectedObjects [ i ].Visibility >= percentageToSee)
                {
                    objectFound = detectedObjects [ i ];
                    return true;
                }
            }
        }
        return false;
    }

    public bool CanSeeObjectOfType<T> (out T objectFound) where T : class
    {
        objectFound = null;
        for (int i = 0; i < detectedObjects.Count; i++)
        {
            T x = detectedObjects [ i ].Collider.GetComponent<T>();
            if (x != null)
            {
                if (detectedObjects [ i ].Visibility >= percentageToSee)
                {
                    objectFound = x;
                    return true;
                }
            }
        }
        return false;
    }

}