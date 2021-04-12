using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Bolt;

public class WinningCountDownBolt : GlobalEventListener
{
    public int winningConditionSecond; 
    public Text winningCountDownDisplay;

    private int countDownSecond;

    private Coroutine runningCountDown;

    // Start is called before the first frame update
    void Start()
    {
        runningCountDown = null;
        countDownSecond = winningConditionSecond;
        winningCountDownDisplay.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public override void OnEvent(StartWinningCountDownEvent evnt)
    {
        countDownSecond = winningConditionSecond;
        winningCountDownDisplay.gameObject.SetActive(true);
        runningCountDown = StartCoroutine(countDown());
    }

    public override void OnEvent(StopWinningCountDownEvent evnt)
    {
        winningCountDownDisplay.gameObject.SetActive(false);
        StopCoroutine(runningCountDown);
        runningCountDown = null;
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
