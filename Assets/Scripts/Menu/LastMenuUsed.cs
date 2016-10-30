using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LastMenuUsed : MonoBehaviour
{
    public static string LastMenuUsedName;


    public void ReturnToLastMenuUsed()
    {
        print("Return to last menu used");
        EventManager.TriggerEvent(StandardEventName.ReturnToMenu);
        SceneManager.LoadScene(LastMenuUsedName);
    }


    public void ReturnToMainMenu()
    {
        print("Return to main menu");
        EventManager.TriggerEvent(StandardEventName.ReturnToMenu);
        SceneManager.LoadScene(0);
    }


    public void RestartMission()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
