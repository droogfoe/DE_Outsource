using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class TransformPlayModeKeepChange : MonoBehaviour
{
#if UNITY_EDITOR
    private void OnPlayModeStateChanged(PlayModeStateChange stateChange)
    {
        if (stateChange == PlayModeStateChange.ExitingPlayMode)
        {
            EditorPrefs.SetFloat(this.GetInstanceID() + "pos_x", transform.position.x);
            EditorPrefs.SetFloat(this.GetInstanceID() + "pos_y", transform.position.y);
            EditorPrefs.SetFloat(this.GetInstanceID() + "pos_z", transform.position.z);

            EditorPrefs.SetFloat(this.GetInstanceID() + "rot_x", transform.rotation.eulerAngles.x);
            EditorPrefs.SetFloat(this.GetInstanceID() + "rot_y", transform.rotation.eulerAngles.y);
            EditorPrefs.SetFloat(this.GetInstanceID() + "rot_z", transform.rotation.eulerAngles.z);
        }

        if (stateChange == PlayModeStateChange.EnteredEditMode)
        {
            Vector3 storePos = new Vector3();
            Vector3 storeAng = new Vector3();
            storePos.x = EditorPrefs.GetFloat(this.GetInstanceID() + "pos_x");
            storePos.y = EditorPrefs.GetFloat(this.GetInstanceID() + "pos_y");
            storePos.z = EditorPrefs.GetFloat(this.GetInstanceID() + "pos_z");

            storeAng.x = EditorPrefs.GetFloat(this.GetInstanceID() + "rot_x");
            storeAng.y = EditorPrefs.GetFloat(this.GetInstanceID() + "rot_y");
            storeAng.z = EditorPrefs.GetFloat(this.GetInstanceID() + "rot_z");

            transform.position = storePos;
            transform.rotation = Quaternion.Euler(storeAng);
        }
    }
#endif
    private void OnEnable()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }
    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
    }
}
