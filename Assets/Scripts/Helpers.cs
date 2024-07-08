using UnityEngine;

public static class Extend_TransformHelpers
{
    public static Transform FindChildByName(this Transform transform, string name)
    {
        Transform[] transforms = transform.GetComponentsInChildren<Transform>();

        foreach(Transform t in transforms)
        {
            if (t.gameObject.name.Equals(name))
                return t;
        }

        return null;
    }
}
public static class UIHelpers
{
    public static Canvas CreateBillbordCanvas(string resourceName, Transform transform, Camera camera)
    {
        GameObject prefab = Resources.Load<GameObject>(resourceName);
        GameObject obj = GameObject.Instantiate<GameObject>(prefab, transform);

        Canvas canvas = obj.GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;

        return canvas;
    }
}
