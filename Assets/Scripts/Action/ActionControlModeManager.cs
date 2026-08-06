using UnityEngine;

/// <summary>
/// ActionScene에서 사용할 조작 방식.
/// Card   : 카드를 사용한 조작 (CardSpawnManager / ScriptDragManager / ActionExecuter)
/// Record : Card 관련 기능을 비활성화하고, Recording 씬처럼 A / D / Space 키를 직접 입력받아 조작
/// </summary>
public enum ActionControlMode
{
    Card,
    Record
}

public class ActionControlModeManager : MonoBehaviour
{
    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify here
    public static ActionControlModeManager instance {get; private set;}

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic()
    {
        instance = null;
        CurrentMode = ActionControlMode.Card;
    }
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else Destroy(gameObject);

        // ActionScene의 나머지 기능(카드 스폰 등)이 Start에서 동작하기 전에
        // 조작 방식을 먼저 확정한다. (모든 오브젝트의 Awake는 Start보다 먼저 호출됨)
        DecideControlMode();
    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    // ----------------------------------------------------

    /// <summary>
    /// 이번 ActionScene에 실제로 적용된 조작 방식.
    /// 외부 클래스는 이 값(또는 IsCardMode / IsRecordMode)으로 현재 조작 방식을 확인할 수 있다.
    /// </summary>
    public static ActionControlMode CurrentMode { get; private set; } = ActionControlMode.Card;

    public static bool IsCardMode => CurrentMode == ActionControlMode.Card;
    public static bool IsRecordMode => CurrentMode == ActionControlMode.Record;

    private void DecideControlMode()
    {
        // StageModeSetting.isPreview가 true면 Record(=Preview) 모드, false면 Card 모드로 시작한다.
        CurrentMode = StageModeSetting.isPreview ? ActionControlMode.Record : ActionControlMode.Card;

        if (CurrentMode == ActionControlMode.Record)
        {
            // Card 기반 조작(카드 스폰/드래그/사용)을 비활성화한다.
            if (CardSpawnManager.instance != null)
                CardSpawnManager.instance.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (CurrentMode != ActionControlMode.Record) return;
        if (ActionExecuter.instance != null && ActionExecuter.instance.isLoading()) return;

        // Recording 씬의 InputManager와 동일한 A / D / Space 기반 직접 조작
        MovementType move = MovementType.Idle;
        JumpType jump = JumpType.Idle;
        AttackType atk = AttackType.Idle;

        if (Input.GetKey(KeyCode.A))
            move = MovementType.LeftNormal;
        else if (Input.GetKey(KeyCode.D))
            move = MovementType.RightNormal;

        if (Input.GetKeyDown(KeyCode.Space))
            jump = JumpType.JumpNormal;

        Action act = default;
        act.move = move;
        act.jump = jump;
        act.atk = atk;

        PlayerController.instance.ExecuteAction(act);
    }
}
