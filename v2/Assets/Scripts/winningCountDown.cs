using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class winningCountDown : MonoBehaviour
{
    public int winningConditionSecond; 
    public GameObject hotZoneTrigger;
    public GameObject handCollider;
    public Text winningCountDownDisplay;

    private bool allowWin;
    private int countDownSecond;

    private int hotZoneTriggerCount;
    private int handColliderCount;
    private Coroutine runningCountDown;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("winningCountDown: operating");
        StartCoroutine(starting());
        runningCountDown = null;
        countDownSecond = winningConditionSecond;

        // winningCountDownDisplay = GameObject.Find("Winning Counter Display").GetComponent<Text>();
        Debug.Log("winningCountDown: winningCountDownDisplay = " + winningCountDownDisplay.gameObject.name);
        winningCountDownDisplay.gameObject.SetActive(false);

        // hotZoneTrigger = GameObject.Find("HotZoneCubeTrigger");
        Debug.Log("winningCountDown: hotZoneTrigger = " + hotZoneTrigger.name);
        // handCollider = GameObject.FindGameObjectsWithTag("Player")[0];
        Debug.Log("winningCountDown: handCollider = " + handCollider.name);
    }

    // Update is called once per frame
    void Update()
    {
        hotZoneTriggerCount = hotZoneTrigger.GetComponent<triggerHotZone>().getTouchedCount();
        // Debug.Log("winningCountDown: hotZoneTriggerCount = " + hotZoneTriggerCount.ToString());
        handColliderCount = handCollider.GetComponent<HandCollider>().getCollidingsCount();
        // Debug.Log("winningCountDown: handColliderCount = " + handColliderCount.ToString());
        
        Debug.Log("winningCountDown: allowWin = " + allowWin.ToString() + ", hotZoneTriggerCount = " + hotZoneTriggerCount.ToString() + ", handColliderCount = " + handColliderCount.ToString() + ", runningCountDown = " + ((runningCountDown != null) ? runningCountDown.ToString() : "null"));
        if (allowWin && hotZoneTriggerCount == 0 && handColliderCount == 0)
        {
            if (runningCountDown == null)
            {
                Debug.Log("winningCountDown: startCoroutine");
                winningCountDownDisplay.gameObject.SetActive(true);
                runningCountDown = StartCoroutine(countDown());
            }
        }
        else
        {
            if (runningCountDown != null)
            {
                Debug.Log("winningCountDown: endCoroutine");
                winningCountDownDisplay.gameObject.SetActive(false);
                StopCoroutine(runningCountDown);
                runningCountDown = null;
            }
        }
    }

    IEnumerator starting()
    {
        Debug.Log("winningCountDown: IEnumerator starting()");
        yield return new WaitForSeconds(1f);
        allowWin = true;
        Debug.Log("winningCountDown: allowWin = " + allowWin.ToString());
        Debug.Log("winningCountDown: IEnumerator starting() finished");
    }

    IEnumerator countDown()
    {
        Debug.Log("winningCountDown: IEnumerator countDown()"); 
        while (countDownSecond > 0)
        {
            winningCountDownDisplay.text = countDownSecond.ToString();
            yield return new WaitForSeconds(1f);
            countDownSecond--;
        }
        winningCountDownDisplay.text = "YOU WIN!";
    }
}
