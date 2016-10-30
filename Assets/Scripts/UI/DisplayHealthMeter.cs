using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DisplayHealthMeter : MonoBehaviour {

    private PlayerHealth playerHealth;
    private Transform canvasTransform;

    private Image healthMeter;
    private Image damageIndicator;
    private float currentFillAmount;
    private float previousFillAmount;

    public float sceneMagnitude;
    public float canvasMagnitude;
    public float duration;

    



    // Use this for initialization
    void Start () {
        playerHealth = FindObjectOfType<PlayerHealth>();

        healthMeter = GameObject.Find("Health Meter").GetComponent<Image>();
        damageIndicator = GameObject.Find("Damage Indicator").GetComponent<Image>();
        canvasTransform = GameObject.Find("Canvases").GetComponent<Transform>();
    }
	
	// Update is called once per frame
	void Update () {

        currentFillAmount = (float)playerHealth.CurrentHealth / playerHealth.StartingHealth;
        healthMeter.fillAmount = currentFillAmount;

        if (currentFillAmount <= 0)
            damageIndicator.color = new Color(Color.red.r, Color.red.g, Color.red.b, 0.587f);
        else if (currentFillAmount < previousFillAmount)
        {
            StartCoroutine(FlashDamage());
            StartCoroutine(Shake());
            //EventManager.TriggerEvent(TwoFloatsEventName.ShakeCamera, sceneMagnitude, duration);
        }

        previousFillAmount = currentFillAmount;

    }

    IEnumerator FlashDamage()
    {
        damageIndicator.color = new Color(Color.red.r, Color.red.g, Color.red.b, 0.587f);
        yield return new WaitForSeconds(0.1f);
        damageIndicator.color = new Color(Color.white.r, Color.white.g, Color.white.b, 0.587f);
    }


    IEnumerator Shake()
    {
        float elapsed = 0.0f;

        Vector3 oroginalCanvasPos = canvasTransform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float percentComplete = elapsed / duration;
            float damper = 1.0f - Mathf.Clamp(4.0f * percentComplete - 3.0f, 0.0f, 1.0f);

            // map value to [-1, 1]
            float z = Random.value * 2.0f - 1.0f;
            float x = Random.value * 2.0f - 1.0f;
            z *= damper;
            x *= damper;

            canvasTransform.position = new Vector3(z * canvasMagnitude, x * canvasMagnitude, oroginalCanvasPos.z);

            yield return null;
        }

        canvasTransform.position = oroginalCanvasPos;
    }
}
