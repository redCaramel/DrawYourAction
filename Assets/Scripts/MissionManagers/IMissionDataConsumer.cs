// missionObjects 리스트에 등록된 미션 컴포넌트 중, 자신에게 대응하는 MissionData를
// 직접 갱신해야 하는 경우(currentValue 등) 구현한다.
// ActionMissionManager/PreviewMissonManager가 Init() 시점에 인덱스가 같은 MissionData를 주입해준다.
public interface IMissionDataConsumer
{
    void SetMissionData(MissionData data);
}
