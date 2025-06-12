using System;
using UnityEngine;

namespace PlayerSystems.Controls.Weapons.Animations {
    [Serializable]
    public class PlayerAnimationConfig {
        [SerializeField] Animator animator;
        [Space]
        [SerializeField] OneShotAnimationConfig equip = OneShotAnimationConfig.Default;
        [SerializeField] PlayerLocomotionConfig locomotion = PlayerLocomotionConfig.Default;
        [Header("Optional")]
        [SerializeField] OneShotAnimationConfig jump = OneShotAnimationConfig.Default;
        [SerializeField] OneShotAnimationConfig land = OneShotAnimationConfig.Default;
        [SerializeField] OneShotAnimationConfig slideStart = OneShotAnimationConfig.Default;

        public Animator Animator => animator;
        
        public OneShotAnimationConfig EquipConfig => equip;
        public OneShotAnimationConfig? JumpConfig => jump.Clip != null ? jump : null;
        public OneShotAnimationConfig? LandConfig => land.Clip != null ? land : null;
        public OneShotAnimationConfig? SlideStartConfig => slideStart.Clip != null ? slideStart : null;
        
        public PlayerLocomotionConfig LocomotionConfig => locomotion;
    }
}