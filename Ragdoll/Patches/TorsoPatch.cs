using HarmonyLib;
using UnityEngine;
using Plugin = Ragdoll.Main;

#nullable disable
namespace Patches;

[HarmonyPatch(typeof (VRRig), "PostTick")]
public class TorsoPatch
{
  public static bool enabled = true;
  public static int mode;

  public static void Postfix(VRRig __instance)
  {
    if (!TorsoPatch.enabled || !__instance.isLocal || !Plugin.isDead || (Object) Plugin.Ragdoll == (Object) null)
      return;
    Plugin.SyncRigToRagdoll(__instance);
    if (!Plugin.fbtEnabled)
      return;
    TorsoPatch.ApplyBodyTracking(__instance);
  }

  private static void ApplyBodyTracking(VRRig rig)
  {
    Vector3 forward = rig.head.rigTarget.transform.forward with
    {
      y = 0.0f
    };
    if ((double) forward.sqrMagnitude <= 1.0 / 1000.0)
      return;
    forward.Normalize();
    Quaternion quaternion1 = Quaternion.LookRotation(forward, Vector3.up);
    Transform transform = rig.transform;
    Quaternion rotation = rig.transform.rotation;
    double x = (double) rotation.eulerAngles.x;
    double y = (double) quaternion1.eulerAngles.y;
    rotation = rig.transform.rotation;
    double z = (double) rotation.eulerAngles.z;
    Quaternion quaternion2 = Quaternion.Euler((float) x, (float) y, (float) z);
    transform.rotation = quaternion2;
  }
}
