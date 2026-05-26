using UnityEngine;

public class InputManager : MonoBehaviour
{
    void Update()
    {
        if(ActionLoader.instance.isLoading()) return;

        if(Input.GetKeyDown(KeyCode.I)) ActionLoader.instance.StartLoading(ActionRecorder.instance.GetAction());
        if(Input.GetKeyDown(KeyCode.O)) ActionRecorder.instance.StartRecording(5);

        MovementType move = MovementType.Idle;
        JumpType jump = JumpType.Idle;
        AttackType atk = AttackType.Idle;

        if(Input.GetKey(KeyCode.A))
            move = MovementType.LeftNormal;
        else if(Input.GetKey(KeyCode.D))
            move = MovementType.RightNormal;

        if (Input.GetKeyDown(KeyCode.Space))
            jump = JumpType.JumpNormal;

        Action act = default;
        act.move = move;
        act.jump = jump;
        act.atk = atk;

        PlayerController.instance.ExecuteAction(act);

        if (ActionRecorder.instance.isRecording())
            ActionRecorder.instance.ApplyAction(act);
    }
}
