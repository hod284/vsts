using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIButtonClickScript : MonoBehaviour
{
    [SerializeField] private int _SelectIndex = 0;
    [SerializeField] private UIItemInformationManager _IFmanager;
    public void SetIndex(int n ) => _SelectIndex = n;
    public void Showinginformation()
    {
        _IFmanager.SetActiveExplainOB(_SelectIndex);
    }
}
