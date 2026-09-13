using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace MonoSandbox.Networking
{
    public enum SandboxEventCode : byte
    {
        Spawn = 1,
        Delete = 2,
        ToggleFlag = 3,
        FullSyncRequest = 4,
        FullSyncObject = 5,
    }
    
    public static class SandboxNet
    {
        private const byte EventBase = 130;
        private const int MinCode = 1;
        private const int MaxCode = 5;

        private const string PropHasMenu = "MM_HasMenu";
        private const string PropSandboxOn = "MM_SandboxOn";

        public static byte CodeFor(SandboxEventCode code) => (byte)(EventBase + (byte)code);

        public static bool TryDecode(byte rawCode, out SandboxEventCode code)
        {
            int offset = rawCode - EventBase;
            if (offset < MinCode || offset > MaxCode)
            {
                code = default;
                return false;
            }
            code = (SandboxEventCode)offset;
            return true;
        }
        
        public static void AnnouncePresence(bool sandboxEnabled)
        {
            if (!PhotonNetwork.InRoom) return;

            var props = new Hashtable
            {
                { PropHasMenu, true },
                { PropSandboxOn, sandboxEnabled },
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        public static bool PlayerHasSandbox(Player p)
        {
            if (p == null) return false;
            return p.CustomProperties.TryGetValue(PropHasMenu, out object hasMenu) && hasMenu is bool hm && hm
                && p.CustomProperties.TryGetValue(PropSandboxOn, out object hasSandbox) && hasSandbox is bool hs && hs;
        }

        public static void Raise(SandboxEventCode code, object[] data, bool reliable = true)
        {
            if (!PhotonNetwork.InRoom) return;

            var options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            var sendOptions = new SendOptions { Reliability = reliable };
            PhotonNetwork.RaiseEvent(CodeFor(code), data, options, sendOptions);
        }

        public static void RaiseTo(SandboxEventCode code, object[] data, Player target, bool reliable = true)
        {
            if (!PhotonNetwork.InRoom) return;

            var options = new RaiseEventOptions { TargetActors = new[] { target.ActorNumber } };
            var sendOptions = new SendOptions { Reliability = reliable };
            PhotonNetwork.RaiseEvent(CodeFor(code), data, options, sendOptions);
        }
    }
    
    public class NetworkedMonoObject : MonoBehaviour
    {
        public string NetworkId;
        public int OwnerActorNumber;
        
        public string SpawnKey;
        public Vector3 SpawnPoint;
        public Vector3 SpawnNormal;
        public object[] SpawnArgs;
    }

    public static class SandboxObjectRegistry
    {
        private static readonly Dictionary<string, GameObject> _objects = new Dictionary<string, GameObject>();
        private static int _localCounter;

        public static string RegisterLocal(GameObject obj, string spawnKey, Vector3 point, Vector3 normal, object[] args)
        {
            string id = $"{PhotonNetwork.LocalPlayer.ActorNumber}_{_localCounter++}";
            Tag(obj, id, PhotonNetwork.LocalPlayer.ActorNumber, spawnKey, point, normal, args);
            return id;
        }

        public static void RegisterRemote(string id, int ownerActor, string spawnKey, Vector3 point, Vector3 normal, object[] args, GameObject obj)
        {
            Tag(obj, id, ownerActor, spawnKey, point, normal, args);
        }

        private static void Tag(GameObject obj, string id, int ownerActor, string spawnKey, Vector3 point, Vector3 normal, object[] args)
        {
            var tag = obj.GetComponent<NetworkedMonoObject>();
            if (tag == null) tag = obj.AddComponent<NetworkedMonoObject>();

            tag.NetworkId = id;
            tag.OwnerActorNumber = ownerActor;
            tag.SpawnKey = spawnKey;
            tag.SpawnPoint = point;
            tag.SpawnNormal = normal;
            tag.SpawnArgs = args;

            _objects[id] = obj;
        }

        public static GameObject Get(string id) =>
            _objects.TryGetValue(id, out var obj) && obj != null ? obj : null;

        public static void Remove(string id) => _objects.Remove(id);

        public static List<KeyValuePair<string, GameObject>> AllLocallyOwned()
        {
            string prefix = PhotonNetwork.LocalPlayer.ActorNumber + "_";
            return _objects.Where(kv => kv.Key.StartsWith(prefix) && kv.Value != null).ToList();
        }

        public static List<string> IdsOwnedBy(int actorNumber)
        {
            string prefix = actorNumber + "_";
            return _objects.Where(kv => kv.Key.StartsWith(prefix)).Select(kv => kv.Key).ToList();
        }
        
        public static bool TryGetId(Transform hitTransform, out string id)
        {
            var tag = hitTransform != null ? hitTransform.GetComponentInParent<NetworkedMonoObject>() : null;
            id = tag != null ? tag.NetworkId : null;
            return tag != null;
        }

        public static void Clear() => _objects.Clear();
    }
    
    public delegate GameObject SandboxSpawnFactory(Vector3 point, Vector3 normal, object[] args);

    public static class SandboxSpawnRegistry
    {
        private static readonly Dictionary<string, SandboxSpawnFactory> _factories = new Dictionary<string, SandboxSpawnFactory>();

        public static void Register(string key, SandboxSpawnFactory factory) => _factories[key] = factory;

        public static GameObject Spawn(string key, Vector3 point, Vector3 normal, object[] args)
        {
            if (_factories.TryGetValue(key, out var factory)) return factory(point, normal, args);

            Debug.LogWarning($"[MonoSandbox] No networked spawn factory registered for key '{key}' - is that manager's Awake() running before spawns happen?");
            return null;
        }
    }
}
