using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    //board size
    public int _width = 6;
    public int _height = 8;

    //define some spacingfor the board 
    public float _spacingX;
    public float _spacingY;

    //token, node(s) refs
    public GameObject[] _tokenPrefabs;
    private Node[,] _gameBoard;
    public GameObject _tokenBoardGameObj;


    public ArrayLayout _arrLayout; //sourced file (https://drive.google.com/file/d/16sUIA2QjwPntjzXlu-rxu8qQlQ9FO6Uo/view) that allows 2d arrays to be seen in-inspector
    public static GameBoard _instance; //singleton

    //on awake, make the board a singleton
    private void Awake()
    {
        _instance = this;
    }

    //init board on start
    private void Start()
    {
        InitBoard();
    }

    //init board method
    void InitBoard()
    {
        //set size of node using pre defined data
        _gameBoard = new Node[_width, _height];

        //set spacing betwene nodes
        _spacingX = (float)(_width - 1) / 2;
        _spacingY = (float)(_height - 1) / 2;

        //nested loop to set pos of (and) place random token in each node of the game board
        for(int y = 0; y < _height; y++)
        {
            for(int x = 0; x < _width; x++)
            {
                //set pos of each node
                Vector2 _position = new Vector2(x - _spacingX, y - _spacingY);

                //get random value from array (get random token prefab to place)
                int _randIndx = Random.Range(0, _tokenPrefabs.Length);

                //use sourced _arrLayout (in inspector) to check whether a node should be usable or not
                if (_arrLayout.rows[y].row[x]) 
                    _gameBoard[x, y] = new Node(false, null);
                else
                {
                    GameObject _token = Instantiate(_tokenPrefabs[_randIndx], _position, Quaternion.identity); //create game object of token in-engine
                    _token.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
                    _token.GetComponent<Token>().SetIndances(x, y); //set indaces (pos) of token
                    _gameBoard[x, y] = new Node(true, _token); //pos onto board and create a new node (_isMoveable and _tokenGameObj)
                }

            }
        }
    }
}
