using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    //board size
    public int _width = 6;
    public int _height = 8;

    //define some spacing between the nodes
    public float _spacingX;
    public float _spacingY;

    //token, node(s) refs
    public GameObject[] _tokenPrefabs; //list prefabs of the tokens
    private Node[,] _gameBoard; //2d array of game board positions (nodes)
    public GameObject _tokenBoardGameObj; //object of the bored itself.


    public ArrayLayout _arrLayout; //sourced file (https://drive.google.com/file/d/16sUIA2QjwPntjzXlu-rxu8qQlQ9FO6Uo/view) that allows 2d arrays to be seen in-inspector
    public static GameBoard _instance; //singleton ref

    public List<GameObject> _tokensToDestroy = new();

    [SerializeField]
    private Token _selectedToken;

    [SerializeField]
    private bool _isMoving = false;


    //on awake, make the board a singleton
    private void Awake()
    {
        //set singleton
        _instance = this;
    }

    //init board on start
    private void Start()
    {
        //initialise board
        InitBoard();
    }

    private void Update()
    {
        //when we click
        if(Input.GetMouseButtonDown(0))
        {
            //check what we have clicked on via a raycast
            Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D _hit = Physics2D.Raycast(_ray.origin, _ray.direction);

            //if the item is valid
            if(_hit.collider != null && _hit.collider.gameObject.GetComponent<Token>())
            {
                if (_isMoving) return;

                Token _token = _hit.collider.gameObject.GetComponent<Token>();

                SelectToken(_token);
            }
        }
    }

    //init board method
    void InitBoard()
    {
        DestroyTokens();

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
                //set pos of each node (used index of both loops (subtract node spacing))
                Vector2 _position = new Vector2(x - _spacingX, y - _spacingY);

                //get random prefab from array (get random token prefab to place)
                int _randIndx = Random.Range(0, _tokenPrefabs.Length);

                //use sourced _arrLayout (in inspector) to check whether a node should be usable or not
                if (_arrLayout.rows[y].row[x])
                    _gameBoard[x, y] = new Node(false, null);
                else
                {
                    GameObject _token = Instantiate(_tokenPrefabs[_randIndx], _position, Quaternion.identity); //create game object of token in-engine
                    _token.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f); //set size/scale of token(s)
                    _token.GetComponent<Token>().SetIndances(x, y); //set indaces (pos) of token
                    _gameBoard[x, y] = new Node(true, _token); //pos onto board and create a new node (_isMoveable and _tokenGameObj)

                    _tokensToDestroy.Add(_token);
                }

            }
        }
//        InitBoard();
    }

    private void DestroyTokens()
    {
        if(_tokensToDestroy != null)
        {
            foreach(GameObject _token in _tokensToDestroy)
            {
                Destroy(_token);
            }

            _tokensToDestroy.Clear();
        }
    }

    public bool BoardCheck()
    {
        Debug.Log("Checking Board");
        bool _hasMatched = false; //pairing bool

        List<Token> _tokensToRemove = new List<Token>(); //list of tokens to be removed

        //loop through board
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                //is the node (on board) usable?
                if (_gameBoard[x, y]._isUsable)
                {
                    //get token game object from board
                    Token _token = _gameBoard[x, y]._tokenObj.GetComponent<Token>();

                    //if token is NOT already matched
                    if (!_token._isMatched)
                    {
                        //check to see if token is connected to other tokens
                        MatchResult _matchedTokens = IsConnected(_token);

                        //if 3 or more are connected
                        if(_matchedTokens._connectedTokens.Count >= 3)
                        {
                            MatchResult _greaterMatchedTokens = AdvancedMatch(_matchedTokens);

                            _tokensToRemove.AddRange(_greaterMatchedTokens._connectedTokens);

                            foreach(Token _t in _greaterMatchedTokens._connectedTokens)
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

    private MatchResult AdvancedMatch(MatchResult _pMatchedTokens)
    {
        //check for horizontal or longer horizontal match
        if (_pMatchedTokens._direction == MatchDirection.Horizontal || _pMatchedTokens._direction == MatchDirection.LongHorizontal)
        {
            //for every token in the connected tokens
            foreach (Token _token in _pMatchedTokens._connectedTokens)
            {
                //make a list of the extra (advanced) matched tokens
                List<Token> _extraConnectedTokens = new List<Token>();

                //if horizontal match is  found, check for extra or 'advanced' connections
                CheckTokenDirection(_token, new Vector2Int(0, 1), _extraConnectedTokens); //check up
                CheckTokenDirection(_token, new Vector2Int(0, -1), _extraConnectedTokens); //check down

                //if 2 or more extra connections are found - register connected tokens
                if (_extraConnectedTokens.Count >= 2)
                {
                    Debug.Log("Horizontal Advanced Match Found.");
                    _extraConnectedTokens.AddRange(_pMatchedTokens._connectedTokens);

                    //return out advanced match
                    return new MatchResult
                    {
                        _connectedTokens = _extraConnectedTokens,
                        _direction = MatchDirection.Super
                    };
                }
            }
            return new MatchResult
            {
                _connectedTokens = _pMatchedTokens._connectedTokens,
                _direction = _pMatchedTokens._direction
            };
        }

        //check for vertical or longer vertical match
        else if (_pMatchedTokens._direction == MatchDirection.Vertical || _pMatchedTokens._direction == MatchDirection.LongVertical)
        {
            //for every token in the connected tokens
            foreach (Token _token in _pMatchedTokens._connectedTokens)
            {
                //make a list of the extra (advanced) matched tokens
                List<Token> _extraConnectedTokens = new List<Token>();

                //if vertical match is  found, check for extra or 'advanced' connections
                CheckTokenDirection(_token, new Vector2Int(1, 0), _extraConnectedTokens);
                CheckTokenDirection(_token, new Vector2Int(-1, 0), _extraConnectedTokens);

                //if 2 or more extra connections are found - register connected tokens
                if (_extraConnectedTokens.Count >= 2)
                {
                    Debug.Log("Vertical Advanced Match Found.");
                    _extraConnectedTokens.AddRange(_pMatchedTokens._connectedTokens);

                    //return new match
                    return new MatchResult
                    {
                        _connectedTokens = _extraConnectedTokens,
                        _direction = MatchDirection.Super
                    };
                }
            }
            return new MatchResult
            {
                _connectedTokens = _pMatchedTokens._connectedTokens,
                _direction = _pMatchedTokens._direction
            };
        }
        return null;

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

        //clear list of connected tokens between <-> and  ^  checks, and then reinstate the starting token
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

    #region Swap Tokens

    public void SelectToken(Token _pToken)
    {
        //select token if one is not yet selected
        if(_selectedToken == null)
        {
            Debug.Log(_pToken);
            _selectedToken = _pToken;
        }
        //if same token is selected twice, unselect it
        else if(_selectedToken == _pToken)
        {
            _selectedToken = null;
        }
        //if selected token is not null and is not the current token -> try and swap the two tokens. Select again to unselect.
        else if(_selectedToken != _pToken)
        {
            TokenSwap(_selectedToken, _pToken);
            _selectedToken = null;
        }
    }   
    
    //swap the pos of two tokens
    private void TokenSwap(Token _pSCurrentToken, Token _targetToken)
    {
        //check adjacency
        if (!IsAdjacent(_pSCurrentToken, _targetToken))
        {
            return;
        }

        SwapTokenPositions(_pSCurrentToken, _targetToken);
        _isMoving = true;

        StartCoroutine(ProcessTokenMatches(_pSCurrentToken, _targetToken));
    }

    private bool IsAdjacent(Token _pSCurrentToken, Token _targetToken)
    {
        //retuen as 1 (true) if the reletive position(s) == 1
        return Mathf.Abs(_pSCurrentToken._indX - _targetToken._indX) + Mathf.Abs(_pSCurrentToken._indY - _targetToken._indY) == 1;
    }

    private void SwapTokenPositions(Token _pSCurrentToken, Token _targetToken)
    {
        //temp save place for one of the tokens
        GameObject _tempToken = _gameBoard[_pSCurrentToken._indX, _pSCurrentToken._indY]._tokenObj;

        //swap the tokens by overriding the current with the target, and then the old target with temp (current)
        _gameBoard[_pSCurrentToken._indX, _pSCurrentToken._indY]._tokenObj = _gameBoard[_targetToken._indX, _targetToken._indY]._tokenObj;
        _gameBoard[_targetToken._indX, _targetToken._indY]._tokenObj = _tempToken;

        //update indices
        int _tempIndX = _pSCurrentToken._indX;
        int _tempIndY = _pSCurrentToken._indY;

        _pSCurrentToken._indX = Mathf.Clamp(_targetToken._indX, 0, _width - 1);
        _pSCurrentToken._indY = Mathf.Clamp(_targetToken._indY, 0, _height - 1);
        _targetToken._indX = Mathf.Clamp(_tempIndX, 0, _width - 1);
        _targetToken._indY = Mathf.Clamp(_tempIndY, 0, _height - 1);

        //apply movement
        _pSCurrentToken.MoveToTargetPos(_gameBoard[Mathf.Clamp(_targetToken._indX, 0, _width - 1), Mathf.Clamp(_targetToken._indY, 0, _height - 1)]._tokenObj.transform.position);
        _targetToken.MoveToTargetPos(_gameBoard[Mathf.Clamp(_pSCurrentToken._indX, 0, _width - 1), Mathf.Clamp(_pSCurrentToken._indY, 0, _height - 1)]._tokenObj.transform.position);
    }



    private IEnumerator ProcessTokenMatches(Token _pCurrentPos, Token _pTargetPos)
    {
        yield return new WaitForSeconds(0.25f);

        bool matched = BoardCheck();

        if(!matched)
        {
            SwapTokenPositions(_pCurrentPos, _pTargetPos);
        }
        _isMoving = false;
    }


    #endregion
}









//class to store a list of the connected tokens and the direction of the pairing
//class used instead of struct so that it is stored in the heap
public class MatchResult
{
    public List<Token> _connectedTokens;
    public MatchDirection _direction;
}

//enum to store the directions of pairings
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
