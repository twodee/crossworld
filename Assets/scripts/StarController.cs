﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StarController : PlayerController {
  private AmpersandController ampersand;
  private Text loot;

  override public void Start() {
    base.Start();
    this.Type = "D";
    base.declaration = "char value;";
    loot = transform.Find("canvas/loot").GetComponent<Text>();
    ampersand = GameObject.Find("/players/ampersand").GetComponent<AmpersandController>();
  }

  public void Acquire(string label) {
    loot.GetComponent<Text>().color = GameController.SINGLETON.letterWithValueColor;
    loot.text = label;
  }

  override public void Update() {
    base.Update();
  }

  override public bool IsTransmittable() {
    return loot.text != "";
  }

  public IEnumerator Release() {
    if (loot.text == "") {
      yield return null;
    } else {
      Color startColor = GameController.SINGLETON.letterWithValueColor;
      Color endColor = startColor;
      endColor.a = 0.0f;

      float startTime = Time.time;
      float targetTime = 0.2f;
      float elapsedTime = 0.0f;

      while (elapsedTime <= targetTime) {
        float proportion = elapsedTime / targetTime;
        loot.GetComponent<Text>().color = Color.Lerp(startColor, endColor, proportion);
        yield return null;
        elapsedTime = Time.time - startTime;
      }

      loot.GetComponent<Text>().color = endColor;
    }
  }

  override public IEnumerator Transmit() {
    GameObject payload = Instantiate(loot.gameObject);
    Text payloadText = payload.GetComponent<Text>();
    payload.transform.SetParent(ampersand.Target.gameObject.transform.Find("canvas"));
    payload.transform.position = ampersand.gameObject.transform.position;

    Vector2 startPosition = ampersand.gameObject.transform.position;
    Vector2 endPosition = ampersand.Target.gameObject.transform.position;
    Color startColor = GameController.SINGLETON.letterWithValueColor;
    Color endColor = GameController.SINGLETON.letterInCellColor;

    float startTime = Time.time;
    float targetTime = 1.0f;
    float elapsedTime = 0.0f;

    while (elapsedTime <= targetTime) {
      float proportion = elapsedTime / targetTime;
      payload.transform.position = Vector2.Lerp(startPosition, endPosition, proportion);
      payloadText.color = Color.Lerp(startColor, endColor, proportion);
      yield return null;
      elapsedTime = Time.time - startTime;
    }

    payload.transform.position = endPosition;
    payloadText.color = endColor;
    GameController.SINGLETON.Log("*p = value; // *p = '" + loot.text + "'");

    Destroy(payload);
    ampersand.Target.Label = loot.text;
  }

  public Vector2 LootPosition {
    get {
      return loot.gameObject.transform.position;
    }
  }
}
