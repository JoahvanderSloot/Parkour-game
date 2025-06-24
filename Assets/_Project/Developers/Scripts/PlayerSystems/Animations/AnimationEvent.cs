using System;
using UnityEngine;
using UnityEngine.Events;

namespace PlayerSystems.Controls.Weapons.Animations {
    [Serializable]
    public class AnimationEvent {
        [SerializeField] UnityEvent animationEvent;
        [SerializeField, Range(0,1)] float time;
        [SerializeField, Range(0,1)] float requiredBlendWeight;
        bool invoked;
        
        public float Time => time;
        public float RequiredBlendWeight => requiredBlendWeight;
        public bool Invoked => invoked;
        
        public void Invoke() {
            if (invoked) 
                return;
            
            invoked = true;
            animationEvent.Invoke();
        }
        
        public void Reset() {
            invoked = false;
        }
        
        public void Update(float normalizedTime, float blendWeight) {
            if (normalizedTime < time)
                Reset();
            else if (!invoked && normalizedTime >= time)
                Invoke();
        }
        public void Update(double normalizedTime, float blendWeight) {
            Update((float)normalizedTime, blendWeight);
        }
    }
}