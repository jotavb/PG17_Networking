/*
    Copyright (C) Vitor Brito, 2020
*/
using GameSavvy.Byterizer;
using UnityEngine.AI;
using UnityEngine;

namespace LLNet
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NetPlayer : MonoBehaviour
    {
        private bool _isMine = false;
        public bool IsMine { get {  return _isMine; } set { _isMine = value; }}

        private LLClient _client = null;
        private NavMeshAgent _agent = null;
        private Camera _mainCamera = null;

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if(IsMine == false) return;

            if(Input.GetMouseButtonDown(0))
            {
                Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition); 
                if ( Physics.Raycast (ray, out RaycastHit hit)) 
                {
                    _agent.SetDestination(hit.point);
                    ByteStream data = new ByteStream();
                    data.Encode
                    (
                        (byte)NetMessageType.GAMEPLAY_SETDESTINATION,
                        hit.point
                    );
                    // Send GAMEPLAY_SETDESTINATION message
                    _client.SendNetMessage(_client.ReliableChannel, data.ToArray());
                }
            }
        }

        public void SetDestination(Vector3 destination)
        {
            _agent.SetDestination(destination);
        }

        public void SetClient(LLClient client)
        {
            _client = client;
        }
    }
}
