using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseOverFeedback : MonoBehaviour {

    public float fadeInSpeed;
    public float fadeOutSpeed;

    private new Camera camera;

    private Outline lastOutline;
    private IEnumerator currentFadeIn;

	// Use this for initialization
	void Start () {
        camera = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo)) {
            Outline outline = hitInfo.collider.GetComponent<Outline>();
            if (outline != lastOutline) {
                if (currentFadeIn != null) {
                    StopCoroutine(currentFadeIn);
                    currentFadeIn = null;
                }
                if (lastOutline != null) {
                    StartCoroutine(FadeOut(lastOutline));
                }

                currentFadeIn = FadeIn(outline);
                StartCoroutine(currentFadeIn);
                lastOutline = outline;
            }
        } else if (lastOutline != null) {
            if (currentFadeIn != null) {
                StopCoroutine(currentFadeIn);
                currentFadeIn = null;
            }

            StartCoroutine(FadeOut(lastOutline));
            lastOutline = null;
        }
	}

    IEnumerator FadeIn(Outline outline) {
        Color c = outline.OutlineColor;
        if (!outline.enabled) {
            // Ensure we're at a valid alpha if we're starting to fade in
            c.a = 0;
        }
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
        if (!outline.enabled) {
            // No point fading out if we're not enabled
            yield break;
        }

        Color c = outline.OutlineColor;
        while (c.a > 0) {
            c.a -= (1 / fadeOutSpeed) * Time.deltaTime;
            outline.OutlineColor = c;
            yield return null;
        }

        c.a = 0;
        outline.OutlineColor = c;
        outline.enabled = false;
    }
}
