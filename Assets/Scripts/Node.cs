using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    //variables and refs
    public bool _isUsable;
    public GameObject _tokenObj;

    //constructor to init values
    public Node(bool _pIsUsable, GameObject _pTokenObj)
    {
        _isUsable = _pIsUsable;
        _tokenObj = _pTokenObj;
    }
}
