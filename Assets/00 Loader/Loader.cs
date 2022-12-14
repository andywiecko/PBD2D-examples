using andywiecko.PBD2D.Systems;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace andywiecko.PBD2D.Examples
{
    public class Loader : MonoBehaviour
    {
        [SerializeField]
        private string[] names = { };

        [SerializeField]
        private Dropdown scenesDropdown = default;

        [SerializeField]
        private Button reloadButton = default;

        private int sceneId;

        private void OnValidate()
        {
#if UNITY_EDITOR
            var scenes = UnityEditor.EditorBuildSettings.scenes;
            names = UnityEditor.EditorBuildSettingsScene
                .GetActiveSceneList(scenes)
                .Select(i => i.Split('/')[^1][..^6])
                .ToArray();

            scenesDropdown.ClearOptions();
            scenesDropdown.AddOptions(names[1..].ToList());
#endif
        }

        private void Start()
        {
            if (names is { Length: <= 1 })
            {
                return;
            }

            Application.targetFrameRate = 60;

            scenesDropdown.ClearOptions();
            scenesDropdown.AddOptions(names[1..].ToList());

            SceneManager.LoadScene(sceneId = 1, LoadSceneMode.Additive);

            scenesDropdown.onValueChanged
                .AddListener(new(i => StartCoroutine(nameof(ChangeScene), i + 1)));
            reloadButton.onClick
                .AddListener(new(() => StartCoroutine(nameof(ChangeScene), sceneId)));

            // HACK: required for build, unity strips the unused namespaces
            _ = new BoundsSystem();
        }

        private IEnumerator ChangeScene(int i)
        {
            yield return SceneManager.UnloadSceneAsync(sceneId);
            sceneId = i;
            yield return SceneManager.LoadSceneAsync(sceneId, LoadSceneMode.Additive);
        }
    }
}