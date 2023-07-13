using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private List<Rigidbody> allRigidbodies;

    private void Start()
    {
        foreach (var currentRigidbody in allRigidbodies) currentRigidbody.isKinematic = true;
    }

    public void MakePhysical()
    {
        animator.enabled = false;
        foreach (var currentRigidbody in allRigidbodies) currentRigidbody.isKinematic = false;
    }
}