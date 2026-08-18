using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleExiter : MonoBehaviour
{
    [SerializeField] private Button btn;

    void Awake()
    {
        btn.onClick.AddListener(exit);
    }
    void exit()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
