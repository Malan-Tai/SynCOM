using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterruptionQueue : MonoBehaviour
{
    private Queue<Interruption> _queue;

    private void Awake()
    {
        _queue = new Queue<Interruption>();
    }

    void Update()
    {
        if (_queue.Count <= 0) return;

        Interruption head = _queue.Peek();
        if (head.IsDone) _queue.Dequeue();
        else if (!head.HasStarted) head.StartCoroutine(this);
    }

    public bool IsEmpty()
    {
        return _queue.Count <= 0;
    }

    public void Enqueue(Interruption interruption)
    {
        _queue.Enqueue(interruption);
    }
}
