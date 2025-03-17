using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private ComicView ComicView;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.OpenComicViewEvent += OpenComicView;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenComicView(int comicPage)
    {
        ComicView.UpdateComicViewPage(comicPage);
        GameManager.Instance.SetState(GameState.Menu);
    }
    public void CloseComicView()
    {
        ComicView.CloseComicView();
        GameManager.Instance.SetState(GameState.Playing);
    }
}
