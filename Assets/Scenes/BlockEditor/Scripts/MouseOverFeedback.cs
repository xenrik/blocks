using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseOverFeedback : MonoBehaviour {

    public float fadeInSpeed;
    public float fadeOutSpeed;
    public float spinSpeed;

    private new Camera camera;

    private Highlight currentHighlight;

	// Use this for initialization
	void Start () {
        camera = GetComponent<Camera>();
	}

    // Update is called once per frame
    void Update() {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo)) {
            if (Tags.PALETTE_BLOCK.HasTag(hitInfo.collider)) {
                if (currentHighlight == null || currentHighlight.block != hitInfo.collider.gameObject) {
                    if (currentHighlight != null) {
                        ResetHighlight(currentHighlight, false);
                    }

                    currentHighlight = new Highlight(hitInfo.collider.gameObject);
                    StartHighlight(currentHighlight);
                    return;
                } else {
                    // We're already highlighting this block
                    return;
                }
            }
        }

        // If we get here we were not over a suitable game object
        Reset(false);
    }

    public void Reset(bool immediate) {
        if (currentHighlight != null) {
            ResetHighlight(currentHighlight, immediate);
            currentHighlight = null;
        }
    }

    void ResetHighlight(Highlight highlight, bool immediate) {
        if (highlight.fade != null) {
            StopCoroutine(highlight.fade);
            highlight.fade = null;
        }
        if (highlight.spin != null) {
            StopCoroutine(highlight.spin);
            highlight.spin = null;
        }
        if (highlight.outline.enabled) {
            if (immediate) {
                highlight.outline.enabled = false;
            }

            // Fade out (or just reset)
            StartCoroutine(FadeOut(highlight.outline));
        }

        highlight.rotator.enabled = true;
    }

    void StartHighlight(Highlight highlight) {
        highlight.outline.enabled = true;
        highlight.rotator.enabled = false;

        highlight.fade = FadeIn(highlight.outline);
        StartCoroutine(highlight.fade);

        highlight.spin = Spin(highlight.block);
        StartCoroutine(highlight.spin);
    }

    IEnumerator FadeIn(Outline outline) {
        Color c = outline.OutlineColor;
        outline.enabled = true;

        while (c.a < 1) {
            c.a += (1 / fadeInSpeed) * Time.deltaTime;
            outline.OutlineColor = c;
            yield return null;
        }

        c.a = 1;
        outline.OutlineColor = c;
    }

    IEnumerator FadeOut(Outline outline) {
        Color c = outline.OutlineColor;

        if (!outline.enabled) {
            // No point fading out if we're not enabled, just ensure the alpha is reset
            c.a = 0;
            outline.OutlineColor = c;

            yield break;
        }

        while (c.a > 0) {
            c.a -= (1 / fadeOutSpeed) * Time.deltaTime;
            outline.OutlineColor = c;
            yield return null;
        }

        c.a = 0;
        outline.OutlineColor = c;
        outline.enabled = false;
    }

    IEnumerator Spin(GameObject block) {
        Quaternion target = Quaternion.Euler(Vector3.forward);
        Quaternion current = block.transform.localRotation;

        float t = 0, p = 0;
        while (t < spinSpeed) {
            p = t / spinSpeed;
            block.transform.localRotation = Quaternion.Slerp(current, target, p);

            t += Time.deltaTime;
            yield return null;
        }

        block.transform.localRotation = target;
    }

    private class Highlight {
        public readonly GameObject block;
        public readonly Outline outline;
        public readonly SimpleRotator rotator;

        public IEnumerator fade;
        public IEnumerator spin;

        public Highlight(GameObject block) {
            this.block = block;
            this.outline = block.GetComponent<Outline>();
            this.rotator = block.GetComponent<SimpleRotator>();

            this.fade = null;
            this.spin = null;
        }
    }
}
