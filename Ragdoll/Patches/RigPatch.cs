using HarmonyLib;

#nullable disable
namespace Patches;

[HarmonyPatch(typeof (VRRig), "OnDisable")]
public class RigPatch
{
  public static bool Prefix(VRRig __instance) => !__instance.isLocal;
}
