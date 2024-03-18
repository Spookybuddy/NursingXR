using Microsoft.MixedReality.Toolkit.Utilities;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GIGXR.Platform.Core.User
{
    /// <summary>
    /// A collection of various representations of the user that will appear in a
    /// session.
    /// </summary>
    public static class UserRepresentations
    {
        private static HashSet<UserAvatar> knownAvatars = new HashSet<UserAvatar>();

        private static HashSet<UserAvatarHand> knownHands = new HashSet<UserAvatarHand>();

        public static UserAvatar FindAvatar(string id)
        {
            return knownAvatars.Where(avatar => avatar.UserId == id)
                               .FirstOrDefault();
        }

        public static bool NameTagsEnabled => areNameTagsOn;

        public static bool HeadsEnabled => areAvatarHeadsOn;

        public static bool HandsEnabled => areAvatarHandsOn;

        public static bool LocalHandsEnabled => areLocalHandsOn;

        // By default, we have name tags on to start
        private static bool areNameTagsOn = true;
        private static bool areAvatarHeadsOn = false;
        private static bool areAvatarHandsOn = false;
        private static bool areLocalHandsOn = false;

        /// <summary>
        /// Generates a user card for the input user details.
        /// </summary>
        /// <param name="nickName"></param>
        /// <param name="userId"></param>
        /// <param name="colocated"></param>
        public static UserAvatar GenerateUserAvatarHead(string nickname)
        {
            // TODO Instantiate without referencing PhotonNetwork
            var avatarHeadGO = PhotonNetwork.Instantiate
            (
                "AvatarHead",
                Vector3.zero,
                Quaternion.identity
            );

            avatarHeadGO.name = nickname + " Avatar Head";

            var avatar = avatarHeadGO.GetComponent<UserAvatar>();

            avatar.SetupAvatar(nickname);

            avatar.SetHeadState(areAvatarHeadsOn);
            avatar.SetLabelState(areNameTagsOn);

            // Mobile users do not have hand tracking and should not generate hands, everyone else should
            // Application.isMobilePlatform and SystemInfo.deviceType are not reliable because of UWP/HoloLens
#if !(UNITY_IOS || UNITY_ANDROID)
            GenerateUserAvatarHand(nickname, Handedness.Left);
            GenerateUserAvatarHand(nickname, Handedness.Right);
#endif

            return avatar;
        }

        public static UserAvatarHand GenerateUserAvatarHand(string nickname, Handedness handedness)
        {
            // TODO Instantiate without referencing PhotonNetwork
            var avatarHandGO = PhotonNetwork.Instantiate
            (
                $"Avatar{handedness}Hand",
                Vector3.zero,
                Quaternion.identity
            );

            avatarHandGO.name = $"{nickname}'s {handedness} Hand";

            var avatar = avatarHandGO.GetComponent<UserAvatarHand>();

            avatar.SetupAvatarHand(handedness);

            avatar.SetHandState(areAvatarHandsOn);

            return avatar;
        }

        /// <summary>
        /// Generates a physical representation of the user's location with a 3D model that tracks the user's physical 
        /// head and hands position and orientations.
        /// </summary>
        public static UserAvatar Generate(string nickname)
        {
            return GenerateUserAvatarHead(nickname);
        }

        public static void AddAvatarHead(UserAvatar newAvatar)
        {
            knownAvatars.Add(newAvatar);
        }

        public static void RemoveAvatarHead(UserAvatar newAvatar)
        {
            knownAvatars.Remove(newAvatar);
        }

        public static void AddAvatarHand(UserAvatarHand newHand)
        {
            knownHands.Add(newHand);
        }

        public static void RemoveAvatarHand(UserAvatarHand newHand)
        {
            knownHands.Remove(newHand);
        }

        public static void ToggleAllNametagState()
        {
            areNameTagsOn = !areNameTagsOn;

            foreach (var avatar in knownAvatars)
            {
                avatar.SetLabelState(areNameTagsOn);
            }
        }

        public static void ToggleAllAvatarHeadState()
        {
            areAvatarHeadsOn = !areAvatarHeadsOn;

            foreach (var avatar in knownAvatars)
            {
                avatar.SetHeadState(areAvatarHeadsOn);
            }
        }

        public static void ToggleAllAvatarHandState()
        {
            areAvatarHandsOn = !areAvatarHandsOn;

            foreach (var hand in knownHands)
            {
                hand.SetHandState(areAvatarHandsOn);
            }
        }

        public static void ToggleLocalHandState()
        {
            areLocalHandsOn = !areLocalHandsOn;

            foreach (var hand in knownHands)
            {
                hand.SetLocalHandState(areLocalHandsOn);
            }
        }

        public static void SetAllNametagState(bool state)
        {
            areNameTagsOn = state;

            foreach (var avatar in knownAvatars)
            {
                avatar.SetLabelState(areNameTagsOn);
            }
        }

        public static void SetAllAvatarHeadState(bool state)
        {
            areAvatarHeadsOn = state;

            foreach (var avatar in knownAvatars)
            {
                avatar.SetHeadState(areAvatarHeadsOn);
            }
        }

        public static void SetAllAvatarHandState(bool state)
        {
            areAvatarHandsOn = state;

            foreach (var hand in knownHands)
            {
                hand.SetHandState(areAvatarHandsOn);
            }
        }

        public static void CleanUp()
        {
            knownHands.Clear();

            knownAvatars.Clear();
        }
    }
}