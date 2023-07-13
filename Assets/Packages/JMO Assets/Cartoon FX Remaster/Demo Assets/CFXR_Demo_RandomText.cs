using UnityEngine;

namespace CartoonFX
{
    public class CFXR_Demo_RandomText : MonoBehaviour
    {
        public ParticleSystem particles;
        public CFXR_ParticleText dynamicParticleText;

        private void OnEnable()
        {
            InvokeRepeating("SetRandomText", 0f, 1.5f);
        }

        private void OnDisable()
        {
            CancelInvoke("SetRandomText");
            particles.Clear(true);
        }

        private void SetRandomText()
        {
            // set text and properties according to the random damage:
            // - bigger damage = big text, red to yellow gradient
            // - lower damage = smaller text, fully red
            var damage = Random.Range(10, 1000);
            var text = damage.ToString();
            var intensity = damage / 1000f;
            var size = Mathf.Lerp(0.8f, 1.3f, intensity);
            var color1 = Color.Lerp(Color.red, Color.yellow, intensity);
            dynamicParticleText.UpdateText(text, size, color1);

            particles.Play(true);
        }
    }
}