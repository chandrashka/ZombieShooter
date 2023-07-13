using UnityEngine;

// Cartoon FX  - (c) 2015 Jean Moreno

// Automatically destroys the GameObject when there are no children left.

public class CFX_AutodestructWhenNoChildren : MonoBehaviour
{
    // Update is called once per frame
    private void Update()
    {
        if (transform.childCount == 0) Destroy(gameObject);
    }
}