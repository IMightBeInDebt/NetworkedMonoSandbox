using GorillaNetworking;
using HarmonyLib;
using UnityEngine;
using Valve.VR;

namespace MonoSandbox.Behaviours
{
    public class InputHandling : MonoBehaviour
    {
        public static float LeftTrigger, RightTrigger, LeftGrip, RightGrip;
        public static SteamVR_Action_Vector2 LeftJoystickAxisSteam, RightJoystickAxisSteam;
        public static Vector2 LeftJoystickAxisQuest, RightJoystickAxisQuest;
        public static bool LeftPrimary, RightPrimary, LeftSecondary, RightSecondary, rightJoystickDown, leftJoystickDown;
        public static bool IsSteam = Traverse.Create(PlayFabAuthenticator.instance).Field("platform").GetValue().ToString().ToLower() == "steam";

        public void Update()
        {
            if (!IsSteam)
            {
                LeftTrigger = ControllerInputPoller.instance.leftControllerIndexFloat;
                LeftGrip = ControllerInputPoller.instance.leftControllerGripFloat;
                RightTrigger = ControllerInputPoller.instance.rightControllerIndexFloat;
                RightGrip = ControllerInputPoller.instance.rightControllerGripFloat;
                LeftPrimary = ControllerInputPoller.instance.leftControllerPrimaryButton;
                LeftSecondary = ControllerInputPoller.instance.leftControllerSecondaryButton;
                RightPrimary = ControllerInputPoller.instance.rightControllerPrimaryButton;
                RightSecondary = ControllerInputPoller.instance.rightControllerSecondaryButton;

                // joystick
                rightJoystickDown = GetRightJoystickDownQuest();
                leftJoystickDown = GetLeftJoystickDownQuest();
                RightJoystickAxisQuest = ControllerInputPoller.instance.rightControllerPrimary2DAxis;
                LeftJoystickAxisQuest = ControllerInputPoller.instance.leftControllerPrimary2DAxis;
            }
            else
            {
                LeftTrigger = SteamVR_Actions.gorillaTag_LeftTriggerFloat.GetAxis(SteamVR_Input_Sources.LeftHand);
                LeftGrip = SteamVR_Actions.gorillaTag_LeftGripFloat.GetAxis(SteamVR_Input_Sources.LeftHand);
                RightTrigger = SteamVR_Actions.gorillaTag_RightTriggerFloat.GetAxis(SteamVR_Input_Sources.RightHand);
                RightGrip = SteamVR_Actions.gorillaTag_RightGripFloat.GetAxis(SteamVR_Input_Sources.RightHand);
                LeftPrimary = SteamVR_Actions.gorillaTag_LeftPrimaryClick.state;
                LeftSecondary = SteamVR_Actions.gorillaTag_LeftSecondaryClick.state;
                RightPrimary = SteamVR_Actions.gorillaTag_RightPrimaryClick.state;
                RightSecondary = SteamVR_Actions.gorillaTag_RightSecondaryClick.state;
                
                // joystick
                rightJoystickDown = GetRightJoystickDownSteam();
                leftJoystickDown = GetLeftJoystickDownSteam();
                RightJoystickAxisSteam = SteamVR_Actions.gorillaTag_RightJoystick2DAxis;
                LeftJoystickAxisSteam = SteamVR_Actions.gorillaTag_LeftJoystick2DAxis;
            }
        }

        public bool GetLeftJoystickDownQuest()
        {
            ControllerInputPoller.instance.leftControllerDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out bool isDown);
                return isDown;
        }

        public bool GetRightJoystickDownQuest()
        {
            ControllerInputPoller.instance.rightControllerDevice.TryGetFeatureValue(
                UnityEngine.XR.CommonUsages.primary2DAxisClick, out bool isDown);
            return isDown;
        }

        public bool GetLeftJoystickDownSteam()
        {
            bool isDown = SteamVR_Actions.gorillaTag_LeftJoystickClick.state;
            return isDown;
        }

        public bool GetRightJoystickDownSteam()
        {
            bool isDown = SteamVR_Actions.gorillaTag_RightJoystickClick.state;
            return isDown;
        }
    }
}
