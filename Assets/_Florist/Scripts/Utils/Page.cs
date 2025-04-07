using System;
using UnityEngine;

public abstract class Page : MonoBehaviour
{
    public abstract void Open(Action onCompleted = null);

    public abstract void Close(Action onCompleted = null);
}
