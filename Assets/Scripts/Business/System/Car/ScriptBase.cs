using UnityEngine;
using System.Collections;

public abstract class MonoBase:MonoBehaviour
{
    public abstract void Init();
}

public abstract class ScriptBase
{
	public abstract void Init();
	public abstract void Play ();
	public abstract void Stop();
}
