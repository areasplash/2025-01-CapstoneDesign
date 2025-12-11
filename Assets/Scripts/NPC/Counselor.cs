using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Counseler : MonoBehaviour, IInteractable {
    // interactable
    public InteractionType InteractionType => InteractionType.Talk;
    public bool IsInteractable { get { return true; } }

    private string lastUserInput = "";

    public void Interact(GameObject interactor) {
        StartCoroutine(RunInteraction());
    }

    private IEnumerator RunInteraction() {
        // 1) 인사
        UIManager.EnqueueDialogue("상담사", "안녕~ 반가워요", null);
        yield return WaitDialogueDrain();

        // 2) 선택지
        int selected = -1;

        UIManager.BeginChoices();
        UIManager.AddChoice("얘기 좀 들어줄 수 있어요?", () => { selected = 0; });
        UIManager.AddChoice("안녕!", () => { selected = 1; });
        UIManager.ShowChoices();

        // 선택될 때까지 대기
        yield return new WaitUntil(() => selected >= 0);

        if (selected == 0) {
            // 상담 시작 멘트
            UIManager.EnqueueDialogue("상담사", "그럼요 편하게 말해보세요. 다 들어드릴게요!", null);
            yield return WaitDialogueDrain();

            // 3) 사용자 입력 받기
            bool inputDone = false;
            UIManager.BeginDialogueInput(data =>
            {
                lastUserInput = data.text ?? "";
                inputDone = true;
                Debug.Log($"[Counseler] 사용자가 입력한 내용: {lastUserInput}");
            });

            // 입력 끝날 때까지 기다리기
            yield return new WaitUntil(() => inputDone);

            // 입력 끝난 뒤 간단한 응답
            UIManager.EnqueueDialogue("상담사", "좋아요. 말씀해줘서 고마워요.", null);
            yield return WaitDialogueDrain();
        }
        else {
            UIManager.EnqueueDialogue("상담사", "안녕! 오늘 하루 어땠나요?", null);
            yield return WaitDialogueDrain();
        }
    }

    private IEnumerator WaitDialogueDrain()
    {
        yield return null;
        while (UIManager.HasPendingDialogue
            || UIManager.IsDialogueHeld
            || UIManager.IsChoiceOpen
            || UIManager.IsDialogueInput)
        {
            yield return null;
        }
        yield return null;
    }
}
