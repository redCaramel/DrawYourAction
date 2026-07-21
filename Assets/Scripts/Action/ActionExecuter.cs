using System.Collections.Generic;
using UnityEngine;

public class ActionExecuter : MonoBehaviour
{
    private Queue<Action> _queue = new Queue<Action>();
    private bool loading = false;
    private float _elapsedTime = 0f;

    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify here
    public static ActionExecuter instance {get; private set;}

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic()
    {
        instance = null;
    }
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else Destroy(gameObject);
    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    // ----------------------------------------------------

    public void Update()
    {
        if (!loading) return;

        _elapsedTime += Time.deltaTime;

        while (_queue.Count > 0 && _queue.Peek().timestamp <= _elapsedTime)
        {
            PlayerController.instance.ExecuteAction(_queue.Dequeue());
        }
        if (_queue.Count == 0)
        {
            loading = false;
            PlayerController.instance.StopMovement();
        }
    }

    public void StartLoading(List<Action> acts)
    {
        _queue = new Queue<Action>(acts);
        _elapsedTime = 0f;
        loading = true;
    }
    public bool isLoading()
    {
        return loading;
    }
}
