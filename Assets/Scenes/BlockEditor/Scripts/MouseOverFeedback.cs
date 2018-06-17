using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseOverFeedback : MonoBehaviour {

    public float FadeInSpeed;
    public float FadeOutSpeed;
    public float SpinSpeed;

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
                if (currentHighlight == null || currentHighlight.Block != hitInfo.collider.gameObject) {
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
        if (highlight.Fade != null) {
            StopCoroutine(highlight.Fade);
            highlight.Fade = null;
        }
        if (highlight.Spin != null) {
            StopCoroutine(highlight.Spin);
            highlight.Spin = null;
        }
        if (highlight.Outline.enabled) {
            if (immediate) {
                highlight.Outline.enabled = false;
            }

            // Fade out (or just reset)
            StartCoroutine(FadeOut(highlight.Outline));
        }

        highlight.Rotator.enabled = true;
    }

    void StartHighlight(Highlight highlight) {
        highlight.Outline.enabled = true;
        highlight.Rotator.enabled = false;

        highlight.Fade = FadeIn(highlight.Outline);
        StartCoroutine(highlight.Fade);

        highlight.Spin = Spin(highlight.Block);
        StartCoroutine(highlight.Spin);
    }

    IEnumerator FadeIn(Outline outline) {
        Color c = outline.OutlineColor;
        outline.enabled = true;

        while (c.a < 1) {
            c.a += (1 / FadeInSpeed) * Time.deltaTime;
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
            c.a -= (1 / FadeOutSpeed) * Time.deltaTime;
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
        while (t < SpinSpeed) {
            p = t / SpinSpeed;
            block.transform.localRotation = Quaternion.Slerp(current, target, p);

            t += Time.deltaTime;
            yield return null;
        }

        block.transform.localRotation = target;
    }

    private class Highlight {
        public readonly GameObject Block;
        public readonly Outline Outline;
        public readonly SimpleRotator Rotator;

        public IEnumerator Fade;
        public IEnumerator Spin;

        public Highlight(GameObject block) {
            this.Block = block;
            this.Outline = block.GetComponent<Outline>();
            this.Rotator = block.GetComponent<SimpleRotator>();

            this.Fade = null;
            this.Spin = null;
        }
    }
}
