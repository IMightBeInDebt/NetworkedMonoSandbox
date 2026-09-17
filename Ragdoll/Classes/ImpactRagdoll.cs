using GorillaLocomotion;
using MonoSandbox;
using UnityEngine;

#nullable disable
namespace Ragdoll;

// Watches the local player's speed and force-ragdolls when a fast peak is
// immediately followed by a near-complete stop (running into a wall, hard
// landing, etc). Attach it once - Main.Awake() does this for you below.
public class ImpactRagdoll : MonoBehaviour
{
    // --- Tuning ---
    public float speedThreshold = 12f;     // m/s needed to "arm" the trigger
    public float stopThreshold = 0.3f;    // m/s or below counts as "stopped"
    public float peakWindow = 0.4f;       // seconds a fast peak stays valid before it decays
    public float retriggerCooldown = 3f;  // minimum time between auto-triggers

    private Vector3 lastPos;
    private bool hasLastPos;
    private float peakSpeed;
    private float peakTime;
    private float lastTriggerTime = -999f;

    private void Update()
    {
        if ((object)GTPlayer.Instance == null)
            return;

        Vector3 pos = GTPlayer.Instance.transform.position;
        if (!hasLastPos)
        {
            lastPos = pos;
            hasLastPos = true;
            return;
        }

        float instantSpeed = (pos - lastPos).magnitude / Time.deltaTime;
        lastPos = pos;

        // Remember the fastest speed seen recently; let it decay if too much
        // time passes without a stop, so an old fast moment can't trigger later.
        if (instantSpeed > peakSpeed)
        {
            peakSpeed = instantSpeed;
            peakTime = Time.time;
        }
        else if (Time.time - peakTime > peakWindow)
        {
            peakSpeed = 0f;
        }

        bool wasFastEnough = peakSpeed >= speedThreshold;
        bool isNowStopped = instantSpeed <= stopThreshold;
        bool offCooldown = Time.time - lastTriggerTime >= retriggerCooldown;

        if (wasFastEnough && isNowStopped && offCooldown && Plugin.InRoom && Main.impactRagdoll && !Main.isDead)
        {
            lastTriggerTime = Time.time;
            peakSpeed = 0f;

            Main.isDead = true;
            Main.instance.Die();
        }
    }
}
