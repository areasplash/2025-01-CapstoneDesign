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

            UIManager.EnqueueDialogue("예진", "안녕 반가워~");
            UIManager.EnqueueDialogue("예진", "어떻게 할래?", () => {
                // 2) 마지막 대사의 onEnd에서 선택지 열기
                UIManager.BeginChoices();

                UIManager.AddChoice("선택지1", () => {
                    UIManager.EnqueueDialogue("플레이어", "선택지1을 골랐음");
                    UIManager.EnqueueDialogue("예진", "좋아, 그럼 시작하자!");
                });

                UIManager.AddChoice("선택지2", () => {
                    UIManager.EnqueueDialogue("플레이어", "선택지2를 골랐음");

                });

                if (QuestManager.Instance.IsMissionActive("quest1", 1)) {
                    UIManager.AddChoice("퀘스트1에 대하여", () => {
                        UIManager.EnqueueDialogue("예진", "아, 너였구나~!");
                    });
                }

                UIManager.ShowChoices();
            });
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
