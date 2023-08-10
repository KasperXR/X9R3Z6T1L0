using Lean.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

[System.Serializable]
public class SerializedGameObject
{
    [System.Serializable]
    public class SerializedComponent
    {
        public string type;
        public string jsonData;
    }

    public string name;
    public string tag;
    public int layer;
    public bool isStatic;
    public Vector3 localPosition;
    public Quaternion localRotation;
    public Vector3 localScale;
    public bool activeSelf;
    public bool isRoot;

    public List<SerializedComponent> components = new List<SerializedComponent>();
    public List<SerializedMeshRenderer> meshRenderers = new List<SerializedMeshRenderer>();
    public List<SerializedGameObject> children = new List<SerializedGameObject>();

    public static SerializedGameObject Serialize(GameObject obj)
    {
        SerializedGameObject serializedObj = new SerializedGameObject
        {
            name = obj.name,
            tag = obj.tag,
            layer = obj.layer,
            isStatic = obj.isStatic,
            localPosition = obj.transform.localPosition,
            localRotation = obj.transform.localRotation,
            localScale = obj.transform.localScale,
            activeSelf = obj.activeSelf
        };

        foreach (Component component in obj.GetComponents<Component>())
        {
            if (component is Transform) continue;

            SerializedComponent serializedComponent = new SerializedComponent
            {
                type = component.GetType().AssemblyQualifiedName
            };

            try
            {
                if (component is MeshRenderer)
                {
                    serializedObj.meshRenderers.Add(SerializeMeshRenderer(component as MeshRenderer));
                }
                else if (component is MeshCollider)
                {
                    SerializedMeshCollider serializedMeshCollider = SerializeMeshCollider(component as MeshCollider);
                    serializedComponent.jsonData = JsonUtility.ToJson(serializedMeshCollider);
                    serializedObj.components.Add(serializedComponent);
                }
                else if (component is BoxCollider)
                {
                    SerializedBoxCollider serializedBoxCollider = SerializeBoxCollider(component as BoxCollider);
                    serializedComponent.jsonData = JsonUtility.ToJson(serializedBoxCollider);
                    serializedObj.components.Add(serializedComponent);
                }
                else if (component is Rigidbody)
                {
                    SerializedRigidbody serializedRigidbody = SerializeRigidbody(component as Rigidbody);
                    serializedComponent.jsonData = JsonUtility.ToJson(serializedRigidbody);
                    serializedObj.components.Add(serializedComponent);
                }
                else if (component is AssignModels)
                {
                    SerializedAssignModels serializedAssignModels = SerializeAssignModels(component as AssignModels);
                    serializedComponent.jsonData = JsonUtility.ToJson(serializedAssignModels);
                    serializedObj.components.Add(serializedComponent);
                }
                else if (component is GroupingObjects)
                {
                    serializedComponent.jsonData = JsonUtility.ToJson(SerializeGroupingObjects(component as GroupingObjects));
                    serializedObj.components.Add(serializedComponent);
                }
                else if (component is Outline)
                {
                    SerializedOutline serializedOutline = SerializeOutline(component as Outline);
                    serializedComponent.jsonData = JsonUtility.ToJson(serializedOutline);
                    serializedObj.components.Add(serializedComponent);
                }
            }
            catch (ArgumentException)
            {
                Debug.LogWarning($"Could not serialize component of type {component.GetType().Name} on object {obj.name}");
            }
        }

        foreach (Transform child in obj.transform)
        {
            serializedObj.children.Add(Serialize(child.gameObject));
        }

        return serializedObj;
    }

    public static GameObject Deserialize(SerializedGameObject serializedObj)
    {
        GameObject obj = new GameObject();
        obj.name = serializedObj.name;
        obj.tag = serializedObj.tag;
        obj.layer = serializedObj.layer;
        obj.isStatic = serializedObj.isStatic;
        obj.SetActive(serializedObj.activeSelf);
        obj.transform.localPosition = serializedObj.localPosition;
        obj.transform.localRotation = serializedObj.localRotation;
        obj.transform.localScale = serializedObj.localScale;

        foreach (SerializedComponent serializedComponent in serializedObj.components)
        {
            Type componentType = Type.GetType(serializedComponent.type);
            Component component = obj.AddComponent(componentType);

            try
            {
                if (component is MeshCollider)
                {
                    SerializedMeshCollider serializedMeshCollider = JsonUtility.FromJson<SerializedMeshCollider>(serializedComponent.jsonData);
                    DeserializeMeshCollider(component as MeshCollider, serializedMeshCollider);
                }
                else if (component is BoxCollider)
                {
                    SerializedBoxCollider serializedBoxCollider = JsonUtility.FromJson<SerializedBoxCollider>(serializedComponent.jsonData);
                    DeserializeBoxCollider(component as BoxCollider, serializedBoxCollider);
                }
                else if (component is Rigidbody)
                {
                    SerializedRigidbody serializedRigidbody = JsonUtility.FromJson<SerializedRigidbody>(serializedComponent.jsonData);
                    DeserializeRigidbody(component as Rigidbody, serializedRigidbody);
                }
                else if (component is AssignModels)
                {
                    SerializedAssignModels serializedAssignModels = JsonUtility.FromJson<SerializedAssignModels>(serializedComponent.jsonData);
                    DeserializeAssignModels(obj, serializedAssignModels);
                }
                else if (componentType == typeof(GroupingObjects))
                {
                    DeserializeGroupingObjects(component as GroupingObjects, JsonUtility.FromJson<SerializedGroupingObjects>(serializedComponent.jsonData));

                }
                else if (component is Outline)
                {
                    SerializedOutline serializedOutline = JsonUtility.FromJson<SerializedOutline>(serializedComponent.jsonData);
                    DeserializeOutline(component as Outline, serializedOutline);
                }

                else
                {
                    JsonUtility.FromJsonOverwrite(serializedComponent.jsonData, component);
                }

            }
            catch (ArgumentException)
            {
                Debug.LogWarning($"Could not deserialize component of type {component.GetType().Name} on object {obj.name}");
            }
        }

        foreach (SerializedMeshRenderer serializedMeshRenderer in serializedObj.meshRenderers)
        {
            DeserializeMeshRenderer(obj, serializedMeshRenderer);
        }

        foreach (SerializedGameObject child in serializedObj.children)
        {
            GameObject childObj = Deserialize(child);
            childObj.transform.SetParent(obj.transform, false);
        }

        return obj;
    }

    private static SerializedMeshRenderer SerializeMeshRenderer(MeshRenderer meshRenderer)
    {
        if (meshRenderer == null) return null;

        MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
        if (meshFilter == null) return null;

        SerializedMeshRenderer serializedMeshRenderer = new SerializedMeshRenderer
        {
            meshAssetPath = meshFilter.sharedMesh ? meshFilter.sharedMesh.name : "",
            materialGUIDs = meshRenderer.sharedMaterials.Select(x => x.name).ToArray(),
            meshName = meshFilter.sharedMesh ? meshFilter.sharedMesh.name : "",
            materialNames = meshRenderer.sharedMaterials.Select(x => x.name).ToArray()
        };

        return serializedMeshRenderer;
    }

    private static void DeserializeMeshRenderer(GameObject obj, SerializedMeshRenderer serializedMeshRenderer)
    {
        if (serializedMeshRenderer == null) return;

        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = obj.AddComponent<MeshFilter>();
        }

        MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = obj.AddComponent<MeshRenderer>();
        }

        meshFilter.sharedMesh = Resources.Load<Mesh>("Meshes/" + serializedMeshRenderer.meshName);
        meshRenderer.sharedMaterials = serializedMeshRenderer.materialNames
            .Select(x => Resources.Load<Material>("Materials/" + x))
            .ToArray();

        if (meshRenderer.sharedMaterials.Any(x => x == null))
        {
            Debug.LogWarning("Some materials could not be deserialized and are missing.");
        }
    }

    private static SerializedMeshCollider SerializeMeshCollider(MeshCollider meshCollider)
    {
        string meshAssetPath = meshCollider.sharedMesh ? meshCollider.sharedMesh.name : "";

        SerializedMeshCollider serializedMeshCollider = new SerializedMeshCollider
        {
            meshAssetPath = meshAssetPath,
            isConvex = meshCollider.convex
        };

        return serializedMeshCollider;
    }

    private static SerializedBoxCollider SerializeBoxCollider(BoxCollider boxCollider)
    {
        SerializedBoxCollider serializedBoxCollider = new SerializedBoxCollider
        {
            center = boxCollider.center,
            size = boxCollider.size,
            isEnabled = boxCollider.enabled
        };

        return serializedBoxCollider;
    }
    private static void DeserializeMeshCollider(MeshCollider meshCollider, SerializedMeshCollider serializedMeshCollider)
    {
        if (!string.IsNullOrEmpty(serializedMeshCollider.meshAssetPath))
        {
            meshCollider.sharedMesh = Resources.Load<Mesh>("Meshes/" + serializedMeshCollider.meshAssetPath);
        }

        meshCollider.convex = serializedMeshCollider.isConvex;
    }

    private static void DeserializeBoxCollider(BoxCollider boxCollider, SerializedBoxCollider serializedBoxCollider)
    {
        boxCollider.center = serializedBoxCollider.center;
        boxCollider.size = serializedBoxCollider.size;
        boxCollider.enabled = serializedBoxCollider.isEnabled;
    }
    private static SerializedRigidbody SerializeRigidbody(Rigidbody rb)
    {
        SerializedRigidbody serializedRigidbody = new SerializedRigidbody
        {
            velocity = rb.velocity,
            angularVelocity = rb.angularVelocity,
            mass = rb.mass,
            isKinematic = rb.isKinematic,
            useGravity = rb.useGravity,
            constraints = rb.constraints,
            interpolation = rb.interpolation,
            collisionDetectionMode = rb.collisionDetectionMode,
            centerOfMass = rb.centerOfMass,
            inertiaTensor = rb.inertiaTensor,
            inertiaTensorRotation = rb.inertiaTensorRotation,
            drag = rb.drag,
            angularDrag = rb.angularDrag,
            maxAngularVelocity = rb.maxAngularVelocity,
            maxDepenetrationVelocity = rb.maxDepenetrationVelocity,
            solverIterations = rb.solverIterations,
            solverVelocityIterations = rb.solverVelocityIterations
        };

        return serializedRigidbody;
    }
    private static void DeserializeRigidbody(Rigidbody rb, SerializedRigidbody serializedRigidbody)
    {
        rb.velocity = serializedRigidbody.velocity;
        rb.angularVelocity = serializedRigidbody.angularVelocity;
        rb.mass = serializedRigidbody.mass;
        rb.isKinematic = serializedRigidbody.isKinematic;
        rb.useGravity = serializedRigidbody.useGravity;
        rb.constraints = serializedRigidbody.constraints;
        rb.interpolation = serializedRigidbody.interpolation;
        rb.collisionDetectionMode = serializedRigidbody.collisionDetectionMode;
        rb.centerOfMass = serializedRigidbody.centerOfMass;
        rb.inertiaTensor = serializedRigidbody.inertiaTensor;
        rb.inertiaTensorRotation = serializedRigidbody.inertiaTensorRotation;
        rb.drag = serializedRigidbody.drag;
        rb.angularDrag = serializedRigidbody.angularDrag;
        rb.maxAngularVelocity = serializedRigidbody.maxAngularVelocity;
        rb.maxDepenetrationVelocity = serializedRigidbody.maxDepenetrationVelocity;
        rb.solverIterations = serializedRigidbody.solverIterations;
        rb.solverVelocityIterations = serializedRigidbody.solverVelocityIterations;
    }

    private static SerializedAssignModels SerializeAssignModels(AssignModels script)
    {
        SerializedAssignModels serializedScript = new SerializedAssignModels
        {
            standardModelPrefabPath = script.standardModelPrefab != null ? script.standardModelPrefab.name : "",
            windowModelPrefabPath = script.windowModelPrefab != null ? script.windowModelPrefab.name : "",
            switchWindowScriptName = script.switchWindowScript != null ? script.switchWindowScript.gameObject.name : "",
            rotatorScriptName = script.rotatorScript != null ? script.rotatorScript.gameObject.name : "",
        };

        return serializedScript;
    }

    private static void DeserializeAssignModels(GameObject obj, SerializedAssignModels serializedScript)
    {
        AssignModels script = obj.GetComponent<AssignModels>();

        script.standardModelPrefab = !string.IsNullOrEmpty(serializedScript.standardModelPrefabPath) ? Resources.Load<GameObject>(serializedScript.standardModelPrefabPath) : null;
        script.windowModelPrefab = !string.IsNullOrEmpty(serializedScript.windowModelPrefabPath) ? Resources.Load<GameObject>(serializedScript.windowModelPrefabPath) : null;
        script.switchWindowScript = GameObject.Find(serializedScript.switchWindowScriptName)?.GetComponent<SwitchWindow>();
        script.rotatorScript = GameObject.Find(serializedScript.rotatorScriptName)?.GetComponent<ObjectRotator>();
    }
    private static SerializedGroupingObjects SerializeGroupingObjects(GroupingObjects script)
    {
        SerializedGroupingObjects serializedScript = new SerializedGroupingObjects
        {
            orbitControlsScriptName = script.orbitControlsScript != null ? script.orbitControlsScript.gameObject.name : "",
            replaceOrAddScriptName = script.replaceOrAddScript != null ? script.replaceOrAddScript.gameObject.name : "",
        };

        return serializedScript;
    }
    private static void DeserializeGroupingObjects(GroupingObjects script, SerializedGroupingObjects serializedScript)
    {
        script.orbitControlsScript = GameObject.Find(serializedScript.orbitControlsScriptName)?.GetComponent<OrbitControls>();
        script.replaceOrAddScript = GameObject.Find(serializedScript.replaceOrAddScriptName)?.GetComponent<ReplaceOrAdd>();
    }
    private static SerializedOutline SerializeOutline(Outline outline)
    {
        SerializedOutline serializedOutline = new SerializedOutline
        {
            outlineMode = outline.OutlineMode,
            outlineColor = outline.OutlineColor,
            outlineWidth = outline.OutlineWidth,
            precomputeOutline = outline.precomputeOutline
        };

        return serializedOutline;
    }

    private static void DeserializeOutline(Outline outline, SerializedOutline serializedOutline)
    {
        outline.OutlineMode = serializedOutline.outlineMode;
        outline.OutlineColor = serializedOutline.outlineColor;
        outline.OutlineWidth = serializedOutline.outlineWidth;
        outline.precomputeOutline = serializedOutline.precomputeOutline;
    }

}

[System.Serializable]
public class SerializedMeshRenderer
{
    public string meshAssetPath;
    public string[] materialGUIDs;
    public string meshName;
    public string[] materialNames;
}
[System.Serializable]
public class SerializedMeshCollider
{
    public string meshAssetPath;
    public bool isConvex;
}

[System.Serializable]
public class SerializedBoxCollider
{
    public Vector3 center;
    public Vector3 size;
    public bool isEnabled;
}
[System.Serializable]
public class SerializedRigidbody
{
    public Vector3 velocity;
    public Vector3 angularVelocity;
    public float mass;
    public bool isKinematic;
    public bool useGravity;
    public RigidbodyConstraints constraints;
    public RigidbodyInterpolation interpolation;
    public CollisionDetectionMode collisionDetectionMode;
    public Vector3 centerOfMass;
    public Vector3 inertiaTensor;
    public Quaternion inertiaTensorRotation;
    public float drag;
    public float angularDrag;
    public float maxAngularVelocity;
    public float maxDepenetrationVelocity;
    public int solverIterations;
    public int solverVelocityIterations;
}
[System.Serializable]
public class SerializedAssignModels
{
    public string standardModelPrefabPath;
    public string windowModelPrefabPath;
    public string switchWindowScriptName;
    public string rotatorScriptName;
}
[System.Serializable]
public class SerializedGroupingObjects
{
    public string orbitControlsScriptName;
    public string replaceOrAddScriptName;
}
[System.Serializable]
public class SerializedOutline
{
    public Outline.Mode outlineMode;
    public Color outlineColor;
    public float outlineWidth;
    public bool precomputeOutline;


}