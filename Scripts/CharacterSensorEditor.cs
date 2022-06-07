using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( CharacterSensor ) )]
public class CharacterSensorEditor : Editor
{

    void OnSceneGUI ()
    {
        CharacterSensor characterSensor = ( CharacterSensor ) target;
        if (characterSensor == null)
            return;

        DrawSensorBounds( characterSensor );

        DrawVisionBounds( characterSensor );

        for (int i = 0; i < characterSensor.DetectedObjects.Count; i++)
        {
            DrawDebugTexts( characterSensor, i );

            DrawVisionDetection( characterSensor, i );
        }
    }

    private static void DrawDebugTexts (CharacterSensor characterSensor, int i)
    {
        if (characterSensor.debugMode)
        {
            //Vector3 offset = ( characterSensor.transform.position - characterSensor.DetectedObjects [ i ].Collider.transform.position ).normalized * 2;
            Vector3 offset = 2 * Vector3.up;
            Vector3 handlePosition = characterSensor.DetectedObjects [ i ].Collider.transform.position + offset;

            GUIStyle style = new GUIStyle();

            style.normal.textColor = ( characterSensor.DetectedObjects [ i ].Visibility > 0 ) ? Color.red : Color.gray;

            Handles.Label( handlePosition,
                            "Dist: " + characterSensor.DetectedObjects [ i ].Distance.ToString( "F2" ) + "\n" +
                            "Angle: " + characterSensor.DetectedObjects [ i ].Angle.ToString( "F2" ) + "º\n" +
                            "Visibility: " + characterSensor.DetectedObjects [ i ].Visibility.ToString( "F2" ) + "%",
                            style );
        }
    }

    private static void DrawSensorBounds (CharacterSensor characterSensor)
    {
        if (characterSensor.debugMode)
        {
            Handles.color = Color.white;

            Handles.DrawWireDisc( characterSensor.transform.position, characterSensor.transform.up, characterSensor.SensorRange );
            //Handles.DrawWireDisc( characterSensor.transform.position, characterSensor.transform.right, characterSensor.SensorRange );
        }
    }

    private static void DrawVisionBounds (CharacterSensor characterSensor)
    {
        if (characterSensor.debugMode)
        {
            Handles.color = Color.white;

            Handles.DrawWireArc( characterSensor.transform.position, characterSensor.transform.up, characterSensor.transform.forward, -characterSensor.VisionAngle, characterSensor.VisionRange );
            Handles.DrawWireArc( characterSensor.transform.position, characterSensor.transform.up, characterSensor.transform.forward, characterSensor.VisionAngle, characterSensor.VisionRange );

            Quaternion leftRayRotation = Quaternion.AngleAxis( -characterSensor.VisionAngle, characterSensor.transform.up );
            Quaternion rightRayRotation = Quaternion.AngleAxis( characterSensor.VisionAngle, characterSensor.transform.up );
            Vector3 leftRayDirection = leftRayRotation * characterSensor.transform.forward;
            Vector3 rightRayDirection = rightRayRotation * characterSensor.transform.forward;
            Handles.DrawLine( characterSensor.transform.position, characterSensor.transform.position + ( leftRayDirection * characterSensor.VisionRange ) );
            Handles.DrawLine( characterSensor.transform.position, characterSensor.transform.position + ( rightRayDirection * characterSensor.VisionRange ) );

            //Handles.DrawWireArc( characterExample.transform.position, characterExample.transform.right, characterExample.transform.forward, -characterExample.VisionAngle, characterExample.VisionRange );
            //Handles.DrawWireArc( characterExample.transform.position, characterExample.transform.right, characterExample.transform.forward, characterExample.VisionAngle, characterExample.VisionRange );

            //Quaternion upRayRotation = Quaternion.AngleAxis( -characterExample.VisionAngle, characterExample.transform.right );
            //Quaternion downRayRotation = Quaternion.AngleAxis( characterExample.VisionAngle, characterExample.transform.right );
            //Vector3 upRayDirection = upRayRotation * characterExample.transform.forward;
            //Vector3 downRayDirection = downRayRotation * characterExample.transform.forward;
            //Handles.DrawLine( characterExample.transform.position, characterExample.transform.position + ( upRayDirection * characterExample.VisionRange ) );
            //Handles.DrawLine( characterExample.transform.position, characterExample.transform.position + ( downRayDirection * characterExample.VisionRange ) );
        }
    }

    private static void DrawVisionDetection (CharacterSensor characterSensor, int i)
    {
        Handles.color = ( characterSensor.DetectedObjects [ i ].Visibility > 0 ) ? Color.red : Color.gray;
        Handles.DrawLine( characterSensor.EyesTransform.position, characterSensor.DetectedObjects [ i ].Collider.transform.position );
    }

}
