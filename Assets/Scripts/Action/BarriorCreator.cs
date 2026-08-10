using System.Collections;
using UnityEngine;

public class BarriorCreator : MonoBehaviour
{
    [Header("시간에 따른 이동")]
    [SerializeField] private float moveDelay = 3f; // 이 시간이 지나면 targetPosition으로 순간이동한다.
    [SerializeField] private Vector3 targetPosition; // 이동할 목표 위치 (월드 좌표)

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(MoveAfterDelay());
    }

    // moveDelay초가 지나면 targetPosition으로 즉시 순간이동시킨다.
    private IEnumerator MoveAfterDelay()
    {
        yield return new WaitForSeconds(moveDelay);

        transform.position = targetPosition;
    }
}
