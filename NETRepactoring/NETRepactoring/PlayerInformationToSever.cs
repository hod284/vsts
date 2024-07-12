using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInformationToSever : MonoBehaviour
{
    [SerializeField] private string ArmyNumber = "Initial Value";
    [SerializeField] private string PlayerName = "Initial Value";
    [SerializeField] private string Classes = "Initial Value";
    public string GetArmyNumber { get => ArmyNumber; }
    public string GetPlayerName { get => PlayerName; }
    public string GetClasses { get => Classes; }
    public void SetArmyNumber(string armynumber)
    {
        ArmyNumber = armynumber;
    }
    public void SetPlayerName(string playername)
    {
        PlayerName = playername;
    }
    public void SetClasses(string classes)
    {
        Classes = classes;
    }
}
