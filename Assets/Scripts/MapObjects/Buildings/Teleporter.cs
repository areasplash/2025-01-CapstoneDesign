using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections;

public class Teleporter : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform targetPosition;
    [SerializeField] private bool isEntrance = true;
    [SerializeField] private bool fadeTransition = true;
    [SerializeField] private bool isLocked = false;
    [SerializeField] private string lockComment = "";

    private bool isInRange;

    // interactable
    public InteractionType InteractionType => isEntrance? InteractionType.BuildingEntry : InteractionType.BuildingExit;
    public bool IsInteractable { get { return true; } }


    public void Interact(GameObject interactor) {
        if (isLocked) {
            // 잠김 문구 출력

            //UIManager.EnqueueDialogue("", lockComment);

            // 1) 없던 → 등장 (큐 파라미터로 BG 지정)
            UIManager.EnqueueDialogue("Elden", "……여긴, 낯익은 방이야.", "D_D1_01");

            // 2) 유지 (배경 파라미터 null → 유지)
            UIManager.EnqueueDialogue("Elden", "벽지 냄새가 그대로 남아있네.", null);

            // 3) 인라인 전환 (대사 중간에 01 → 02로 교체)
            UIManager.EnqueueDialogue("Elden",
                "창문 쪽을 바라보니{Delay(0.2)}… {BG(D_D1_02)}빛이 바뀌었다. 그리고 잠깐, 무언가가 스친다.", null);

            // 4) 또 다른 인라인 전환 (02 → 03), 딜레이 섞기
            UIManager.EnqueueDialogue("Elden",
                "숨을 고르고{Delay(0.15)}… {BG(D_D1_03)}기억의 파편이 하나씩 떠오른다.", null);

            // 5) 큐 파라미터로 다른 장면(03 → 04) (크로스 페이드)
            UIManager.EnqueueDialogue("Elden",
                "거실로 걸음을 옮기자, 오래된 액자들이 눈에 들어온다.", "D_D1_04");

            // 6) 또 다른 장면 (04 → 05)
            UIManager.EnqueueDialogue("Elden",
                "사진 속 미소가 왜 이렇게 또렷하지…", "D_D1_05");

            // 7) 인라인으로 05 → 06
            UIManager.EnqueueDialogue("Elden",
                "문득 떠오르는 소리.{Delay(0.2)} {BG(D_D1_06)}도어락 ‘삑’ 하는 소리만 들어도 가슴이 쿵 내려앉았어.", null);

            // 8) 인라인으로 06 → 07
            UIManager.EnqueueDialogue("Elden",
                "하지만 지금은… {Delay(0.15)} {BG(D_D1_07)}그때와는 다르다고, 스스로에게 말해본다.", null);

            // 9) 클리어 (있던 → 없음) : 자연스러운 페이드아웃
            UIManager.EnqueueDialogue("Elden",
                "눈을 감고, 천천히 숨을 내쉰다. {Delay(0.25)}{BG()}이제 괜찮아.", null);

            // 10) 다시 등장 테스트 (없던 → 01)
            UIManager.EnqueueDialogue("Elden",
                "…그리고 조심스레 눈을 뜬다.", "D_D1_01");
            return;
        }
        if (fadeTransition) { FadeAndTeleport(interactor).Forget(); }
        else { Teleport(interactor); }
    }

    private void Teleport(GameObject interactor) {
        interactor.transform.position = targetPosition.position;
    }

    

    private async UniTask FadeAndTeleport(GameObject interactor) {
        // Fade 화면 열기
        FadeLoadingScreen fade = (FadeLoadingScreen)UIManager.OpenScreen(Screen.FadeLoading);

        // Fade 재생 중간에 텔레포트 실행
        await fade.Play(Color.black, async () =>
        {
            Teleport(interactor);
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
        });
    }


	
	void Update() {
		float time = EnvironmentManager.TimeOfDay % 1f;
		isLocked = (20f / 24f) <= time || time < (6f / 24f);
	}
}
