using UnityEngine;

public class StairAutoUp : MonoBehaviour
{
    public enum StairDirection
    {
        RightToLeft, // 오른쪽에서 왼쪽으로 이동할 때 올라감
        LeftToRight  // 왼쪽에서 오른쪽으로 이동할 때 올라감
    }

    [Header("계단 오르기 조건 설정")]
    [SerializeField] private StairDirection climbDirection = StairDirection.RightToLeft;

    [Header("상승 속도 및 감지 설정")]
    [SerializeField] private float liftForce = 4f;
    [SerializeField] private float minVelocityThreshold = 0.1f;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb == null) return;

            // 플레이어의 실제 X축 이동 속도 확인
            // Unity 2023.1+ / Unity 6: rb.linearVelocity.x
            // Unity 2022 이하: rb.velocity.x
            float currentVelocityX = rb.linearVelocity.x;

            bool shouldClimb = false;

            switch (climbDirection)
            {
                case StairDirection.RightToLeft:
                    // 왼쪽으로 이동 중(X 속도가 음수)일 때 올라감
                    shouldClimb = currentVelocityX < -minVelocityThreshold;
                    break;

                case StairDirection.LeftToRight:
                    // 오른쪽으로 이동 중(X 속도가 양수)일 때 올라감
                    shouldClimb = currentVelocityX > minVelocityThreshold;
                    break;
            }

            if (shouldClimb)
            {
                // Y축 속도를 부여하여 계단으로 올려줌
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, liftForce);
            }
        }
    }
}