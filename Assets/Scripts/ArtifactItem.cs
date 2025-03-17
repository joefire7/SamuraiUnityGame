using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ArtifactItem : MonoBehaviour, IInteractable
{
    [SerializeField]
    private int comicPage;

    [SerializeField]
    private CinemachineImpulseSource impulseSource;

    private Coroutine coroutine;
    public void Execute()
    {
        Debug.Log("HIT Artifact Item");
       
        if(coroutine != null) { coroutine =  null; }
        coroutine = StartCoroutine(WaitForShake(1.0f));
    }

    // Start is called before the first frame update
    void Start()
    {
        if(impulseSource == null)
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator WaitForShake(float delay)
    {
        impulseSource.GenerateImpulse();
        yield return new WaitForSeconds(delay);
        GameManager.Instance.OpenComicViewEvent(comicPage);
    }
}
