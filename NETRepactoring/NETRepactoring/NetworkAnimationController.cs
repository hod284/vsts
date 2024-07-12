using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkAnimationController : NetworkBehaviour
{
    private string Triger = null;
    private string AnimationTrrigername = null;
    private int Inttriger = 0;
    private float Floattriger = 0;
    private bool Booltriger = false;
    [SerializeField] private Animator AnimationController;
    private void Start()
    {
        if (AnimationController == null)
        {
            AnimationController = GetComponent<Animator>();
        }
    }
    public void SeTTriger(string AnimationTrrigerName)
    {
        Settrriger_ServerRpc(AnimationTrrigerName);
    }
    public void SetIntTriger(string AnimationTrrigerName_int, int intvalue)
    {
        Setinttrriger_ServerRpc(AnimationTrrigerName_int, intvalue);
    }
    public void SeFloatTriger(string AnimationTrrigerName_float, float floatvalue)
    {
        Setfloattrriger_ServerRpc(AnimationTrrigerName_float, floatvalue);
    }
    public void SeBoolTriger(string AnimationTrrigerName_bool, bool boolvalue)
    {
        Setbooltrriger_ServerRpc(AnimationTrrigerName_bool, boolvalue);
    }
    [ClientRpc]
    private void Settrriger_ClientRpc(string AnimationTrrigerName)
    {
        Triger = AnimationTrrigerName;
        AnimationController.SetTrigger(Triger);
    }
    [ClientRpc]
    private void Setinttrriger_ClientRpc(string AnimationTrrigerName_int, int intvalue)
    {
        AnimationTrrigername = AnimationTrrigerName_int;
        Inttriger = intvalue;
        AnimationController.SetInteger(AnimationTrrigername, Inttriger);
    }
    [ClientRpc]
    private void Setfloattrriger_ClientRpc(string AnimationTrrigerName_float, float floatvalue)
    {
        AnimationTrrigername = AnimationTrrigerName_float;
        Floattriger = floatvalue;
        AnimationController.SetFloat(AnimationTrrigername, Floattriger);
    }
    [ClientRpc]
    private void Setbooltrriger_ClientRpc(string AnimationTrrigerName_bool, bool boolvalue)
    {
        AnimationTrrigername = AnimationTrrigerName_bool;
        Booltriger = boolvalue;
        AnimationController.SetBool(AnimationTrrigername, Booltriger);
    }
    [ServerRpc(RequireOwnership = false)]
    private void Settrriger_ServerRpc(string AnimationTrrigerName)
    {
        Settrriger_ClientRpc(AnimationTrrigerName);
    }
    [ServerRpc(RequireOwnership = false)]
    private void Setinttrriger_ServerRpc(string AnimationTrrigerName_int, int intvalue)
    {
        Setinttrriger_ClientRpc(AnimationTrrigerName_int, intvalue);
    }
    [ServerRpc(RequireOwnership = false)]
    private void Setfloattrriger_ServerRpc(string AnimationTrrigerName_float, float floatvalue)
    {
        Setfloattrriger_ClientRpc(AnimationTrrigerName_float, floatvalue);
    }
    [ServerRpc(RequireOwnership = false)]
    private void Setbooltrriger_ServerRpc(string AnimationTrrigerName_bool, bool boolvalue)
    {
        Setbooltrriger_ClientRpc(AnimationTrrigerName_bool, boolvalue);
    }
}
