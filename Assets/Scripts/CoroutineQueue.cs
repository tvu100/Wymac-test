using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CoroutineQueue
{
    MonoBehaviour _Owner = null;
    Coroutine _InternalCoroutine = null;
    Queue<IEnumerator> actions = new Queue<IEnumerator>();
    public CoroutineQueue(MonoBehaviour aCoroutineOwner)
    {
        _Owner = aCoroutineOwner;
    }
    public void StartLoop()
    {
        _InternalCoroutine = _Owner.StartCoroutine(Process());
    }
    public void StopLoop()
    {
        _Owner.StopCoroutine(_InternalCoroutine);
        _InternalCoroutine = null;
    }
    public void EnqueueAction(IEnumerator aAction)
    {
        actions.Enqueue(aAction);
    }

    private IEnumerator Process()
    {
        while (true)
        {
            if (actions.Count > 0)
                yield return _Owner.StartCoroutine(actions.Dequeue());
            else
                yield return null;
        }
    }
    public void EnqueueWait(IEnumerator wait)
    {
        actions.Enqueue(wait);
    }

    private IEnumerator Wait(float aWaitTime)
    {
        yield return new WaitForSeconds(aWaitTime);
    }
}