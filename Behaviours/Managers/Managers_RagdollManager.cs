using GorillaTag;
using Menu;
using MonoSandbox;
using MonoSandbox.Behaviours;
using MonoSandbox.Networking;
using UnityEngine;

public class RagdollManager : PlacementHandling
{
    public bool UseGorilla;
    public GameObject Gorilla, Body;

    public void Start()
    {
        Offset = 4.5f;
    }

    public void Awake()
    {
        SandboxSpawnRegistry.Register("Ragdoll", (point, normal, args) => SpawnRagdollObject(point, (bool)args[0]));
    }

    public override GameObject CursorRef
    {
        get
        {
            GameObject cursor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cursor.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
            Destroy(cursor.GetComponent<Collider>());
            return cursor;
        }
    }

    public override void DrawCursor(RaycastHit hitInfo)
    {
        base.DrawCursor(hitInfo);

        Cursor.transform.position = hitInfo.point + Vector3.up * 0.15f;
    }

    public override void Activated(RaycastHit hitInfo)
    {
        base.Activated(hitInfo);

        GameObject obj = SpawnRagdollObject(hitInfo.point, UseGorilla);
        SandboxNetworking.BroadcastSpawn(obj, "Ragdoll", hitInfo.point, hitInfo.normal, new object[] { UseGorilla });
    }
    
    public GameObject SpawnRagdollObject(Vector3 point, bool useGorilla)
    {
        GameObject ragdollPrefab = useGorilla ? Gorilla : Body;
        GameObject Ragdoll = Instantiate(ragdollPrefab);
        Ragdoll.name += "MonoObject_Ragdoll";
        Ragdoll.transform.SetParent(RefCache.SandboxContainer.transform, false);
        
        if (useGorilla)
        {
            foreach (Transform g in Ragdoll.transform.GetChild(1).GetComponentsInChildren<Transform>())
            {
                g.gameObject.layer = 8;
                g.name = string.Concat(g.name, "MonoObject");

                // Add Rigidbody to each part
                if (g.GetComponent<Rigidbody>() == null)
                {
                    Rigidbody rb = g.gameObject.AddComponent<Rigidbody>();
                    rb.mass = 1f;
                    rb.drag = 0.5f;
                    rb.angularDrag = 0.5f;
                }

                // Keep existing mesh colliders but ensure they're properly configured
                MeshCollider mc = g.GetComponent<MeshCollider>();
                if (mc != null)
                {
                    mc.convex = true;
                }
            }

            Ragdoll.transform.position = point + new Vector3(0f, 0.45f, 0f);
        }
        else
        {
            // For body ragdoll - keep mesh colliders
            foreach (Transform g in Ragdoll.transform.GetChild(0).GetComponentsInChildren<Transform>())
            {
                g.gameObject.layer = 8;
                g.name = string.Concat(g.name, "MonoObject");

                // Add Rigidbody to each part
                if (g.GetComponent<Rigidbody>() == null)
                {
                    Rigidbody rb = g.gameObject.AddComponent<Rigidbody>();
                    rb.mass = 1f;
                    rb.drag = 0.5f;
                    rb.angularDrag = 0.5f;
                }
            }

            Ragdoll.transform.position = point + new Vector3(0f, 0.6f, 0f);
            Ragdoll.transform.localScale = new Vector3(0.4f, 0.4f, 0.5f);
            
            MeshCollider meshCollider = Ragdoll.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                meshCollider.convex = true;
            }

            Ragdoll.transform.GetChild(1).GetComponent<Renderer>().material.color = Color.grey;
        }
        
        if (Ragdoll.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = Ragdoll.AddComponent<Rigidbody>();
            rb.mass = 5f;
            rb.drag = 0.5f;
            rb.angularDrag = 0.5f;
        }

        return Ragdoll;
    }
}
