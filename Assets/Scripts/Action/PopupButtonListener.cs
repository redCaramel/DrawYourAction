using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PopupButtonListener : MonoBehaviour
{
    [SerializeField] private Button s_againBtn; // 성공 - 재촬영
    [SerializeField] private Button s_backBtn; // 성공 - 나가기
    [SerializeField] private Button f_againBtn; // 실패 - 그대로 다시
    [SerializeField] private Button f_recordBtn; // 실패 - 녹화 다시
    [SerializeField] private Button f_backBtn; // 실패 - 나가기
    [SerializeField] private Button f_previewBtn; // 실패 - 프리뷰다시
    [SerializeField] private Button p_recordBtn; // 프리뷰 - 녹화
    [SerializeField] private Button p_backBtn; // 프리뷰 - 나가기
    [SerializeField] private Button p_againBtn; //프리뷰 - 프리뷰다시

    void Awake()
    {
        s_againBtn.onClick.AddListener(s_again);
        s_backBtn.onClick.AddListener(s_back);
        f_againBtn.onClick.AddListener(f_again);
        f_recordBtn.onClick.AddListener(f_record);
        f_backBtn.onClick.AddListener(f_back);
        f_previewBtn.onClick.AddListener(f_preview);
        p_recordBtn.onClick.AddListener(p_record);
        p_backBtn.onClick.AddListener(p_back);
        p_againBtn.onClick.AddListener(p_again);
    }
    private void s_again()
    {
        SceneManager.LoadScene(StageImporter.sceneName);
    }
    private void s_back()
    {
        SceneManager.LoadScene("StageScene");
    }
    private void f_again()
    {
        SceneManager.LoadScene(StageImporter.sceneName);
    }
    private void f_record()
    {
        StageModeSetting.setMode(false);
        SceneManager.LoadScene("RecordDevelopingScene");
    }
    private void f_back()
    {
        SceneManager.LoadScene("StageScene");
    }
    private void f_preview()
    {
        StageModeSetting.setMode(true);
        SceneManager.LoadScene(StageImporter.sceneName);
    }
    private void p_record()
    {
        StageModeSetting.setMode(false);
        SceneManager.LoadScene("RecordDevelopingScene");
    }
    private void p_back()
    {
        SceneManager.LoadScene("StageScene");
    }
    private void p_again()
    {
        StageModeSetting.setMode(true);
        SceneManager.LoadScene(StageImporter.sceneName);
    }
}
