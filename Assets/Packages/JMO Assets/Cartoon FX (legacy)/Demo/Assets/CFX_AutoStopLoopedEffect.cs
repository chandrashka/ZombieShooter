using UnityEngine;

// Cartoon FX  - (c) 2015 Jean Moreno
//
// Script handling looped effect in the Demo Scene, so that they eventually stop

[RequireComponent(typeof(ParticleSystem))]
public class CFX_AutoStopLoopedEffect : MonoBehaviour
{
    public float effectDuration = 2.5f;
    private float d;

    private void Update()
    {
        if (d > 0)
        {
            d -= Time.deltaTime;
            if (d <= 0)
            {
                GetComponent<ParticleSystem>().Stop(true);

                var translation = gameObject.GetComponent<CFX_Demo_Translate>();
                if (translation != null) translation.enabled = false;
            }
        }
    }

    private void OnEnable()
    {
        d = effectDuration;
    }
}