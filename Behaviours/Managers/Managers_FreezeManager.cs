using MonoSandbox;
using MonoSandbox.Behaviours;
using MonoSandbox.Behaviours.Managers;
using MonoSandbox.Networking;
using UnityEngine;

public class FreezeManager : MonoBehaviour
{
    bool primaryDown;
    bool canPlace;
    public bool editMode = false;

    GameObject Cursor = null;
    GameObject itemsFolder = null;

    void Start()
    {
        itemsFolder = gameObject;
    }

    void Update()
    {
        RaycastHit hitInfo = RefCache.Hit;
        if (Cursor != null)
        {
            bool isAllowed = hitInfo.transform.gameObject.name.Contains("MonoObject") && hitInfo.collider != null && hitInfo.collider.attachedRigidbody != null;
            Cursor.GetComponent<Renderer>().material.color = isAllowed ? ThemeManager.FreezeManagerColor1 : ThemeManager.FreezeManagerColor2;

            Cursor.transform.position = hitInfo.point;

            primaryDown = InputHandling.RightPrimary;
            if (primaryDown)
            {
                if (canPlace && isAllowed)
                {
                    Rigidbody freezeRB = hitInfo.collider.attachedRigidbody;
                    freezeRB.constraints = freezeRB.constraints == RigidbodyConstraints.None ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
                    
                    if (SandboxObjectRegistry.TryGetId(hitInfo.transform, out string netId))
                    {
                        SandboxNetworking.BroadcastToggle(netId, "freeze");
                    }

                    HapticManager.Haptic(HapticManager.HapticType.Create);
                    canPlace = false;
                }
            }
            else
            {
                canPlace = true;
            }
        }
        else
        {
            if (editMode)
            {
                Cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Cursor.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                Cursor.GetComponent<Renderer>().material = new Material(RefCache.Selection);

                Destroy(Cursor.GetComponent<SphereCollider>());
            }
            else if (Cursor)
            {
                Destroy(Cursor.gameObject);
            }
        }
        if (!editMode && Cursor)
        {
            Destroy(Cursor.gameObject);
        }
    }
}
public class GravityManager : MonoBehaviour
{
    bool primaryDown;
    bool canPlace;
    public bool editMode = false;

    GameObject Cursor = null;

    void Update()
    {
        RaycastHit hitInfo = RefCache.Hit;

        if (Cursor != null)
        {
            bool isAllowed = hitInfo.transform.gameObject.name.Contains("MonoObject") && hitInfo.collider != null && hitInfo.collider.attachedRigidbody != null;
            Cursor.GetComponent<Renderer>().material.color = isAllowed ? ThemeManager.FreezeManagerColor1 : ThemeManager.FreezeManagerColor2;

            Cursor.transform.position = hitInfo.point;

            primaryDown = InputHandling.RightPrimary;
            if (primaryDown)
            {
                if (canPlace && hitInfo.transform.gameObject.name.Contains("MonoObject") && hitInfo.collider != null && hitInfo.collider.attachedRigidbody != null)
                {
                    Rigidbody gravityRB = hitInfo.collider.attachedRigidbody;
                    gravityRB.useGravity = !gravityRB.useGravity;

                    if (SandboxObjectRegistry.TryGetId(hitInfo.transform, out string netId))
                    {
                        SandboxNetworking.BroadcastToggle(netId, "gravity");
                    }

                    HapticManager.Haptic(HapticManager.HapticType.Create);
                    canPlace = false;
                }
            }
            else
            {
                canPlace = true;
            }
        }
        else
        {
            if (editMode)
            {
                Cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Cursor.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                Cursor.GetComponent<Renderer>().material = new Material(RefCache.Selection);

                Destroy(Cursor.GetComponent<SphereCollider>());
            }
            else if (Cursor)
            {
                Destroy(Cursor.gameObject);
            }
        }
        if (!editMode && Cursor)
        {
            Destroy(Cursor.gameObject);
        }
    }
}
