using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedbackPaletteBlock : MonoBehaviour {

    public float FadeInSpeed;
    public float FadeOutSpeed;
    public float SpinSpeed;

    public Color MouseOverColour;

    private Quaternion initialRotation;

    private enum State { HIDDEN, SHOWING, SHOWN, HIDING }
    private State state = State.HIDDEN;

    private IEnumerator currentAnimation;
    
    private void Start() {
        initialRotation = gameObject.transform.rotation;
    }

    private void Update() {
        GameObject currentBlock = GetBlockUnderMouse();
        if (gameObject != currentBlock) {
            switch (state) {
            case State.SHOWING:
            case State.SHOWN:
                if (currentAnimation != null) {
                    StopCoroutine(currentAnimation);
                }
                currentAnimation = MouseOut();
                StartCoroutine(currentAnimation);
                break;

            default:
                // Ignore
                break;
            }
        } else {
            switch (state) {
            case State.HIDING:
            case State.HIDDEN:
                if (currentAnimation != null) {
                    StopCoroutine(currentAnimation);
                }
                currentAnimation = MouseOver();
                StartCoroutine(currentAnimation);
                break;

            default:
                // Ignore
                break;
            }
        }
    }

    private GameObject GetBlockUnderMouse() {
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
        if (Tags.PALETTE_BLOCK.HasTag(gameObject)) {
            return gameObject;
        }

        return null;
    }

    private IEnumerator MouseOut() {
        state = State.HIDING;

        var rotator = GetComponent<SimpleRotator>();
        rotator.enabled = true;

        var fadeOut = FadeOut();
        while (fadeOut.MoveNext()) {
            yield return null;
        }

        state = State.HIDDEN;
    }

    private IEnumerator MouseOver() {
        state = State.SHOWING;

        var rotator = GetComponent<SimpleRotator>();
        rotator.enabled = false;

        var fadeIn = FadeIn();
        var spin = Spin();

        var loop = true;
        while (loop) {
            loop = fadeIn.MoveNext();
            loop |= spin.MoveNext();

            yield return null;
        }

        state = State.SHOWN;
    }

    private IEnumerator Spin() {
        Quaternion target = initialRotation;
        Quaternion current = gameObject.transform.localRotation;

        float t = 0, p = 0;
        while (t < SpinSpeed) {
            p = t / SpinSpeed;
            gameObject.transform.localRotation = Quaternion.Slerp(current, target, p);

            t += Time.deltaTime;
            yield return null;
        }

        gameObject.transform.localRotation = target;
    }

    private IEnumerator FadeIn() {
        Outline outline = GetComponent<Outline>();
        Color initialColour = outline.OutlineColor;
        if (initialColour.a == 0) {
            initialColour = MouseOverColour;
            initialColour.a = 0;
        }
        outline.OutlineColor = initialColour;
        outline.enabled = true;

        Color currentColour;
        Color targetColour = MouseOverColour;
        float t = 0;
        while (t < FadeInSpeed) {
            float p = t / FadeInSpeed;
            currentColour = Color.Lerp(initialColour, targetColour, p);
            outline.OutlineColor = currentColour;
            yield return null;

            t += Time.deltaTime;
        }

        outline.OutlineColor = targetColour;
    }

    private IEnumerator FadeOut() {
        Outline outline = GetComponent<Outline>();
        Color initialColour = outline.OutlineColor;
        if (!outline.enabled) {
            // No point fading out if we're not enabled, just ensure the alpha is reset
            initialColour.a = 0;
            outline.OutlineColor = initialColour;

            yield break;
        }

        Color currentColour;
        Color targetColour = initialColour;
        targetColour.a = 0;
        float t = 0;
        while (t < FadeOutSpeed) {
            float p = t / FadeInSpeed;
            currentColour = Color.Lerp(initialColour, targetColour, p);
            outline.OutlineColor = currentColour;
            yield return null;

            t += Time.deltaTime;
        }

        outline.OutlineColor = targetColour;
        outline.enabled = false;
    }
}
