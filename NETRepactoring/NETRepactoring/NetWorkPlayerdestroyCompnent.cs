using BNG;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetWorkPlayerdestroyCompnent : NetworkBehaviour
{
    [SerializeField] BNGPlayerController bNGPlayerController;
    [SerializeField] PlayerGravity playerGravity;
    [SerializeField] PlayerRotation playerRotation;
    [SerializeField] SmoothLocomotion smoothLocomotion;
    [SerializeField] TrackedDevice Right_trackabledevice;
    [SerializeField] TrackedDevice Left_trackabledevice;
    [SerializeField] Transform Seconmodel;
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        var network = GetComponent<NetworkObject>();
        if (!network.IsLocalPlayer)
        {
            if (bNGPlayerController != null)
                Destroy(bNGPlayerController);
            if (playerGravity != null)
                Destroy(playerGravity);
            if (playerRotation != null)
                Destroy(playerRotation);
            if (smoothLocomotion != null)
                Destroy(smoothLocomotion);
            if (Right_trackabledevice != null)
                Destroy(Right_trackabledevice);
            if (Left_trackabledevice != null)
                Destroy(Left_trackabledevice);
        }

        if (network.IsOwner)
            gameObject.SetActive(false);
        else
        {
            if (Seconmodel != null)
                Seconmodel.localPosition = new Vector3(0, 2.5f, 0);
        }
    }
}
