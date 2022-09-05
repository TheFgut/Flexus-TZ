using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public Text moneyDisplay;
    public Text comboDisplay;

    public static Score scr;
    internal int score;
    internal int combo;

    public float comboDuration;

    public int moneyCalculationSpeed;
    int moneyToCalculate;
    // Start is called before the first frame update
    void Awake()
    {
        scr = this;
        moneyDisplay.text = "Money: " + score;
        comboDisplay.enabled = false;
    }

    Coroutine moneyCalculateRoutine = null;
    public void IncreaseScore(int count)
    {
        moneyToCalculate += count;
        if (moneyCalculateRoutine == null)
        {
            moneyCalculateRoutine = StartCoroutine(moneyCalculateAnim());
        }
        IncreaseCombo();
    }
    IEnumerator moneyCalculateAnim()
    {
        do
        {
            yield return new WaitForEndOfFrame();//don't remove it from start
            int devidedMoney = moneyCalculationSpeed;
            moneyToCalculate -= devidedMoney;
            if (moneyToCalculate < 0)
            {
                devidedMoney += moneyToCalculate;
                moneyToCalculate = 0;
            }
            score += devidedMoney;
            moneyDisplay.text = "Money: " + score;

        } while (moneyToCalculate > 0);
        moneyCalculateRoutine = null;
    }
    Coroutine comboRoutine = null;
    public void IncreaseCombo()
    {
        if (comboRoutine != null)
        {
            StopCoroutine(comboRoutine);
        }
        combo++;
        updateComboDisplay(combo);

        comboRoutine = StartCoroutine(comboTimer(comboDuration));
    }

    public void updateComboDisplay(int count)
    {
        comboDisplay.text = "x" + count;
        comboDisplay.color = Color.Lerp(Color.white, Color.green, count / 10f);
    }
    IEnumerator comboTimer(float EndTime)
    {
        comboDisplay.enabled = true;
        float timer = EndTime;
        Vector3 increasedScale = new Vector3(2, 2, 2);
        Vector3 defaultScale = new Vector3(1, 1, 1);

        Color col = comboDisplay.color;
        col.a = 1;
        comboDisplay.color = col;
        do
        {
            timer -= Time.deltaTime;
            comboDisplay.transform.localScale = Vector3.Lerp(defaultScale, increasedScale, timer/ EndTime);
            yield return new WaitForEndOfFrame();
        } while (timer > 0);
        combo = 0;
        updateComboDisplay(combo);
        col = comboDisplay.color;
        do
        {
            col.a -= Time.deltaTime;
            comboDisplay.color = col;
            yield return new WaitForEndOfFrame();
        } while (col.a > 0);
        comboDisplay.enabled = false;

        comboRoutine = null;
    }
}
