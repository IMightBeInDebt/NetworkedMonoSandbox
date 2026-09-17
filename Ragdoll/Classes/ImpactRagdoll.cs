using GorillaLocomotion;
using MonoSandbox;
using UnityEngine;

#nullable disable
namespace Ragdoll;

public class ImpactRagdoll : MonoBehaviour
{
    // --- Tuning ---
    public float speedThreshold = 12f;  
    public float stopThreshold = 0.3f;  
    public float peakWindow = 0.4f;       
    public float retriggerCooldown = 3f;  

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
