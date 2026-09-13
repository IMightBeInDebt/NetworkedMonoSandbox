using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace MonoSandbox.Networking
{
    public class SandboxNetworking : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public static SandboxNetworking Instance;

        private void Awake()
        {
            Instance = this;
        }

        public void OnEvent(EventData photonEvent)
        {
            Player sender = PhotonNetwork.CurrentRoom?.GetPlayer(photonEvent.Sender);

            if (SandboxNet.TryDecode(photonEvent.Code, out SandboxEventCode code))
            {
                if (!SandboxNet.PlayerHasSandbox(sender)) return; // ignore anyone without the menu / sandbox off
                if (!(photonEvent.CustomData is object[] data)) return;

                switch (code)
                {
                    case SandboxEventCode.Spawn:
                    case SandboxEventCode.FullSyncObject:
                        HandleRemoteSpawn(photonEvent.Sender, data);
                        break;
                    case SandboxEventCode.Delete:
                        HandleRemoteDelete(data);
                        break;
                    case SandboxEventCode.ToggleFlag:
                        HandleRemoteToggle(data);
                        break;
                    case SandboxEventCode.FullSyncRequest:
                        HandleFullSyncRequest(sender);
                        break;
                }
                return;
            }
            
            if (WeaponNet.TryDecodeProjectile(photonEvent.Code, photonEvent.CustomData, out string spawnKey, out Vector3 point, out Vector3 normal, out object[] args))
            {
                if (!SandboxNet.PlayerHasSandbox(sender)) return;
                SandboxSpawnRegistry.Spawn(spawnKey, point, normal, args);
                return;
            }
            
            if (WeaponNet.TryDecodeColorize(photonEvent.Code, photonEvent.CustomData, out string netId, out Color color))
            {
                if (!SandboxNet.PlayerHasSandbox(sender)) return;

                GameObject obj = SandboxObjectRegistry.Get(netId);
                Renderer rend = obj != null ? obj.GetComponent<Renderer>() : null;
                if (rend != null) rend.material.color = color;
            }
        }

        // ---------------- Spawning ----------------
        
        public static void BroadcastSpawn(GameObject spawnedObject, string spawnKey, Vector3 point, Vector3 normal, object[] extraArgs)
        {
            if (spawnedObject == null) return;
            extraArgs ??= new object[0];

            string id = SandboxObjectRegistry.RegisterLocal(spawnedObject, spawnKey, point, normal, extraArgs);

            object[] payload = { id, spawnKey, point.x, point.y, point.z, normal.x, normal.y, normal.z, extraArgs };
            SandboxNet.Raise(SandboxEventCode.Spawn, payload);
        }

        private void HandleRemoteSpawn(int senderActor, object[] data)
        {
            string id = (string)data[0];
            string spawnKey = (string)data[1];
            var point = new Vector3((float)data[2], (float)data[3], (float)data[4]);
            var normal = new Vector3((float)data[5], (float)data[6], (float)data[7]);
            var extraArgs = (object[])data[8];

            if (SandboxObjectRegistry.Get(id) != null) return; // already known about this one

            GameObject obj = SandboxSpawnRegistry.Spawn(spawnKey, point, normal, extraArgs);
            if (obj != null)
            {
                obj.name += "_Net"; // just so it's obvious in the hierarchy this came from the network
                SandboxObjectRegistry.RegisterRemote(id, senderActor, spawnKey, point, normal, extraArgs, obj);
            }
        }

        // ---------------- Deleting ----------------

        public static void BroadcastDelete(string networkId)
        {
            if (string.IsNullOrEmpty(networkId)) return;
            SandboxNet.Raise(SandboxEventCode.Delete, new object[] { networkId });
            SandboxObjectRegistry.Remove(networkId);
        }

        private void HandleRemoteDelete(object[] data)
        {
            string id = (string)data[0];
            GameObject obj = SandboxObjectRegistry.Get(id);
            if (obj != null) Destroy(obj);
            SandboxObjectRegistry.Remove(id);
        }

        // ---------------- Toggling (freeze / gravity / etc on an EXISTING object) ----------------

        public static void BroadcastToggle(string networkId, string toggleType)
        {
            if (string.IsNullOrEmpty(networkId)) return;
            SandboxNet.Raise(SandboxEventCode.ToggleFlag, new object[] { networkId, toggleType });
        }

        private void HandleRemoteToggle(object[] data)
        {
            string id = (string)data[0];
            string toggleType = (string)data[1];

            GameObject obj = SandboxObjectRegistry.Get(id);
            if (obj == null) return;

            Rigidbody rb = obj.GetComponentInChildren<Rigidbody>();
            if (rb == null) return;

            switch (toggleType)
            {
                case "freeze":
                    rb.constraints = rb.constraints == RigidbodyConstraints.None ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
                    break;
                case "gravity":
                    rb.useGravity = !rb.useGravity;
                    break;
            }
        }

        // ---------------- Late-join ----------------
        
        public void RequestFullSync()
        {
            SandboxNet.Raise(SandboxEventCode.FullSyncRequest, new object[0]);
        }

        private void HandleFullSyncRequest(Player requester)
        {
            foreach (var kv in SandboxObjectRegistry.AllLocallyOwned())
            {
                var tag = kv.Value.GetComponent<NetworkedMonoObject>();
                if (tag == null) continue;

                object[] payload =
                {
                    tag.NetworkId, tag.SpawnKey,
                    tag.SpawnPoint.x, tag.SpawnPoint.y, tag.SpawnPoint.z,
                    tag.SpawnNormal.x, tag.SpawnNormal.y, tag.SpawnNormal.z,
                    tag.SpawnArgs,
                };
                SandboxNet.RaiseTo(SandboxEventCode.FullSyncObject, payload, requester);
            }
        }

        // ---------------- Cleanup on leave ----------------

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            foreach (string id in SandboxObjectRegistry.IdsOwnedBy(otherPlayer.ActorNumber))
            {
                GameObject obj = SandboxObjectRegistry.Get(id);
                if (obj != null) Destroy(obj);
                SandboxObjectRegistry.Remove(id);
            }
        }
    }
}
