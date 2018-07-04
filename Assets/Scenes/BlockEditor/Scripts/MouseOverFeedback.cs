using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseOverFeedback : MonoBehaviour {

    public float FadeInSpeed;
    public float FadeOutSpeed;
    public float SpinSpeed;

    private Highlight currentHighlight;

	// Use this for initialization
	void Start () {
	}

    // Update is called once per frame
    void Update() {
        GameObject gameObject = GetGameObjectUnderMouse();
        if (gameObject == null) {
            Reset(false);
            return;
        }

        if (currentHighlight == null || currentHighlight.Block != gameObject) {
            if (currentHighlight != null) {
                ResetHighlight(currentHighlight, false);
            }

            currentHighlight = new Highlight(gameObject);
            StartHighlight(currentHighlight);

            return;
        }
    }

    private void OnDisable() {
        Reset(true);
    }

    public GameObject GetGameObjectUnderMouse() {
        Camera camera = CameraExtensions.FindCameraUnderMouse();
        if (camera == null) {
            return null;
        }

        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (!Physics.Raycast(ray, out hitInfo)) {
            return null;
        }

        GameObject gameObject = hitInfo.collider.gameObject.GetRoot();
        if (!Tags.EDITOR_BLOCK.HasTag(gameObject) && !Tags.PALETTE_BLOCK.HasTag(gameObject)) {
            return null;
        }

        return gameObject;
    }

    public void Reset(bool immediate) {
        if (currentHighlight != null) {
            ResetHighlight(currentHighlight, immediate);
            currentHighlight = null;
        }
    }

    void ResetHighlight(Highlight highlight, bool immediate) {
        if (highlight.Outline == null) {
            return;
        }

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
        if (highlight.Rotator) {
            highlight.Rotator.enabled = true;
        }
    }

    void StartHighlight(Highlight highlight) {
        if (highlight.Outline == null) {
            return;
        }

        highlight.Outline.enabled = true;
        highlight.Fade = FadeIn(highlight.Outline);
        StartCoroutine(highlight.Fade);

        if (highlight.Rotator) {
            highlight.Rotator.enabled = false;
            highlight.Spin = Spin(highlight);
            StartCoroutine(highlight.Spin);
        }
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

    IEnumerator Spin(Highlight highlight) {
        Quaternion target = highlight.Rotator.InitialRotation;
        Quaternion current = highlight.Block.transform.localRotation;

        float t = 0, p = 0;
        while (t < SpinSpeed) {
            p = t / SpinSpeed;
            highlight.Block.transform.localRotation = Quaternion.Slerp(current, target, p);

            t += Time.deltaTime;
            yield return null;
        }

        highlight.Block.transform.localRotation = target;
    }

    private class Highlight {
        public readonly GameObject Block;
        public readonly Outline Outline;
        public readonly SimpleRotator Rotator;

        public IEnumerator Fade;
        public IEnumerator Spin;

        public Highlight(GameObject block) {
            this.Block = block;
            this.Outline = block.GetComponentInParent<Outline>();
            this.Rotator = block.GetComponent<SimpleRotator>();

            this.Fade = null;
            this.Spin = null;
        }
    }
}
