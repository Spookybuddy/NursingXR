namespace GIGXR.Platform.Networking.Utilities
{
    using Photon.Pun;
    using UnityEngine;

    /// <summary>
    /// Clone of Photon's PhotonTransformView. Allows us to tap into the serialization/deserialization of transform data.
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    public class PhotonLocalTransformView : MonoBehaviourPun, IPunObservable
    {
        // --- Private Variables:

        private const float minimumMoveDistance = 0.001f;

        private float m_Distance;
        private float m_Angle;

        private Vector3 m_Direction;
        private Vector3 m_NetworkPosition;
        private Vector3 m_StoredPosition;

        private Quaternion m_NetworkRotation;

        public bool m_SynchronizePosition = true;
        public bool m_SynchronizeRotation = true;
        public bool m_SynchronizeScale = true;

        [Tooltip("Indicates if localPosition and localRotation should be used. Scale ignores this setting, and always uses localScale to avoid issues with lossyScale.")]
        public bool m_UseLocal = true;

        bool m_firstTake = false;

        // --- Unity Methods:

        public void Awake()
        {
            m_StoredPosition = transform.localPosition;
            m_NetworkPosition = Vector3.zero;

            m_NetworkRotation = Quaternion.identity;

            m_firstTake = true;
        }

        private void Reset()
        {
            // Only default to true with new instances. useLocal will remain false for old projects that are updating PUN.
            m_UseLocal = true;

            photonView.OwnershipTransfer = OwnershipOption.Request;
        }

        public void Update()
        {
            if (!photonView.IsMine)
            {
                if (m_UseLocal)
                {
                    if (m_Distance != 0)
                    {
                        transform.localPosition = Vector3.MoveTowards(transform.localPosition,
                                                                      this.m_NetworkPosition,
                                                                      this.m_Distance * (1.0f / PhotonNetwork.SerializationRate));
                    }

                    if (this.m_Angle != 0)
                    {
                        transform.localRotation = Quaternion.RotateTowards(transform.localRotation,
                                                                           this.m_NetworkRotation,
                                                                           this.m_Angle * (1.0f / PhotonNetwork.SerializationRate));
                    }
                }
                else
                {
                    if (m_Distance != 0)
                    {
                        transform.position = Vector3.MoveTowards(transform.position,
                                                                 this.m_NetworkPosition,
                                                                 this.m_Distance * (1.0f / PhotonNetwork.SerializationRate));
                    }

                    if (m_Angle != 0)
                    {
                        transform.rotation = Quaternion.RotateTowards(transform.rotation,
                                                                      this.m_NetworkRotation,
                                                                      this.m_Angle * (1.0f / PhotonNetwork.SerializationRate));
                    }
                }
            }
        }

        private bool ignoreWrites = false;

        public void SuppressTransformWrite(bool value)
        {
            ignoreWrites = value;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            var tr = transform;

            // Write
            if (stream.IsWriting)
            {
                if (ignoreWrites)
                {
                    return;
                }

                if (this.m_SynchronizePosition)
                {
                    if (m_UseLocal)
                    {
                        this.m_Direction = tr.localPosition - this.m_StoredPosition;
                        this.m_StoredPosition = tr.localPosition;
                        stream.SendNext(tr.localPosition);
                        stream.SendNext(this.m_Direction);
                    }
                    else
                    {
                        this.m_Direction = tr.position - this.m_StoredPosition;
                        this.m_StoredPosition = tr.position;
                        stream.SendNext(tr.position);
                        stream.SendNext(this.m_Direction);
                    }
                }

                if (this.m_SynchronizeRotation)
                {
                    if (m_UseLocal)
                    {
                        stream.SendNext(tr.localRotation);
                    }
                    else
                    {
                        stream.SendNext(tr.rotation);
                    }
                }

                if (this.m_SynchronizeScale)
                {
                    stream.SendNext(tr.localScale);
                }
            }
            // Read
            else
            {
                if (this.m_SynchronizePosition)
                {
                    this.m_NetworkPosition = (Vector3)stream.ReceiveNext();
                    // If a client moves an Asset, there is a brief moment where they let go, the Host has ownership of the object, but the
                    // position data hasn't updated everywhere yet so it moves back for the client making the move. Make sure it reaches the goal here
                    this.m_StoredPosition = this.m_NetworkPosition;

                    this.m_Direction = (Vector3)stream.ReceiveNext();

                    if (m_firstTake)
                    {
                        if (m_UseLocal)
                            tr.localPosition = this.m_NetworkPosition;
                        else
                            tr.position = this.m_NetworkPosition;

                        this.m_Distance = 0f;
                    }
                    else
                    {
                        float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                        this.m_NetworkPosition += this.m_Direction * lag;
                        if (m_UseLocal)
                        {
                            this.m_Distance = Vector3.Distance(tr.localPosition, this.m_NetworkPosition);
                        }
                        else
                        {
                            this.m_Distance = Vector3.Distance(tr.position, this.m_NetworkPosition);
                        }
                    }

                }

                if (this.m_SynchronizeRotation)
                {
                    this.m_NetworkRotation = (Quaternion)stream.ReceiveNext();

                    if (m_firstTake)
                    {
                        this.m_Angle = 0f;

                        if (m_UseLocal)
                        {
                            tr.localRotation = this.m_NetworkRotation;
                        }
                        else
                        {
                            tr.rotation = this.m_NetworkRotation;
                        }
                    }
                    else
                    {
                        if (m_UseLocal)
                        {
                            this.m_Angle = Quaternion.Angle(tr.localRotation, this.m_NetworkRotation);
                        }
                        else
                        {
                            this.m_Angle = Quaternion.Angle(tr.rotation, this.m_NetworkRotation);
                        }
                    }
                }

                if (this.m_SynchronizeScale)
                {
                    tr.localScale = (Vector3)stream.ReceiveNext();
                }

                if (m_firstTake)
                {
                    m_firstTake = false;
                }
            }
        }        
    }
}