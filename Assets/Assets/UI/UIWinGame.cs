using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIWinGame : CloneMonoBehaviour
{
    [SerializeField] public TextMeshProUGUI win_Text;
    [SerializeField] protected Button button_Restart;
    public virtual void SetText(String text){
        win_Text.text = text;
    }
    protected override void Start()
    {
        base.Start();
        button_Restart.onClick.AddListener(RestartGame);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}