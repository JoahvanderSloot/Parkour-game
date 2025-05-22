using TMPro;
using UnityEngine;

namespace PlayerSystems.Movement.Debugging
{
    public class StateUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI stanceTMP;
        [SerializeField] private TextMeshProUGUI groundedTMP;
        [SerializeField] private TextMeshProUGUI velocityTMP;

        public void UpdateStateUI(CharacterState state)
        {
            switch (state.Stance)
            {
                case Stance.Stand:
                    stanceTMP.color = Color.cyan;
                    stanceTMP.text = "Stance: Stand";
                    break;
                case Stance.Crouch:
                    stanceTMP.color = Color.yellow;
                    stanceTMP.text = "Stance: Crouch";
                    break;
                case Stance.Slide:
                    stanceTMP.color = Color.magenta;
                    stanceTMP.text = "Stance: Slide";
                    break;
            }

            if (state.Grounded)
            {
                groundedTMP.color = Color.green;
                groundedTMP.text = "Grounded: True";
            }
            else
            {
                groundedTMP.color = Color.red;
                groundedTMP.text = "Grounded: False";
            }

            velocityTMP.color = Color.blue;
            velocityTMP.text = "Velocity: " + state.Velocity.magnitude.ToString("F2");
        }
    }
}
