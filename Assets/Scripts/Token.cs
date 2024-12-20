using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Token : MonoBehaviour
{
    //enum of token colour (type)
    public TokenType _tokenType;

    //index (pos on grid)
    public int _indX;
    public int _indY;

    //positions (used for switching)
    private Vector2 _currentPos;
    private Vector2 _desiredPos;

    //bools
    public bool _isMatched;
    public bool _isMoving;

    //constructor to init values
    public Token(int _pIndX, int _pIndY)
    {
        _indX = _pIndX; 
        _indY = _pIndY;
    }

    //used to set the grid pos of the token
    public void SetIndances(int _pindX, int _pIndY)
    {
        _indX = _pindX;
        _indY = _pIndY;
    }

    public void MoveToTargetPos(Vector2 _pTargetPos)
    {
        StartCoroutine(MoveToTargetPosCoroutine(_pTargetPos));
    }

    private IEnumerator MoveToTargetPosCoroutine(Vector2 _pTargetPos)
    {
        //flag bool
        _isMoving = true;

        //timer info
        float duration = 0.25f;
        float _timeTaken = 0;

        //pos
        Vector2 _startPos = transform.position;

        //make the swapping of two tokens happen over a given period of time
        while(_timeTaken < duration)
        {
            float time = _timeTaken / duration;

            transform.position = Vector2.Lerp(_startPos, _pTargetPos, time);

            _timeTaken += Time.deltaTime;

            yield return null;
        }

        transform.position = _pTargetPos;
        _isMoving = false;
    }

}

//enum to hold token types/colours
public enum TokenType
{
    Yellow,
    Green,
    Purple,
    Red
}
