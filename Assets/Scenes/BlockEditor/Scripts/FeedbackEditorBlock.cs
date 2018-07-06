using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedbackEditorBlock : MonoBehaviour {

    public float FadeInSpeed;
    public float FadeOutSpeed;

    public Color MouseOverColour;
    public Color SelectedColour;

    private enum State { HIDDEN, OVER, SELECTED }
    private State currentState = State.HIDDEN;
    private State animationState = State.HIDDEN;

    private Outline outline;
    private IEnumerator currentAnimation;

    private void Start() {
        outline = GetComponent<Outline>();
    }

    private void Update() {
        if (outline == null) {
            return;
        }

        GameObject overBlock = GetBlockUnderMouse();
        GameObject selectedBlock = SelectionManager.Selection;
        if (gameObject == overBlock) {
            FadeForState(State.OVER, MouseOverColour, FadeInSpeed);
        } else if (selectedBlock == gameObject) { 
            FadeForState(State.SELECTED, SelectedColour, FadeInSpeed);
        } else {
            Color colour = outline.OutlineColor;
            colour.a = 0;
            FadeForState(State.HIDDEN, colour, FadeOutSpeed);
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
        if (Tags.EDITOR_BLOCK.HasTag(gameObject)) {
            return gameObject;
        }

        return null;
    }

    private void FadeForState(State targetState, Color targetColour, float fadeSpeed) {
        if (currentState == targetState && animationState == targetState) {
            // Animation has completed
            return;
        }

        if (animationState == targetState) {
            // Animation is in progress
            return;
        }

        if (currentAnimation != null) {
            StopCoroutine(currentAnimation);
        }
        currentAnimation = Fade(targetColour, fadeSpeed, targetState);
        StartCoroutine(currentAnimation);
    }

    private IEnumerator Fade(Color targetColour, float fadeSpeed, State targetState) {
        animationState = targetState;
        Color initialColour = outline.OutlineColor;
        if (initialColour.a == 0 && targetColour.a != 0) {
            initialColour = targetColour;
            initialColour.a = 0;
        } else if (initialColour.a == 0 && targetColour.a == 0) {
            // We are already hidden
            outline.enabled = false;

            currentState = targetState;
            yield break;
        }

        outline.OutlineColor = initialColour;
        outline.enabled = true;

        Color currentColour;
        float t = 0;
        while (t < fadeSpeed) {
            float p = t / fadeSpeed;
            currentColour = Color.Lerp(initialColour, targetColour, p);
            outline.OutlineColor = currentColour;
            yield return null;

            t += Time.deltaTime;
        }

        outline.OutlineColor = targetColour;
        if (targetColour.a == 0) {
            outline.enabled = false;
        }

        currentState = targetState;
    }
}
