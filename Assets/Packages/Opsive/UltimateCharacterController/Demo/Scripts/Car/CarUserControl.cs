using UnityEngine;

namespace Opsive.UltimateCharacterController.Demo.UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarUserControl : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use

        private void Awake()
        {
            // get the car controller
            m_Car = GetComponent<CarController>();
        }

        private void FixedUpdate()
        {
            // pass the input to the car!
            var h = Input.GetAxis("Horizontal");
            var v = Input.GetAxis("Vertical");
            var handbrake = Input.GetAxis("Jump");
            m_Car.Move(h, v, v, handbrake);
        }
    }
}