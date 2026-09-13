using System;
using GorillaLocomotion;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using Object = UnityEngine.Object;
using Plugin = Ragdoll.Main;

#nullable disable
namespace Patches;

[HarmonyPatch(typeof (PhotonNetwork), "RunViewUpdate")]
public class PreSerialize
{
  /*
  public static void Prefix()
  {
    if (!Plugin.isDead || !((Object) Plugin.Ragdoll != (Object) null))
      return;
    Plugin.SyncRigToRagdoll(VRRig.LocalRig);
    if (!Plugin.freeMoveEnabled)
    {
      Transform transform = Plugin.Ragdoll.transform.Find("Stand/Gorilla Rig/body");
      if ((Object) transform != (Object) null)
      {
        GorillaTagger.Instance.transform.position = Plugin.World2Player(transform.position + Plugin.startForward * 2f + new Vector3(0.0f, 2f, 0.0f));
        GorillaTagger.Instance.leftHandTransform.position = GorillaTagger.Instance.bodyCollider.transform.position;
        GorillaTagger.Instance.rightHandTransform.position = GorillaTagger.Instance.bodyCollider.transform.position;
      }
    }
    
    if (!Plugin.isDead) return;
    Debug.Log($"[Ragdoll] pre-serialize pos: tagger={GorillaTagger.Instance.transform.position}, VRRig={VRRig.LocalRig.transform.position}");
  }
*/
  
  public static void Prefix()
  {
    if (!Plugin.isDead || !((Object) Plugin.Ragdoll != (Object) null))
      return;
    Console.WriteLine($"[Ragdoll] PRE tagger={GorillaTagger.Instance.transform.position} GTPlayer={GTPlayer.Instance.transform.position} VRRig={VRRig.LocalRig.transform.position}");
    Plugin.SyncRigToRagdoll(VRRig.LocalRig);
  }

  public static void Postfix()
  {
    if (!Plugin.isDead || !((Object) Plugin.Ragdoll != (Object) null))
      return;
    Plugin.instance.UpdateRigPos();
    Console.WriteLine($"[Ragdoll] POST tagger={GorillaTagger.Instance.transform.position} GTPlayer={GTPlayer.Instance.transform.position}");
  }
}
