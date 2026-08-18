using UnityEngine;

public class PreviewInputManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        MovementType move = MovementType.Idle;
        JumpType jump = JumpType.Idle;
        AttackType atk = AttackType.Idle;
            if(Input.GetKey(KeyCode.W)) 
                atk = AttackType.AttackNormal;
            else if(Input.GetKey(KeyCode.A))
                move = MovementType.LeftNormal;
            else if(Input.GetKey(KeyCode.D))
                move = MovementType.RightNormal;

            if (Input.GetKeyDown(KeyCode.Space))
                jump = JumpType.JumpNormal;

        
        Action act = default;
        act.move = move;
        act.jump = jump;
        act.atk = atk;

        PreviewPlayerMover.instance.ExecuteAction(act);
    }
}
