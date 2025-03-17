using RpgCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationEvents : MonoBehaviour
{
    private PlayerController playerController;
    private void Start()
    {
        if(playerController == null) playerController = gameObject.GetComponentInParent<PlayerController>();
    }
    public void OnAnimationEventAttack1()
    {
        Debug.Log("Animation event triggered!");
        // Add your custom event logic here
        playerController.PlayerAnimator.Play("CharacterFinishAttack_1");
        //playerController.IsAttack = false;
        
    }

    public void OnAnimationEventSlashVFXAttack1()
    {
        playerController.SlashVFXAttack1.Play();
        playerController.VFXSlashLight.SetActive(true);
        InteractionSystem interactionSystem = gameObject.GetComponentInParent<InteractionSystem>();
        if(interactionSystem != null )
        {
            interactionSystem.Interact();
        }
    }

    public void OnAnimationEventFinishAttack()
    {
        playerController.VFXSlashLight.GetComponent<Light>().intensity = 50.0f;
        playerController.IsAttack = false;
        playerController.VFXSlashLight.SetActive(false);
        playerController.VFXSlashLight.GetComponent<Light>().intensity = 5.0f;
    }
}
