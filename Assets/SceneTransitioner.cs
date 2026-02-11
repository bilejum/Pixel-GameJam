using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitioner : MonoBehaviour
{
    public static SceneTransitioner Instance;

    public CanvasGroup canvasGroup;
    public float transitionTime = 1f;

    private void Awake()
    {
        // 单例模式，确保跨场景存在
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 外部调用的接口
    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(LoadLevel(sceneName));
    }

    IEnumerator LoadLevel(string sceneName)
    {
        // 1. 开始转场：淡入黑色
        yield return Fade(1);

        // 2. 真正加载场景
        // 使用异步加载可以防止画面卡死
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone)
        {
            yield return null;
        }

        // 3. 结束转场：淡出黑色
        yield return Fade(0);
    }

    IEnumerator Fade(float targetAlpha)
    {
        float speed = Mathf.Abs(canvasGroup.alpha - targetAlpha) / transitionTime;
        while (!Mathf.Approximately(canvasGroup.alpha, targetAlpha))
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, speed * Time.unscaledDeltaTime);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
    }
}