using System;
using System.Threading;
using PrimeTween;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PlayerSystems.Controls.Weapons.Animations {
    public class PlayerAnimationSystem : IDisposable {
        public readonly PlayerAnimationGraph animationGraph;
        readonly PlayerLocomotionMixer locomotionMixer;

        public AnimationClipPlayable oneShotPlayable;
        Sequence oneShotBlendSequence;

        public static PlayerAnimationSystem Create(PlayerAnimationConfig config, PlayerController playerController, string name) {
            name = $"{name}_{Guid.NewGuid().ToString()}";
            var animationGraph = CreateGraph(config, playerController, name, out var locomotionMixer);
            var animationSystem = new PlayerAnimationSystem(animationGraph, locomotionMixer);
            
            return animationSystem;
        }
        
        PlayerAnimationSystem(PlayerAnimationGraph graph, PlayerLocomotionMixer locomotion) {
            animationGraph = graph;
            locomotionMixer = locomotion;
        }

        static PlayerAnimationGraph CreateGraph(PlayerAnimationConfig config, PlayerController playerController, string name, out PlayerLocomotionMixer locomotion) {
            var animationGraph = PlayerAnimationGraph.Create(config);
            animationGraph.CreatePlayableGraph(name);
            
            AnimationPlayableOutput playableOutput = AnimationPlayableOutput.Create(animationGraph.PlayableGraph, $"{name}AnimationOutput", animationGraph.AnimationConfig.Animator);
            
            animationGraph.CreateTopLevelMixer(2);
            playableOutput.SetSourcePlayable(animationGraph.TopLevelMixer);
            
            locomotion = PlayerLocomotionMixer.Create(animationGraph, playerController);
            
            animationGraph.PlayableGraph.GetRootPlayable(0).SetInputWeight(0, 1f);
            
            return animationGraph;
        }
        
        public void RecreateLocomotion(PlayerLocomotionConfig locomotionConfig) {
            animationGraph.TopLevelMixer.DisconnectInput(0);
            locomotionMixer.CreateLocomotion(locomotionConfig);
            Debug.Log("Recreated Locomotion");
        }
        
        public void Enable() {
            locomotionMixer.Enable();
            animationGraph.Play();
        }
        
        public void Disable() {
            InterruptOneShot();
            locomotionMixer.Disable();
            animationGraph.Stop();
        }

        CancellationTokenSource oneShotTokenSource = new CancellationTokenSource();
        public void PlayOneShot(OneShotAnimationConfig config) {
            if (config.Clip == null) {
                Debug.LogError($"PlayOneShot: Clip is null on graph: {animationGraph.Name}");
                return;
            }
            
            if (!config.AllowReplay && oneShotPlayable.IsValid() && oneShotPlayable.GetAnimationClip() == config.Clip)
                return;
            
            if (!config.AllowInterrupt && oneShotPlayable.IsValid())
                return;
            
            InterruptOneShot();
            
            oneShotPlayable = AnimationClipPlayable.Create(animationGraph.PlayableGraph, config.Clip);
            animationGraph.TopLevelMixer.ConnectInput(1, oneShotPlayable, 0);
            oneShotPlayable.SetSpeed(config.AnimationSpeed);
            
            OneShotEventCallBackTask(config, oneShotTokenSource.Token);
            
            float length = (config.Clip.length + config.AddedSeconds) / config.AnimationSpeed;
            float blendInDuration = length * config.Blend.x;
            float blendOutDuration = length * (1f - config.Blend.y);
            float delayDuration = length - blendInDuration - blendOutDuration;

            oneShotBlendSequence = Tween.Custom(0f, 1f, blendInDuration, blendTime => {
                float weight = config.BlendInCurve.Evaluate(blendTime);
                animationGraph.TopLevelMixer.SetInputWeight(0, 1f - weight);
                animationGraph.TopLevelMixer.SetInputWeight(1, weight);
            }, Ease.Linear)
            .Chain(Tween.Delay(delayDuration + config.AddedSeconds))
            .Chain(
                Tween.Custom(0f, 1f, blendOutDuration, blendTime => {
                    float weight = config.BlendOutCurve.Evaluate(blendTime);
                    animationGraph.TopLevelMixer.SetInputWeight(0, weight);
                    animationGraph.TopLevelMixer.SetInputWeight(1, 1f - weight);
                }, Ease.Linear)
            ).OnComplete( this, t => t.DisconnectOneShot());
        }

        async Awaitable OneShotEventCallBackTask(OneShotAnimationConfig config, CancellationToken token) {
            while (oneShotPlayable.IsValid() && oneShotPlayable.GetAnimationClip() == config.Clip && !token.IsCancellationRequested) {
                var normalizedTime = oneShotPlayable.GetTime() / oneShotPlayable.GetAnimationClip().length;
            
                foreach (var animationEvent in config.AnimationEvents) {
                    animationEvent.Update((float)normalizedTime, animationGraph.TopLevelMixer.GetInputWeight(1));
                }

                await Awaitable.NextFrameAsync(token);
            }
        
            foreach (var animationEvent in config.AnimationEvents) {
                animationEvent.Reset();
            }
        }
        
        public void InterruptOneShot() {
            oneShotBlendSequence.Stop();
            
            animationGraph.TopLevelMixer.SetInputWeight(0, 1f);
            animationGraph.TopLevelMixer.SetInputWeight(1, 0f);
            
            if (oneShotPlayable.IsValid()) {
                DisconnectOneShot();
            }
        }

        void DisconnectOneShot() {
            oneShotTokenSource.Cancel();
            oneShotTokenSource = new CancellationTokenSource();
            
            animationGraph.TopLevelMixer.DisconnectInput(1);
            animationGraph.PlayableGraph.DestroyPlayable(oneShotPlayable);
        }
        
        public AnimationClip GetCurrentOneShotClip() {
            if (oneShotPlayable.IsValid())
                return oneShotPlayable.GetAnimationClip();
            
            return null;
        }


        public void Dispose() {
            InterruptOneShot();
            
            animationGraph.Dispose();
            locomotionMixer.Dispose();
        }
        
        public void FixedUpdate() {
            locomotionMixer.FixedUpdate();
        }
    }
}