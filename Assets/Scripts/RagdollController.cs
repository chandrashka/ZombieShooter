using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private List<Rigidbody> allRigidbodies;

    private void Start()
    {
        foreach (var currentRigidbody in allRigidbodies)
        {
            currentRigidbody.isKinematic = true;
        }
    }

    private void Update()
    {
        
    }

    private void MakePhysical()
    {
        animator.enabled = false;
        foreach (var currentRigidbody in allRigidbodies)
        {
            currentRigidbody.isKinematic = false;
        }
    }
}
