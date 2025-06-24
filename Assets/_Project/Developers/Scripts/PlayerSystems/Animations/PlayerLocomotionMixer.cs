using System;
using PlayerSystems.Modules.MovementModules;
using PlayerSystems.Movement;
using PrimeTween;
using UnityEngine;
using UnityEngine.Playables;

namespace PlayerSystems.Controls.Weapons.Animations {
    public class PlayerLocomotionMixer : IDisposable {
        readonly PlayerAnimationGraph animationGraph;
        readonly PlayerController player;
        
        AnimationState currentLocomotionState;
        Tween blendTween;
        
        bool enabled;
        
        AnimationStates LocomotionStates => animationGraph.LocomotionStates;
        AnimationState IdleState => LocomotionStates[0];
        AnimationState RunState => LocomotionStates[1];
        AnimationState SlideState => LocomotionStates[2];
        AnimationState AirborneState => LocomotionStates[3];
        AnimationState WallClimbState => LocomotionStates[4];
        
        public static PlayerLocomotionMixer Create(PlayerAnimationGraph animationGraph, PlayerController playerController) {
            var locomotionMixer = new PlayerLocomotionMixer(animationGraph, playerController);
            locomotionMixer.CreateLocomotion(animationGraph.AnimationConfig.LocomotionConfig);
            return locomotionMixer;
        }

        PlayerLocomotionMixer(PlayerAnimationGraph graph, PlayerController playerController) {
            animationGraph = graph;
            player = playerController;
        }
        
        internal void CreateLocomotion(PlayerLocomotionConfig locomotionConfig) {
            var locomotionStates = AnimationStates.Create(5)
                .AddState(locomotionConfig.IdleStateConfig, 0)
                .AddState(locomotionConfig.RunStateConfig, 1)
                .AddState(locomotionConfig.SlideStateConfig, 2)
                .AddState(locomotionConfig.AirborneStateConfig, 3)
                .AddState(locomotionConfig.WallClimbStateConfig, 4)
                .Build();
            
            animationGraph.CreateLocomotion(locomotionStates);
            
            animationGraph.LocomotionMixer.SetInputWeight(0, 1);
            animationGraph.LocomotionMixer.SetInputWeight(1, 0);
            animationGraph.LocomotionMixer.SetInputWeight(2, 0);
            animationGraph.LocomotionMixer.SetInputWeight(3, 0);
            
            animationGraph.TopLevelMixer.ConnectInput(0, animationGraph.LocomotionMixer, 0);
            
            currentLocomotionState = locomotionStates[0];
        }
        
        public void Enable() => enabled = true;
        public void Disable() => enabled = false;

        // ReSharper disable once CognitiveComplexity
        public void FixedUpdate() {
            if (!enabled)
                return;
            
            var state = player.Movement.GetState();
            
            var speed = state.Velocity.magnitude;
            var stance = state.Stance;
            var grounded = state.Grounded;
            
            var normalizedSpeed = speed / player.Movement.BaseSpeed;

            if (grounded) {
                if (normalizedSpeed > 0.1f) {
                    if (stance is Stance.Slide && currentLocomotionState != SlideState)
                        BlendIn(SlideState);
                    
                    if (stance is not Stance.Slide && currentLocomotionState != RunState)
                        BlendIn(RunState);
                }
                else {
                    if (currentLocomotionState != IdleState)
                        BlendIn(IdleState);
                }
            }
            else if (player.Modules.ActiveModule is WallClimbModule) {
                BlendIn(WallClimbState);
            }
            else {
                if (currentLocomotionState != AirborneState)
                    BlendIn(AirborneState);
            }

            float animationSpeed = currentLocomotionState.SpeedScaling != 0f
                ? normalizedSpeed * currentLocomotionState.SpeedScaling * currentLocomotionState.AnimationSpeed
                : currentLocomotionState.AnimationSpeed;
            
            animationGraph.LocomotionMixer.SetSpeed(animationSpeed);

            for (var index = 0; index < LocomotionStates.Count; index++) {
                var locomotionState = LocomotionStates[index];
                
                if (locomotionState.Events.Count == 0)
                    continue;

                var stateIndex = LocomotionStates.IndexOf(locomotionState);
                var inputWeight = animationGraph.LocomotionMixer.GetInputWeight(stateIndex);
                if (inputWeight == 0f)
                    continue;

                foreach (var animationEvent in locomotionState.Events) {
                    if (inputWeight < animationEvent.RequiredBlendWeight)
                        continue;
                    
                    var normalizedTime = animationGraph.LocomotionMixer.GetInput(stateIndex).GetTime() /
                                         locomotionState.Clip.length % 1f;
                    
                    animationEvent.Update(normalizedTime, inputWeight);
                }
            }
        }

        void BlendIn(AnimationState locomotionState) {
            if (!locomotionState.Clip) {
                Debug.LogWarning($"Locomotion state has no animation clip assigned.");
                return;
            }
            
            if (blendTween.isAlive)
                blendTween.Stop();
            
            currentLocomotionState = locomotionState;
            
            var idleWeight = animationGraph.LocomotionMixer.GetInputWeight(0);
            var runWeight = animationGraph.LocomotionMixer.GetInputWeight(1);
            var slideWeight = animationGraph.LocomotionMixer.GetInputWeight(2);
            var airborneWeight = animationGraph.LocomotionMixer.GetInputWeight(3);
            
            var locomotionStateIndex = LocomotionStates.IndexOf(locomotionState);
            var newStateStartWeight = animationGraph.LocomotionMixer.GetInputWeight(locomotionStateIndex);
             
            blendTween = Tween.Custom(0f, 1f, locomotionState.BlendInSeconds, blendTime => {
                float blendInWeight = locomotionState.BlendInCurve.Evaluate(blendTime);
                animationGraph.LocomotionMixer.SetInputWeight(0, idleWeight * (1f - blendInWeight));
                animationGraph.LocomotionMixer.SetInputWeight(1, runWeight * (1f - blendInWeight));
                animationGraph.LocomotionMixer.SetInputWeight(2, slideWeight * (1f - blendInWeight));
                animationGraph.LocomotionMixer.SetInputWeight(3, airborneWeight * (1f - blendInWeight));
                
                animationGraph.LocomotionMixer.SetInputWeight(locomotionStateIndex, Mathf.Lerp(newStateStartWeight, 1f, blendInWeight));
            }, Ease.Linear);
        }

        public void Dispose() {
            Disable();
            if (blendTween.isAlive) {
                blendTween.Stop();
            }
        }
    }
}