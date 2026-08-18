using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 컷씬용 카메라 이동/대기 로직을 모아둔 컴포넌트. CutSceneManager1, 2, 3...에서 이 메소드들을 묶어서 사용
public class CutSceneCameraMover : MonoBehaviour
{
    [System.Serializable]
    public struct CutSceneStep
    {
        public bool isWaitStep;        // true: n초 대기, false: 좌표 이동
        public bool isPlayerMoveStep;  // isWaitStep이 false일 때만 사용. true면 플레이어를 targetPosition까지 이동(카메라가 따라감), false면 카메라만 이동
        public Vector3 targetPosition; // 이동 단계에서 사용할 목표 좌표
        public float duration;         // 대기/카메라 이동 시간(초). 플레이어 이동 단계에서는 사용하지 않음(StatManager의 이동속도로 자동 계산)
    }

    [SerializeField] private Transform cutSceneCamera;
    [SerializeField] private Transform player; // 컷씬 중 이동시킬 플레이어. 비워두면 PlayerController.instance를 사용
    [SerializeField] private Animator playerAnimator; // 비워두면 player에서 자동으로 가져옴
    [SerializeField] private SpriteRenderer playerSprite; // 비워두면 player에서 자동으로 가져옴

    private static readonly int SpeedParam = Animator.StringToHash("Speed");

    void Awake()
    {
        if (cutSceneCamera == null && Camera.main != null)
        {
            cutSceneCamera = Camera.main.transform;
        }

        if (player == null && PlayerController.instance != null)
        {
            player = PlayerController.instance.transform;
        }

        if (player != null)
        {
            if (playerAnimator == null) playerAnimator = player.GetComponent<Animator>();
            if (playerSprite == null) playerSprite = player.GetComponent<SpriteRenderer>();
        }
    }

    // 카메라를 duration초에 걸쳐 targetPosition으로 이동
    public IEnumerator MoveTo(Vector3 targetPosition, float duration)
    {
        if (cutSceneCamera == null) yield break;

        Vector3 startPosition = cutSceneCamera.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            cutSceneCamera.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        cutSceneCamera.position = targetPosition;
    }

    // seconds초 동안 대기
    public IEnumerator Wait(float seconds)
    {
        if (seconds > 0f)
        {
            yield return new WaitForSeconds(seconds);
        }
    }

    // 플레이어를 현재 위치에서 targetPosition까지 StatManager의 이동속도(playerSpeed)로 이동시키고,
    // 이동하는 동안 카메라가 플레이어와의 상대 위치(오프셋)를 유지한 채 따라가며, Animator의 Speed 파라미터를 1로 설정한다.
    // 2D 게임이므로 targetPosition의 z는 무시하고 플레이어/카메라의 기존 z(깊이)를 그대로 유지한다.
    // (z까지 그대로 이동/추적하면 인스펙터에 잘못된 z값이 들어갔을 때 플레이어나 카메라가 깊이축으로 밀려나
    //  스프라이트 정렬이 꼬이거나 화면에서 사라지는 것처럼 보일 수 있다.)
    // lockPlayerControl이 true면 이동 중 플레이어 입력을 잠가서 조작과 겹치지 않게 한다.
    public IEnumerator MovePlayerTo(Vector3 targetPosition, bool lockPlayerControl = true)
    {
        if (player == null) yield break;

        if (lockPlayerControl && PlayerController.instance != null)
        {
            PlayerController.instance.SetControlLocked(true);
        }

        if (playerSprite != null) playerSprite.flipX = targetPosition.x < player.position.x;
        if (playerAnimator != null) playerAnimator.SetFloat(SpeedParam, 1f);

        float speed = StatManager.instance.playerSpeed;
        float playerZ = player.position.z;
        Vector3 target = new(targetPosition.x, targetPosition.y, playerZ);
        Vector2 cameraOffsetXY = cutSceneCamera != null ? (Vector2)cutSceneCamera.position - (Vector2)player.position : Vector2.zero;
        float cameraZ = cutSceneCamera != null ? cutSceneCamera.position.z : 0f;

        if (speed <= 0f)
        {
            player.position = target;
            if (cutSceneCamera != null) cutSceneCamera.position = new Vector3(player.position.x + cameraOffsetXY.x, player.position.y + cameraOffsetXY.y, cameraZ);
        }
        else
        {
            while ((player.position - target).sqrMagnitude > 0.0001f)
            {
                player.position = Vector3.MoveTowards(player.position, target, speed * Time.deltaTime);

                if (cutSceneCamera != null)
                {
                    cutSceneCamera.position = new Vector3(player.position.x + cameraOffsetXY.x, player.position.y + cameraOffsetXY.y, cameraZ);
                }

                yield return null;
            }

            player.position = target;
            if (cutSceneCamera != null) cutSceneCamera.position = new Vector3(player.position.x + cameraOffsetXY.x, player.position.y + cameraOffsetXY.y, cameraZ);
        }

        if (playerAnimator != null) playerAnimator.SetFloat(SpeedParam, 0f);

        if (lockPlayerControl && PlayerController.instance != null)
        {
            PlayerController.instance.SetControlLocked(false);
        }
    }

    // steps를 등록된 순서대로 하나씩 재생 (이동/대기를 묶어서 처리)
    public IEnumerator PlaySteps(IEnumerable<CutSceneStep> steps)
    {
        foreach (var step in steps)
        {
            if (step.isWaitStep)
            {
                yield return Wait(step.duration);
            }
            else if (step.isPlayerMoveStep)
            {
                yield return MovePlayerTo(step.targetPosition);
            }
            else
            {
                yield return MoveTo(step.targetPosition, step.duration);
            }
        }
    }
}
