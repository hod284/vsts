using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public enum SelectColor
{
    wramming, fail, sucess
}

public class BroadcastMessage : NetworkBehaviour
{
    private float speed=1.0f;
    [SerializeField] private Image BackgroundImage;
    [SerializeField] private Text ShowingMessage;
   /// <summary>
   /// 나빼고 전부에게 메세지 전송
   /// </summary>
   /// <param name="selectColor"></param>
   /// <param name="text"></param>
    public void SendBroadCast_withoutme(SelectColor selectColor, string text)
    {
        AllClientRecciveBroadCastwithoutme_ServerRpc(selectColor, text, new ServerRpcParams());
    }
    /// <summary>
    /// 모든 클라이언트 에게 전송
    /// </summary>
    /// <param name="selectColor"></param>
    /// <param name="text"></param>
    public void SendBroadCast_allclient(SelectColor selectColor, string text)
    {
        AllClientRecciveBroadCast_ServerRpc(selectColor, text);
    }
    /// <summary>
    /// 호스트에게만 전송
    /// </summary>
    /// <param name="selectColor"></param>
    /// <param name="text"></param>
    public void SendBroadCast_onlyhost(SelectColor selectColor, string text)
    {
        RecciveBroadCastonlyHost_ServerRpc(selectColor, text);
    }
    /// <summary>
    /// 한 클라이언트에게만 전송
    /// </summary>
    /// <param name="selectColor"></param>
    /// <param name="text"></param>
    /// <param name="ArrmyNumber"></param>
    public void SendBroadCast_onlyone(SelectColor selectColor, string text,string ArrmyNumber)
    {
        OnlyOneClientRecciveBroadCast_ServerRpc(selectColor, text, ArrmyNumber);
    }
    [ClientRpc]
    private void AllClientSendBroadCastwithoutme_ClientRpc(SelectColor selectColor, string text, ClientRpcParams clientRpcParams = default) 
    {
        Debug.Log("clientwithoutme_client");
        SetMessage(selectColor, text);
    }
    [ClientRpc]
    private void AllClientSendBroadCast_ClientRpc(SelectColor selectColor, string text)
    {
        Debug.Log("allclient_client");
        SetMessage(selectColor, text);
    }
    [ClientRpc]
    private void OnlyOneClientSendBroadCast_ClientRpc(SelectColor selectColor, string text, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("onlyoneclient_client");
        SetMessage(selectColor, text);
    }
    [ServerRpc(RequireOwnership = false)]
    private void AllClientRecciveBroadCastwithoutme_ServerRpc(SelectColor selectColor, string text, ServerRpcParams serverRpcParam = default)
    {
        Debug.Log("clientwithoutme_sever");
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                // 제외할 클라이언트의 ID를 설정
                TargetClientIds = NetworkManager.ConnectedClientsIds.Where(id => id != serverRpcParam.Receive.SenderClientId).ToArray()
            }
        };
      
        AllClientSendBroadCastwithoutme_ClientRpc(selectColor, text,clientRpcParams);
    }
    [ServerRpc(RequireOwnership = false)]
    private void AllClientRecciveBroadCast_ServerRpc(SelectColor selectColor, string text)
    {
        Debug.Log("allclient_sever");
        AllClientSendBroadCast_ClientRpc(selectColor, text);
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnlyOneClientRecciveBroadCast_ServerRpc(SelectColor selectColor, string text, string ArrmyNumber)
    {
        Debug.Log("onlyoneclient_sever");
        var client = GameObject.FindAnyObjectByType<NetworkHostManager>().PlayerInformationlist_Public.Find(x => x.GetArmyNumber == ArrmyNumber);
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                // 제외할 클라이언트의 ID를 설정
                TargetClientIds =new ulong[1] { client .GetClientid}
            }
        };
        OnlyOneClientSendBroadCast_ClientRpc(selectColor, text, clientRpcParams);
    }
    [ServerRpc(RequireOwnership = false)]
    private void RecciveBroadCastonlyHost_ServerRpc(SelectColor selectColor, string text)
    {
        Debug.Log("onlyhost");
        SetMessage(selectColor, text);
    }
  
    private void SetMessage(SelectColor selectColor,string text)
    {
        switch (selectColor)
        {
            case SelectColor.wramming:
                BackgroundImage.color = Color.red;
                break;
            case SelectColor.sucess:
                BackgroundImage.color = Color.red;
                break;
            case SelectColor.fail:
                BackgroundImage.color = Color.red;
                break; 
        }
        ShowingMessage.text = text;
        StartCoroutine(blingke());
    }
    private IEnumerator blingke()
    {
        int time = 0;
        float alpha = 0.0f;
        while (time<600)
        {
            time += 1;
            alpha = Mathf.PingPong(Time.time * speed, 1.0f);
            BackgroundImage.color = new Color(BackgroundImage.color.r, BackgroundImage.color.g, BackgroundImage.color.b, alpha);
            ShowingMessage.color = new Color(ShowingMessage.color.r, ShowingMessage.color.g, ShowingMessage.color.b, alpha);
            yield return null;
        }
        BackgroundImage.color = new Color(BackgroundImage.color.r, BackgroundImage.color.g, BackgroundImage.color.b, 0.0f);
        ShowingMessage.color = new Color(ShowingMessage.color.r, ShowingMessage.color.g, ShowingMessage.color.b, 0.0f);
    }
}
