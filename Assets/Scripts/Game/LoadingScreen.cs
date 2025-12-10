using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    private IEnumerator Start()
    {
        // Start the initialization operation
        AsyncOperationHandle<LocalizationSettings> operation = LocalizationSettings.InitializationOperation;

        // Wait until the operation is complete (yields control back to Unity/browser)
        yield return operation;

        // Initialization is complete. The active locale and tables are ready.
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("Localization initialized successfully to locale: " + LocalizationSettings.SelectedLocale.name);
            // You can now safely access localized strings

            // Proceed to load the main scene or initialize the UI
            // SceneManager.LoadScene("MainGameScene");
            SceneTransition.Instance.LoadScene(0);
        }
        else
        {
            Debug.LogError("Localization initialization failed: " + operation.OperationException.Message);
            // Handle error (e.g., show a generic error message)
        }
    }
}