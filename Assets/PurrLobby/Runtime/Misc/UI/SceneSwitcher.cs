using PurrNet;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PurrLobby
{
    public class SceneSwitcher : MonoBehaviour
    {
        [SerializeField] private LobbyManager lobbyManager;
        [PurrScene, SerializeField] private string nextScene;
        private bool isLoadNewSceneCalled;
        public void SwitchScene()
        {
            lobbyManager.SetLobbyStarted();
            SceneManager.LoadSceneAsync(nextScene);
        }

        //MS version
        public CanvasGroup fadeCanvas; // Assign in Inspector
        public float fadeDuration = 1f; // Control fade speed


        public void LoadNewScene_Ms()
        {
            if(!isLoadNewSceneCalled)
            {
                isLoadNewSceneCalled = true;
                lobbyManager.SetLobbyStarted();
                StartCoroutine(FadeAndLoadScene(nextScene));
            }
        }

        private IEnumerator FadeAndLoadScene(string sceneName)
        {
            // Fade to black
            yield return StartCoroutine(FadeOut());

            // Start loading the new scene
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            // Wait until the scene is fully loaded
            while (operation.progress < 0.9f)
            {
                yield return null;
            }

            // Activate the new scene
            operation.allowSceneActivation = true;

            // Wait a frame for the new scene to be fully set up
            yield return null;
        }

        private IEnumerator FadeOut()
        {
            Debug.Log("call fade out");
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                fadeCanvas.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
                yield return null;
            }
            fadeCanvas.alpha = 1; // Ensure fully black
        }
    }
}
