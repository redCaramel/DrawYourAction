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
        // 이전 카드의 행동이 아직 실행 중이었다면, 새 카드를 적용하기 전에 즉시 취소
        if (loading)
        {
            PlayerController.instance.StopMovement();
        }

        _queue = new Queue<Action>(acts);
        _elapsedTime = 0f;
        loading = true;
    }
    public bool isLoading()
    {
        return loading;
    }
}
