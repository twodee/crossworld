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
    loot.text = label;
  }

  override public void Update() {
    base.Update();
  }

  override public bool IsTransmittable() {
    return loot.text != "";
  }

  override public IEnumerator Transmit() {
    GameObject payload = Instantiate(loot.gameObject);
    payload.transform.SetParent(ampersand.Target.gameObject.transform.Find("canvas"));
    payload.transform.position = ampersand.gameObject.transform.position;

    Vector2 startPosition = ampersand.gameObject.transform.position;
    Vector2 endPosition = ampersand.Target.gameObject.transform.position;

    float startTime = Time.time;
    float targetTime = 1.0f;
    float elapsedTime = 0.0f;

    while (elapsedTime <= targetTime) {
      payload.transform.position = Vector2.Lerp(startPosition, endPosition, elapsedTime / targetTime);
      yield return null;
      elapsedTime = Time.time - startTime;
    }

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
