using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Button btnRecord;
    [SerializeField] private Button btnPlay;

    public void Awake()
    {
        btnRecord.onClick.AddListener(OnRecordButtonClicked);
        btnPlay.onClick.AddListener(OnPlayButtonClicked);
    }

    void Update()
    {
        if(ActionLoader.instance.isLoading()) return;

        if(Input.GetKeyDown(KeyCode.I)) ActionLoader.instance.StartLoading(ActionRecorder.instance.GetAction());

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

    // Setting Event in Click UIButton

    public void OnRecordButtonClicked()
    {
        ActionRecorder.instance.StartRecording(5);
    }
    public void OnPlayButtonClicked()
    {
        ActionLoader.instance.StartLoading(ActionRecorder.instance.GetAction(ScriptSelector.instance.GetScriptIndex()));
    }
    
}
