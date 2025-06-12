using System;
using System.Collections;
using System.Collections.Generic;

namespace PlayerSystems.Controls.Weapons.Animations {
    public readonly struct AnimationStates : IEnumerable<AnimationState>, IEquatable<AnimationStates> {
        readonly AnimationState[] states;
        
        AnimationStates(AnimationState[] states) {
            this.states = states;
        }
        
        public AnimationState this[int index] => states[index];
        public AnimationState GetState(int index) => states[index];
        
        public int Count => states.Length;
        
        public bool Contains(AnimationState state) {
            foreach (var s in states) {
                if (s.Equals(state)) {
                    return true;
                }
            }

            return false;
        }   
        
        public int IndexOf(AnimationState state) {
            for (int i = 0; i < states.Length; i++) {
                if (states[i].Equals(state)) {
                    return i;
                }
            }
            return -1;
        }
        
        public IEnumerator<AnimationState> GetEnumerator() {
            return ((IEnumerable<AnimationState>)states).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public static Builder Create(int size) {
            return new Builder(size);
        }
        
        public readonly struct Builder {
            readonly AnimationState[] states;
            
            public Builder(int size) {
                states = new AnimationState[size];
            }
            
            public Builder AddState(AnimationStateConfig stateConfig, int index) {
                states[index] = AnimationState.Create(stateConfig);
                return this;
            }
            
            public Builder AddState(AnimationState state, int index) {
                states[index] = state;
                return this;
            }
            
            public AnimationStates Build() {
                return new AnimationStates(states);
            }
        }

        public bool Equals(AnimationStates other) {
            return Equals(states, other.states);
        }
        public override bool Equals(object obj) {
            return obj is AnimationStates other && Equals(other);
        }
        public override int GetHashCode() {
            return (states != null ? states.GetHashCode() : 0);
        }
        public static bool operator ==(AnimationStates left, AnimationStates right) {
            return left.Equals(right);
        }
        public static bool operator !=(AnimationStates left, AnimationStates right) {
            return !left.Equals(right);
        }
    }
}