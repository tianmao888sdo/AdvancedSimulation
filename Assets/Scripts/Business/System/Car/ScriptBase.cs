using UnityEngine;
using System.Collections;

public abstract class ScriptBase : MonoBehaviour
{
    public abstract void Init();
    public abstract void Play();
    public abstract void Stop();
}
