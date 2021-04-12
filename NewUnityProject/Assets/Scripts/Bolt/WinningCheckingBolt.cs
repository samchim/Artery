using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinningCheckingBolt : Bolt.EntityBehaviour<ICubeState>
{
    public GameObject hotZoneTrigger;
    public GameObject[] handColliderList;

    private bool allowWin;
    private int countDownSecond;

    private int hotZoneTriggerCount;
    private int handColliderCount;
    private bool runningCountDown = false;

    private StartWinningCountDownEvent startWinningCountDownEvent = StartWinningCountDownEvent.Create();
    private StopWinningCountDownEvent stopWinningCountDownEvent = StopWinningCountDownEvent.Create();

    private WinningCountDownBolt _winningCountDownBolt;

    void Start()
    {
        if (entity.IsOwner)
        {
            StartCoroutine(starting());
            hotZoneTrigger = GameObject.FindGameObjectWithTag("HotZoneTrigger");
        }
    }

    public override void SimulateOwner()
    {
        if (entity.IsOwner)
        {
            hotZoneTriggerCount = hotZoneTrigger.GetComponent<triggerHotZone>().getTouchedCount();
            // handColliderCount = handCollider.GetComponent<HandCollider>().getCollidingsCount();
            handColliderCount = 0;
            handColliderList = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject handCollider in handColliderList)
            {
                handColliderCount += handCollider.GetComponent<HandColliderBolt>().getCollidingsCount();
            }

            Debug.Log("winningCountDown: allowWin = " + allowWin.ToString() + ", hotZoneTriggerCount = " + hotZoneTriggerCount.ToString() + ", handColliderCount = " + handColliderCount.ToString() + ", runningCountDown = " + ((runningCountDown != null) ? runningCountDown.ToString() : "null"));
            if (allowWin && hotZoneTriggerCount == 0 && handColliderCount == 0)
            {
                if (runningCountDown == false)
                {
                    startWinningCountDownEvent.Send();
                    runningCountDown = true;
                }
            }
            else
            {
                if (runningCountDown == true)
                {
                    stopWinningCountDownEvent.Send();
                    runningCountDown = false;
                }
            }
        }
    }

    IEnumerator starting()
    {
        yield return new WaitForSeconds(1f);
        allowWin = true;
    }
}
