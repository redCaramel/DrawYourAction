using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string saveFilePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        saveFilePath = Path.Combine(Application.persistentDataPath, "save_slot_1.json");
    }

    // 💾 데이터 저장 (Save)
    public void SaveGame(SaveData data)
    {
        // 객체를 JSON 포맷 문자열로 변환 (true: 보기 좋게 줄바꿈 포맷팅)
        string json = JsonUtility.ToJson(data, true);

        // 파일 쓰기
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"[SaveManager] 게임 저장 완료: {saveFilePath}");
    }

    // 📂 데이터 불러오기 (Load)
    public SaveData LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("[SaveManager] 저장된 세이브 파일이 없어 새로 생성합니다.");
            return new SaveData(); // 새 게임 데이터 반환
        }

        // 파일 읽기
        string json = File.ReadAllText(saveFilePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        return data;
    }

    // 🗑️ 세이브 데이터 삭제 (초기화)
    public void DeleteSave()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("[SaveManager] 세이브 파일 삭제 완료.");
        }
    }

    // 세이브 파일 존재 여부 확인
    public bool HasSaveFile()
    {
        return File.Exists(saveFilePath);
    }

    // 💾 현재 게임 상태(스테이지 진행도 + 볼륨) 자동 저장
    // StageClearManager.currentStage, VolumeManager의 볼륨이 변경될 때 호출됨
    public void SaveCurrentState()
    {
        SaveData data = new SaveData
        {
            currentStage = StageClearManager.currentStage,
            bgmVolume = VolumeManager.instance.bgmVolume,
            sfxVolume = VolumeManager.instance.sfxVolume
        };
        SaveGame(data);
    }
}