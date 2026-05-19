using UnityEngine;

public class InputManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(ActionLoader.instance.isLoading()) return; 
        if(Input.GetKeyDown(KeyCode.I)) ActionLoader.instance.StartLoading(ActionRecorder.instance.GetAction());
        MovementType move = MovementType.Idle;
        JumpType jump = JumpType.Idle;
        AttackType atk = AttackType.Idle;
        if(Input.GetKey(KeyCode.A))
        {
            move = MovementType.LeftNormal;
        }
        else if(Input.GetKey(KeyCode.D))
        {
            move = MovementType.RightNormal;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jump = JumpType.JumpNormal;
        }
        if (ActionRecorder.instance.isRecording())
        {
            Action act;
            act.move = move;
            act.jump = jump;
            act.atk = atk;
            PlayerController.instance.ExecuteAction(act);
            ActionRecorder.instance.ApplyAction(act);
        }
    }
}
