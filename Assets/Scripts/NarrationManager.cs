using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;


public class NarrationManager : MonoBehaviour
{
    public static NarrationManager Instance;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI narrationText;

    [Header("Timing")]
    [SerializeField] float narrationCooldown = 3f;

    [SerializeField] AudioSource audioSource;
    float lastNarrationTime;
    bool isSpeaking;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TryNarrate(NarrationEvent e)
    {
        if (Time.time - lastNarrationTime < narrationCooldown)
            return;

        if (isSpeaking)
            return;

        lastNarrationTime = Time.time;
        StartCoroutine(Narrate(e));
    }

    IEnumerator Narrate(NarrationEvent e)
    {
        isSpeaking = true;

        string prompt = BuildPrompt(e);
        string narrationLine = "";

        yield return StartCoroutine(CallLLM(prompt, result =>
        {
            narrationLine = result;
        }));

        ShowText(narrationLine);

        yield return StartCoroutine(PlayVoice(narrationLine));

        isSpeaking = false;
    }

    string BuildPrompt(NarrationEvent e)
    {
        return
            $@"You are a dramatic sports commentator narrating a small battlefield.
            Speak ONE short sentence.
            Do not mention numbers or percentages.
            Do not explain mechanics.
            Be exciting but clear.

            Event:
            Type: {e.type}
            Player: Player {e.playerId}
            Zone: Zone {e.zoneId}";
    }

    void ShowText(string text)
    {
        narrationText.text = "🎙 " + text;
        CancelInvoke(nameof(ClearText));
        Invoke(nameof(ClearText), 4f);
    }

    void ClearText()
    {
        narrationText.text = "";
    }

    IEnumerator PlayVoice(string text)
    {
        AudioClip clip = null;

        yield return StartCoroutine(CallTTS(text, result =>
        {
            clip = result;
        }));

        // Fallback if TTS fails
        if (!clip)
            yield break;

        audioSource.clip = clip;
        audioSource.Play();

        yield return new WaitForSeconds(clip.length);
    }

    // ----------------------------
    // STUBS (replace later)
    // ----------------------------

 
        IEnumerator CallLLM(string prompt, System.Action<string> onComplete)
        {
            string url = "https://api.openai.com/v1/chat/completions";

            string json = JsonUtility.ToJson(new ChatRequest
            {
                model = "gpt-4o-mini",
                messages = new ChatMessage[]
                {
                    new ChatMessage { role = "system", content = "You are a concise battlefield narrator." },
                    new ChatMessage { role = "user", content = prompt }
                },
                max_tokens = 40,
                temperature = 0.8f
            });

            using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();

                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Authorization", "Bearer " + OpenAIConfig.ApiKey);

                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("LLM error: " + req.error);
                    onComplete.Invoke("The battlefield hangs in tense silence.");
                    yield break;
                }

                ChatResponse response = JsonUtility.FromJson<ChatResponse>(req.downloadHandler.text);
                string narration = response.choices[0].message.content.Trim();

                onComplete.Invoke(narration);
            }
        }

    

        IEnumerator CallTTS(string text, System.Action<AudioClip> onComplete)
        {
            string url = "https://api.openai.com/v1/audio/speech";

            string json = JsonUtility.ToJson(new TTSRequest
            {
                model = "gpt-4o-mini-tts",
                voice = "alloy",
                input = text,
                format = "wav"
            });

            using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.WAV);

                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Authorization", "Bearer " + OpenAIConfig.ApiKey);

                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("TTS error: " + req.error);
                    onComplete.Invoke(null);
                    yield break;
                }

                AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
                onComplete.Invoke(clip);
            }
        }


    string MockNarration()
    {
        string[] lines =
        {
            "Player One is closing in on a critical zone.",
            "Zone control is slipping away!",
            "This battlefield is still wide open.",
            "A bold move is unfolding right now."
        };

        return lines[Random.Range(0, lines.Length)];
    }
}


public enum NarrationEventType
{
    ZoneCaptureStarted,
    ZoneCaptureProgress,
    ZoneCaptured,
    ZoneLost,
    ZoneIdle
}


[System.Serializable]
public struct NarrationEvent
{
    public NarrationEventType type;
    public int playerId;
    public int zoneId;
    public float value; // progress / urgency (0–1)
}


public static class OpenAIConfig
{
    // 🔴 Replace with your key for the hackathon
    public static string ApiKey = "sk-xxxxxxxxxxxxxxxx";
}

[System.Serializable]
public class ChatRequest
{
    public string model;
    public ChatMessage[] messages;
    public int max_tokens;
    public float temperature;
}

[System.Serializable]
public class ChatMessage
{
    public string role;
    public string content;
}

[System.Serializable]
public class ChatResponse
{
    public Choice[] choices;
}

[System.Serializable]
public class Choice
{
    public ChatMessage message;
}


[System.Serializable]
public class TTSRequest
{
    public string model;
    public string voice;
    public string input;
    public string format;
}
