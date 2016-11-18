using UnityEngine;
using System.Collections;

public class ScriptBase : MonoBehaviour {

    public bool pause = true;

	// Update is called once per frame
	void Update ()
    {
        if (pause)
            return;

        BUpdate();
    }

    protected virtual void BUpdate()
    {

    }

    private void FixedUpdate()
    {
        if (pause)
            return;

        BFixedUpdate();
    }

    protected virtual void BFixedUpdate()
    {

    }
}
