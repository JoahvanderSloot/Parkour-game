using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PlayerSystems.Controls.Weapons.Animations {
    public class PlayerAnimationGraph : IDisposable {
        public string Name { get; private set; }
        public PlayerAnimationConfig AnimationConfig { get; }
        public AnimationStates LocomotionStates { get; private set; }
        
        public PlayableGraph PlayableGraph { get; private set; }
        public AnimationMixerPlayable TopLevelMixer { get; private set; }
        public AnimationMixerPlayable LocomotionMixer { get; private set; }
        
        public static PlayerAnimationGraph Create(PlayerAnimationConfig config) {
            return new PlayerAnimationGraph(config);
        }
        PlayerAnimationGraph (PlayerAnimationConfig config) {
            AnimationConfig = config;
        }
        
        public void Play() {
            if (!PlayableGraph.IsValid())
                return;
            
            PlayableGraph.Play();
        }
        
        public void Stop() {
            if (!PlayableGraph.IsValid())
                return;
            
            PlayableGraph.Stop();
        }
        
        public void CreatePlayableGraph(string name) {
            PlayableGraph = PlayableGraph.Create(name);
            Name = name;
        }
        
        public void CreateTopLevelMixer(int inputCount) {
            TopLevelMixer = AnimationMixerPlayable.Create(PlayableGraph, 2);
        }

        public void CreateLocomotion(AnimationStates states) {
            LocomotionStates = states;
            LocomotionMixer = AnimationMixerPlayable.Create(PlayableGraph, LocomotionStates.Count);

            for (var i = 0; i < LocomotionStates.Count; i++) {
                if (!LocomotionStates[i].Clip) {
                    Debug.LogWarning($"Animation state has no clip assigned.");
                    continue;
                }
                
                var state = LocomotionStates[i];
                var playable = AnimationClipPlayable.Create(PlayableGraph, state.Clip);
                playable.GetAnimationClip().wrapMode = WrapMode.Loop;
                LocomotionMixer.ConnectInput(i, playable, 0);
            }
        }
        
        public void Dispose() {
            if (LocomotionMixer.IsValid()) {
                LocomotionMixer.Destroy();
            }
            
            if (TopLevelMixer.IsValid()) {
                TopLevelMixer.Destroy();
            }
            
            if (PlayableGraph.IsValid()) {
                PlayableGraph.Destroy();
            }
        }
    }
}