using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ScreenshotScaler : MonoBehaviour
{
    public SpriteRenderer Sprite = null;
    public Camera TargetCamera = null;
    public float TargetDepthParameter = 0.95f;

    private void Update()
    {
        if(TargetCamera != null && Sprite != null)
        {
			Ray bottomLeftRay = TargetCamera.ScreenPointToRay(new Vector3(0f, 0f));
			Vector3 minPoint = bottomLeftRay.GetPoint(TargetCamera.farClipPlane * TargetDepthParameter);
			Ray topRightRay = TargetCamera.ScreenPointToRay(new Vector3(TargetCamera.pixelWidth, TargetCamera.pixelHeight));
			Vector3 maxPoint = topRightRay.GetPoint(TargetCamera.farClipPlane * TargetDepthParameter);

            Sprite.transform.position = new Vector3((minPoint.x + maxPoint.x) / 2f, minPoint.y, minPoint.z);
            Sprite.transform.localScale = Vector3.one * ((float)(maxPoint.y - minPoint.y) / 1080f);
		}
    }
}
