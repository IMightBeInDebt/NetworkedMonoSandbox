using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace MonoSandbox.Networking
{
    public static class WeaponNet
    {
        private const byte ShotEventCode = 150;
        private const byte ProjectileEventCode = 151;
        private const byte ColorizeEventCode = 152;

        // --- Hitscan shot impacts (Revolver, Shotgun, Sniper, Rifle, Banana Gun, Laser Gun) ---

        public static void BroadcastShot(int weaponIndex, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (!PhotonNetwork.InRoom) return;

            object[] payload = { weaponIndex, hitPoint.x, hitPoint.y, hitPoint.z, hitNormal.x, hitNormal.y, hitNormal.z };
            var options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            var sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent(ShotEventCode, payload, options, sendOptions);
        }

        public static bool TryDecodeShot(byte rawCode, object customData, out int weaponIndex, out Vector3 hitPoint, out Vector3 hitNormal)
        {
            weaponIndex = 0; hitPoint = default; hitNormal = default;
            if (rawCode != ShotEventCode) return false;
            if (!(customData is object[] data) || data.Length != 7) return false;

            weaponIndex = (int)data[0];
            hitPoint = new Vector3((float)data[1], (float)data[2], (float)data[3]);
            hitNormal = new Vector3((float)data[4], (float)data[5], (float)data[6]);
            return true;
        }

        public static void BroadcastProjectile(string spawnKey, Vector3 point, Vector3 normal, object[] args)
        {
            if (!PhotonNetwork.InRoom) return;
            args ??= new object[0];

            object[] payload = { spawnKey, point.x, point.y, point.z, normal.x, normal.y, normal.z, args };
            var options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            var sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent(ProjectileEventCode, payload, options, sendOptions);
        }

        public static bool TryDecodeProjectile(byte rawCode, object customData, out string spawnKey, out Vector3 point, out Vector3 normal, out object[] args)
        {
            spawnKey = null; point = default; normal = default; args = null;
            if (rawCode != ProjectileEventCode) return false;
            if (!(customData is object[] data) || data.Length != 8) return false;

            spawnKey = (string)data[0];
            point = new Vector3((float)data[1], (float)data[2], (float)data[3]);
            normal = new Vector3((float)data[4], (float)data[5], (float)data[6]);
            args = (object[])data[7];
            return true;
        }

        public static void BroadcastColorize(string networkId, Color color)
        {
            if (!PhotonNetwork.InRoom || string.IsNullOrEmpty(networkId)) return;

            object[] payload = { networkId, color.r, color.g, color.b, color.a };
            var options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            var sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent(ColorizeEventCode, payload, options, sendOptions);
        }

        public static bool TryDecodeColorize(byte rawCode, object customData, out string networkId, out Color color)
        {
            networkId = null; color = default;
            if (rawCode != ColorizeEventCode) return false;
            if (!(customData is object[] data) || data.Length != 5) return false;

            networkId = (string)data[0];
            color = new Color((float)data[1], (float)data[2], (float)data[3], (float)data[4]);
            return true;
        }
    }
}
