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
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
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
        BoardCheck();
    }

    public bool BoardCheck()
    {
        Debug.Log("Checking Board");
        bool _hasMatched = false;

        List<Token> _tokensToRemove = new List<Token>();

        //loop through board
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                //is node (on board) usable?
                if (_gameBoard[x, y]._isUsable)
                {
                    //get token game object from board
                    Token _token = _gameBoard[x, y]._tokenObj.GetComponent<Token>();

                    //if token is NOT usable
                    if (!_token._isMatched)
                    {
                        //check to see if token is connected to other tokens
                        MatchResult _matchedTokens = IsConnected(_token);

                        //if 3 or more are connected
                        if(_matchedTokens._connectedTokens.Count >= 3)
                        {
                            _tokensToRemove.AddRange(_matchedTokens._connectedTokens);

                            foreach(Token _t in _matchedTokens._connectedTokens)
                            {
                                _t._isMatched = true;
                            }

                            _hasMatched = true;
                        }
                    }
                }
            }
        }
        return _hasMatched;
    }

    void CheckTokenDirection(Token _pToken, Vector2Int _pDirection, List<Token> _connectedTokens)
    {
        TokenType _tokenType = _pToken._tokenType; //get token type

        //check neighbouring tokens (newly calculated position of neighboring tiles)
        int _xDir = _pToken._indX + _pDirection.x;
        int _yDir = _pToken._indY + _pDirection.y;

        //check board boundaries
        while (_xDir >= 0 && _xDir < _width && _yDir >= 0 && _yDir < _height)
        {
            //if node is usable
            if (_gameBoard[_xDir, _yDir]._isUsable)
            {
                //get a reference to neibouring tile using (newly calculated position of neighboring tiles)
                Token _neighbouringToken = _gameBoard[_xDir, _yDir]._tokenObj.GetComponent<Token>();

                //check for match between token and neighbouring token as well as an already existing match (prevent dupes)
                if (!_neighbouringToken._isMatched && _neighbouringToken._tokenType == _tokenType)
                {
                    //add neighbouring token as a connected token
                    _connectedTokens.Add(_neighbouringToken);

                    //then move to next token's position (to check if there are a string of matching tokens, not only two)
                    _xDir += _pDirection.x;
                    _yDir += _pDirection.y;
                }
                else
                    break; //no match
            }
            else
                break; //not a usable node
        }
    }

    MatchResult IsConnected(Token _pToken)
    {
        //get a list of connected tokens and their types
        List<Token> _connectedTokens = new List<Token>();
        TokenType _tokenType = _pToken._tokenType;

        //add tokens to list
        _connectedTokens.Add(_pToken);

        //check right neighbors
        CheckTokenDirection(_pToken, new Vector2Int(1, 0), _connectedTokens);

        //check left neighbors
        CheckTokenDirection(_pToken, new Vector2Int(-1, 0), _connectedTokens);

        //check for a matching three
        if (_connectedTokens.Count == 3)
        {
            Debug.Log("You have paired 3 " + _connectedTokens[0]._tokenType + " horizontally.");

            return new MatchResult
            {
                _connectedTokens = _connectedTokens,
                _direction = MatchDirection.Horizontal
            };
        }
        else if (_connectedTokens.Count > 3)
        {
            Debug.Log("You have paired more than 3 " + _connectedTokens[0]._tokenType + " horizontally.");
            return new MatchResult
            {
                _connectedTokens = _connectedTokens,
                _direction = MatchDirection.LongHorizontal
            };
        }

        _connectedTokens.Clear();
        _connectedTokens.Add(_pToken);

         //check above neighbors
         CheckTokenDirection(_pToken, new Vector2Int(0, 1), _connectedTokens);

        //check below neighbors
        CheckTokenDirection(_pToken, new Vector2Int(0, -1), _connectedTokens);

        //check for three matching vertically
        if (_connectedTokens.Count == 3)
        {
            Debug.Log("You have paired 3 " + _connectedTokens[0]._tokenType + " vertically.");
            return new MatchResult
            {
                _connectedTokens = _connectedTokens,
                _direction = MatchDirection.Vertical
            };
        }
        else if (_connectedTokens.Count > 3)
        {
            Debug.Log("You have paired more than 3 " + _connectedTokens[0]._tokenType + " vertically.");

            return new MatchResult
            {
                _connectedTokens = _connectedTokens,
                _direction = MatchDirection.LongVertical
            };
        }
        else
        {
            return new MatchResult
            {
                _connectedTokens = _connectedTokens,
                _direction = MatchDirection.None
            };
        }
    }
}



            



  


public class MatchResult
{
    public List<Token> _connectedTokens;
    public MatchDirection _direction;
}

public enum MatchDirection
{
    Vertical, 
    Horizontal,

    //match > 3
    LongVertical,
    LongHorizontal,

    Super, //multiple directions 
    None

}
