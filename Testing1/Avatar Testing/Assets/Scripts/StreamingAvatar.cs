using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Avatar2;
using Photon.Pun;
using System;


public class StreamingAvatar : OvrAvatarEntity
{
    public OculusStuff networkCon;
    PhotonView view;
    // Start is called before the first frame update

    protected override void Awake()
    {
        StartLoadingAvatar();
        base.Awake();
    }

    public void StartAvatar(OculusStuff ncon)
    {
            networkCon = ncon;
            _userId = networkCon._userId;
            StartCoroutine(LoadAvatarWithId());
    }

    public void StartLoadingAvatar()
    {

        PhotonView parentView = GetComponentInParent<PhotonView>();
        object[] args = parentView.InstantiationData;
        Int64 avatarId = (Int64)args[0];
        _userId = Convert.ToUInt64(avatarId);
        
        view = GetComponent<PhotonView>();
        
        if (view.IsMine)
        {
                SetIsLocal(true);
                _creationInfo.features = Oculus.Avatar2.CAPI.ovrAvatar2EntityFeatures.Preset_Default;
        }
        else
        {
                SetIsLocal(false);
                _creationInfo.features = Oculus.Avatar2.CAPI.ovrAvatar2EntityFeatures.Preset_Remote;
        }

        StartCoroutine(LoadAvatarWithId());
    }

    IEnumerator LoadAvatarWithId()
    {
            var hasAvatarRequest = OvrAvatarManager.Instance.UserHasAvatarAsync(_userId);
            while (!hasAvatarRequest.IsCompleted) { yield return null; }
            LoadUser();
    }
}
