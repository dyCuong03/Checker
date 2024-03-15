using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum Chess{
    King,
    Men
}
public class BoardManager : CloneMonoBehaviour
{
    [SerializeField] protected MaterialManager materialManager;
    protected Material _material;
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    [SerializeField] protected GameObject tilePref;
    protected GameObject[,] tiles;
    [SerializeField] protected GameObject chessManPref;
    //protected Dictionary<Chess, List<GameObject>> chessMans = new Dictionary<Chess, List<GameObject>>();
    protected List<GameObject> chessMans = new List<GameObject>();
    protected override void Awake()
    {
        base.Awake();
        this.GenerateAllBroad(TILE_COUNT_X,TILE_COUNT_Y);
    }
    protected virtual void GenerateAllBroad(int tileCountX, int tileCountY){
        tiles = new GameObject[TILE_COUNT_X,TILE_COUNT_Y];
        Material white = materialManager.White_Material;
        Material black = materialManager.Black_Material;
        for(int i = 0 ; i < tileCountX; i++){
            for(int j = 0; j < tileCountY; j++){
                if ((i + j) % 2 == 0){
                    this._material = white;
                } else {
                    this._material = black;
                    if(i == 0 || i == 1 || i == 2){
                        this.GenerateChessMan(black,i, j);
                    }
                    if(i == 5 || i == 6 || i == 7){
                        this.GenerateChessMan(white,i, j);
                    }
                }
                tiles[i,j] = this.GenerateTile(i,j);
            }
        }
    }
    protected virtual GameObject GenerateTile(int xPos, int zPos)
    {
        Vector3 spawPos = new Vector3(xPos, 0, zPos);
        GameObject _tile = Instantiate(tilePref, spawPos, Quaternion.identity);
        _tile.transform.parent = this.transform;
        MeshRenderer renderer = _tile.GetComponent<MeshRenderer>();
        renderer.material = this._material;
        return _tile;
    }
    protected virtual void GenerateChessMan(Material _material, int xPos, int zPos)
    {
        Vector3 spawnPos = new Vector3(xPos, 1, zPos);
        GameObject _chess = Instantiate(chessManPref, spawnPos, Quaternion.identity);
        MeshRenderer renderer = _chess.GetComponent<MeshRenderer>();
        renderer.material = _material;
        chessMans.Add(_chess);
        _chess.transform.parent = this.transform;
    }
    
}
