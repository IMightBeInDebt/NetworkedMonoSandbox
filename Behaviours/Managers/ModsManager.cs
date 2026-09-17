#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GorillaLocomotion;
using MonoSandbox.Behaviours;
using MonoSandbox.Behaviours.Managers;
using UnityEngine;

namespace Menu.Mods;

public class ModsManager
{

    // variables
    public static List<VRRig> GetAllRigs(bool i = true)
    {
        return !i
            ? ((IEnumerable<VRRig>)VRRigCache.ActiveRigs).Where<VRRig>((Func<VRRig, bool>)(e => !e.isOfflineVRRig))
            .ToList<VRRig>()
            : ((IEnumerable<VRRig>)VRRigCache.ActiveRigs).ToList<VRRig>();
    }
    public static float FlySpeed
    {
        get => ActualFlySpeed * (GTPlayer.Instance.scale);
        set => ActualFlySpeed = value;
    }
    public static float threshold = 0.35f;
    private static GameObject leftplat, rightplat, lefthandeffect, righthandeffect;
    private static bool clickedJoy = true;
    public static float acceleration = 0.17f;
    private Vector2 xzSteam;
    private float ySteam;
    private Vector2 xzQuest;
    private float yQuest;
    public static float ActualFlySpeed = 8f;
    public static float speedboost = 9.5f;
    public static bool antiMute;
    public static VRRig reportRig;

    // fixes
    public static void FixRig() => VRRig.LocalRig.enabled = true;
    public static void FixRigHandRotation()
    {
        var player = GTPlayer.Instance;
        var rig = GorillaTagger.Instance.offlineVRRig;
        if (!player || !player.playerRigidBody || !rig) return;

        rig.leftHand.rigTarget.transform.rotation *= Quaternion.Euler(rig.leftHand.trackingRotationOffset);
        rig.rightHand.rigTarget.transform.rotation *= Quaternion.Euler(rig.rightHand.trackingRotationOffset);
    }

    // methods
    public static void IronMonke()
    {
        Rigidbody rb = GorillaTagger.Instance.rigidbody;

        if (InputHandling.LeftGrip > 0.1f)
        {
            Vector3 leftForce = Mods.ModsManager.FlySpeed * -GorillaTagger.Instance.leftHandTransform.right;
            rb.AddForce(leftForce * Time.deltaTime, ForceMode.VelocityChange);

            float hapticStrength = GorillaTagger.Instance.tapHapticStrength / 50f * rb.linearVelocity.magnitude;
            GorillaTagger.Instance.StartVibration(true, hapticStrength, GorillaTagger.Instance.tapHapticDuration);
            VRRig.LocalRig.PlayHandTapLocal(115, true, 0.02f);
        }

        if (InputHandling.RightGrip > 0.1f)
        {
            Vector3 rightForce = Mods.ModsManager.FlySpeed * GorillaTagger.Instance.rightHandTransform.right;
            rb.AddForce(rightForce * Time.deltaTime, ForceMode.VelocityChange);

            float hapticStrength = GorillaTagger.Instance.tapHapticStrength / 50f * rb.linearVelocity.magnitude;
            GorillaTagger.Instance.StartVibration(false, hapticStrength, GorillaTagger.Instance.tapHapticDuration);
            VRRig.LocalRig.PlayHandTapLocal(115, false, 0.02f);
        }
    }
    
    public static void FlyQuest()
    {
        Rigidbody attachedRigidbody = GTPlayer.Instance.bodyCollider.attachedRigidbody;
        attachedRigidbody.AddForce(-Physics.gravity * attachedRigidbody.mass * GTPlayer.Instance.scale);
        var xzQuest = InputHandling.LeftJoystickAxisQuest;
        var yQuest = InputHandling.RightJoystickAxisQuest.y;
        Vector3 vector3 = new Vector3(xzQuest.x, yQuest, xzQuest.y);
        Vector3 forward = GTPlayer.Instance.bodyCollider.transform.forward with
        {
            x = 0.0f
        };
        Vector3 right = GTPlayer.Instance.bodyCollider.transform.right with
        {
            y = 0.0f
        };
        Vector3 b = (vector3.x * right + yQuest * Vector3.up + vector3.z * forward) * FlySpeed;
        attachedRigidbody.velocity = Vector3.Lerp(attachedRigidbody.velocity, b, acceleration);
    }

    public static void Platforms()
    {
        if (InputHandling.LeftGrip > 0.1 && leftplat == null)
        {
            leftplat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftplat.transform.localScale = new Vector3(0.025f, 0.2f, 0.3f);
            leftplat.transform.position = GorillaTagger.Instance.leftHandTransform.position;
            leftplat.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
            leftplat.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
            leftplat.GetComponent<Renderer>().material.color = ThemeManager.ItemButtonColor1;
        }

        if (InputHandling.RightGrip > 0.1 && rightplat == null)
        {
            rightplat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightplat.transform.localScale = new Vector3(0.025f, 0.2f, 0.3f);
            rightplat.transform.position = GorillaTagger.Instance.rightHandTransform.position;
            rightplat.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
            rightplat.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
            rightplat.GetComponent<Renderer>().material.color = ThemeManager.ItemButtonColor2;
        }

        if (InputHandling.RightGrip < 0.1 && rightplat != null)
        {
            GameObject.Destroy(rightplat);
            rightplat = null;
        }

        if (InputHandling.LeftGrip < 0.1 && leftplat != null)
        {
            GameObject.Destroy(leftplat);
            leftplat = null;
        }
    }

    public static void Speedboost()
    {
        GorillaLocomotion.GTPlayer.Instance.maxJumpSpeed = speedboost;
    }
    
    private static bool OverlappingButton(VRRig vrrig, Vector3 position) =>
        new[] {
            vrrig.rightHandTransform.position,
            vrrig.leftHandTransform.position,
            vrrig.rightHand.syncPos,
            vrrig.leftHand.syncPos
        }.Any(handPos => Vector3.Distance(handPos, position) < threshold);

    public static void AntiReport(Action<VRRig, Vector3> onReport)
    {
        if (!NetworkSystem.Instance.InRoom) return;

        if (reportRig != null)
        {
            if (reportRig.isLocal)
                return;

            onReport?.Invoke(reportRig, reportRig.transform.position);
            reportRig = null;
            return;
        }

        foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
        {
            if (line.linePlayer != NetworkSystem.Instance.LocalPlayer) continue;
            Transform report = line.reportButton.gameObject.transform;

            foreach (var vrrig in from vrrig in VRRigCache.ActiveRigs where !vrrig.isLocal where OverlappingButton(vrrig, report.position) || (antiMute && OverlappingButton(vrrig, line.muteButton.gameObject.transform.position))select vrrig) onReport?.Invoke(vrrig, report.transform.position);
        }
    }

    public static float antiReportDelay;
    public static void AntiReportDisconnect()
    {
        AntiReport((vrrig, position) =>
        {
            NetworkSystem.Instance.ReturnToSinglePlayer();

            if (!(Time.time > antiReportDelay)) return;
            antiReportDelay = Time.time + 1f;
        });
    }
}