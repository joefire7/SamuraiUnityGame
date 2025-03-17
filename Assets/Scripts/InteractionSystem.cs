using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public float Radius = 1.0f;
    public LayerMask InteractableLayer;
   

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, Radius, InteractableLayer);
        
        foreach(var hitCollider in hitColliders)
        {
           IInteractable interactable = hitCollider.GetComponent<IInteractable>();
           if(interactable != null)
            {
                interactable.Execute();
            }
        }

    }
}
