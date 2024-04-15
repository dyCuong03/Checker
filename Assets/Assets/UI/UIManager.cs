using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : CloneMonoBehaviour
{
    [SerializeField] protected Text turn_Text;
    [SerializeField] protected UIWinGame ui_Win;
    [SerializeField] protected Button button_Restart;
    protected override void Start()
    {
        base.Start();
        button_Restart.onClick.AddListener(RestartGame);
        ui_Win.gameObject.SetActive(false);
    }
    public virtual void SetText(String text){
        turn_Text.text = text;
    }
    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public virtual void CheckWinCondition()
    {
        if (!HasPiecesRemaining(TeamType.Black))
        {
            Debug.Log("White Team Wins!");
            ui_Win.gameObject.SetActive(true);
            ui_Win.SetText("White Wins!");
        }
        else if (!HasPiecesRemaining(TeamType.White))
        {
            Debug.Log("Black Team Wins!");
            ui_Win.gameObject.SetActive(true);
            ui_Win.SetText("Black Wins!");
        }
    }

    protected virtual bool HasPiecesRemaining(TeamType team)
    {
        for (int x = 0; x < BoardManager.instance.TILE_COUNT_X; x++)
        {
            for (int y = 0; y < BoardManager.instance.TILE_COUNT_Y; y++)
            {
                ChessPiece piece = BoardManager.instance.Pieces[x,y];
                if (piece != null && piece.team == team)
                {
                    return true;
                }
            }
        }
        return false;
    }
}