using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComicView : MonoBehaviour
{
    public Image comicImage;
    public Sprite[] comicsImagesSprites;

    [SerializeField]
    private Canvas comicViewCanvas;

    [SerializeField]
    private Button exitButton;
    // Start is called before the first frame update
    void Start()
    {
        if(comicViewCanvas == null)
        {
            comicViewCanvas = GetComponent<Canvas>();
        }

        exitButton.onClick.AddListener(() =>
        {
            CloseComicView();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateComicViewPage(int page)
    {
        comicImage.sprite = comicsImagesSprites[page];
        comicViewCanvas.enabled = true;
    }

    public void CloseComicView()
    {
        comicViewCanvas.enabled = false;
        GameManager.Instance.SetState(GameState.Playing);
    }
}
