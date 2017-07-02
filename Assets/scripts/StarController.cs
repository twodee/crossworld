using UnityEngine;
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
    loot.GetComponent<Text>().color = GameController.SINGLETON.transmittingFillColor;
    loot.GetComponent<Outline>().effectColor = GameController.SINGLETON.transmittingStrokeColor;
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
      Color startFillColor = GameController.SINGLETON.transmittingFillColor;
      Color endFillColor = startFillColor;
      endFillColor.a = 0.0f;
      Color startStrokeColor = GameController.SINGLETON.transmittingStrokeColor;
      Color endStrokeColor = startStrokeColor;
      endStrokeColor.a = 0.0f;

      float startTime = Time.time;
      float targetTime = 0.2f;
      float elapsedTime = 0.0f;

      while (elapsedTime <= targetTime) {
        float proportion = elapsedTime / targetTime;
        loot.GetComponent<Text>().color = Color.Lerp(startFillColor, endFillColor, proportion);
        loot.GetComponent<Outline>().effectColor = Color.Lerp(startStrokeColor, endStrokeColor, proportion);
        yield return null;
        elapsedTime = Time.time - startTime;
      }

      loot.GetComponent<Text>().color = endFillColor;
      loot.GetComponent<Outline>().effectColor = endStrokeColor;
    }
  }

  override public IEnumerator Transmit() {
    GameObject payload = Instantiate(ampersand.Target.gameObject.transform.Find("canvas/text").gameObject);
    Text payloadText = payload.GetComponent<Text>();
    payload.transform.SetParent(ampersand.Target.gameObject.transform.Find("canvas"));
    payload.transform.position = ampersand.gameObject.transform.position;

    payload.GetComponent<Outline>().enabled = true;
    payloadText.text = loot.text;
    Vector2 startPosition = ampersand.gameObject.transform.position;
    Vector2 endPosition = ampersand.Target.gameObject.transform.position;
    Color startFillColor = GameController.SINGLETON.transmittingFillColor;
    Color endFillColor = GameController.SINGLETON.letterInCellColor;
    Color startStrokeColor = GameController.SINGLETON.transmittingStrokeColor;
    Color endStrokeColor = startStrokeColor;
    endStrokeColor.a = 0;
    Vector3 startScale = loot.gameObject.GetComponent<RectTransform>().localScale;
    Vector3 endScale = ampersand.Target.gameObject.transform.Find("canvas/text").gameObject.GetComponent<RectTransform>().localScale;

    float startTime = Time.time;
    float targetTime = 1.0f;
    float elapsedTime = 0.0f;

    GameController.SINGLETON.Log("*p = value; // *p = '" + loot.text + "'");

    while (elapsedTime <= targetTime) {
      float proportion = elapsedTime / targetTime;
      payload.transform.position = Vector2.Lerp(startPosition, endPosition, proportion);
      payloadText.color = Color.Lerp(startFillColor, endFillColor, proportion);
      payload.GetComponent<Outline>().effectColor = Color.Lerp(startStrokeColor, endStrokeColor, proportion);
      payload.GetComponent<RectTransform>().localScale = Vector3.Lerp(startScale, endScale, proportion);
      yield return null;
      elapsedTime = Time.time - startTime;
    }

    payload.transform.position = endPosition;
    payloadText.color = endFillColor;
    payload.GetComponent<Outline>().effectColor = endStrokeColor;

    Destroy(payload);
    ampersand.Target.Label = loot.text;

    GameController.SINGLETON.CheckForWin();
  }

  public Text Loot {
    get {
      return loot;
    }
  }

  public Vector2 LootPosition {
    get {
      return loot.gameObject.transform.position;
    }
  }
}
