using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChessPieceType{
    King,
    Men
}
public enum TeamType{
    White,
    Black
}
public class ChessPiece : CloneMonoBehaviour
{
    public TeamType team;
    public int xPos;
    public int yPos;
    public ChessPieceType chessPieceType;

}
