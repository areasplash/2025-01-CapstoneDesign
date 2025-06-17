using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ServerRequestManager : MonoBehaviour
{
        public static ServerRequestManager Instance;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    // 텍스트 분석 요청 함수
    public void RequestTextAnalysis(string inputText) {
        StartCoroutine(PostText(inputText));
    }

    // 서버로 텍스트를 POST하고 응답 로그 출력
    private IEnumerator PostText(string input) {
        // 테스트 서버 -> 실제 서버 주소로 수정 필요
        string url = "https://8ba6-125-188-126-212.ngrok-free.app/analyze/text";

        // json 형식으로 요청 형식 변환
        string json = JsonUtility.ToJson(new TextPayload { text = input });
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

        // POST 요청 생성
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("[서버 요청] 텍스트 분석 요청 중...");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            Debug.Log("[서버 응답 수신 완료]");

            /*
            서버 응답 형식 예시
            {
                (작성 예정 아무튼 JSON)
            }
            */
            Debug.Log($"응답 내용: {request.downloadHandler.text}");
        }
        else {
            Debug.LogError($"서버 요청 실패: {request.error}");
        }
    }

    [System.Serializable]
    private class TextPayload {
        public string text;
    }
}

/*
// 서버 응답 파싱용 클래스
[System.Serializable]
private class GPTResponse
{
    public string text;
    public EmotionData emotion;
    public string response;
}

[System.Serializable]
private class EmotionData
{
    public string label;
    public float score;
}
*/