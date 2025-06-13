using BNG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using VSTS;

public class InteractionKeyScript : MonoBehaviour
{
    private enum E_Interaction
    {
        Active,
        PartobjectRotate,
        PartobjectTranslate
    }
    [SerializeField] private GameObject _PartObject;
    [SerializeField] private E_Interaction _Interaction;
    [SerializeField] private Vector3 _GoalRotation;
    [SerializeField] private Vector3 _GoalPosition;
    private float _Speed=1.0f;
    private Vector3 _StartPoint;
    private Vector3 gab;
    private void Start()
    {
        _StartPoint = _PartObject.transform.localPosition;
        gab = _StartPoint - _GoalPosition;

    }

    private void FixedUpdate()
    {
        if (Input.GetMouseButton(2) || InputBridge.Instance.AButtonDown)
        {
            switch (_Interaction)
            { 
                case E_Interaction.Active:
                    _PartObject.gameObject.SetActive(true);
                break;
                case E_Interaction.PartobjectRotate:
                    _PartObject.transform.Rotate(_GoalRotation);
                break;
                case E_Interaction.PartobjectTranslate:
                    var di = Mathf.Abs(gab.magnitude);
                    Debug.Log(di.ToString());
                    float offset = Mathf.PingPong(Time.time*_Speed,di);
                    Debug.Log(offset.ToString());
                    _PartObject.transform.localPosition = _StartPoint + gab.normalized * offset; 
                break;
            }
        }
        else
            _PartObject.gameObject.SetActive(false);
    }
}
