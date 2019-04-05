using DCL;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;


public class SceneController : MonoBehaviour
{
    public static SceneController i { get; private set; }

    public bool startDecentralandAutomatically = true;
    public bool VERBOSE = false;

    [System.NonSerialized]
    public bool isDebugMode;

    [FormerlySerializedAs("factoryManifest")]
    public DCLComponentFactory componentFactory;

    public Dictionary<string, ParcelScene> loadedScenes = new Dictionary<string, ParcelScene>();

    public string[] sceneBlacklist;

    class QueuedSceneMessage
    {
        public enum Type
        {
            NONE,
            SCENE_MESSAGE,
            LOAD_PARCEL,
            TELEPORT,
            UNLOAD_SCENES
        }

        public Type type;
        public string sceneId;
        public string message;
    }

    Queue<QueuedSceneMessage> pendingMessages = new Queue<QueuedSceneMessage>();

    public bool hasPendingMessages => pendingMessages != null && pendingMessages.Count > 0;
    public int pendingMessagesCount => pendingMessages != null ? pendingMessages.Count : 0;

    void Awake()
    {
        if (i != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(this); // We need the DestroyImmediate() when dealing with tests (like visual tests)
#else
            Utils.SafeDestroy(this);
#endif
            return;
        }

        i = this;

        Debug.Log("DCL Unity Build Version: " + DCL.Configuration.ApplicationSettings.version);

        // We trigger the Decentraland logic once SceneController has been instanced and is ready to act.
        if (startDecentralandAutomatically)
        {
            WebInterface.StartDecentraland();
        }

        StartCoroutine(ProcessMessages(pendingMessages));
    }

    public void CreateUIScene(string json)
    {
        CreateUISceneMessage uiScene = JsonUtility.FromJson<CreateUISceneMessage>(json);

        string uiSceneId = uiScene.id;

        if (!loadedScenes.ContainsKey(uiSceneId))
        {
            var newGameObject = new GameObject("UI Scene - " + uiSceneId );

            var newScene = newGameObject.AddComponent<GlobalScene>();
            newScene.ownerController = this;
            newScene.unloadWithDistance = false;

            LoadParcelScenesMessage.UnityParcelScene data = new LoadParcelScenesMessage.UnityParcelScene();
            data.id = uiSceneId;
            data.basePosition = new Vector2Int(0, 0);

            if (!loadedScenes.ContainsKey(uiSceneId))
            {
                loadedScenes.Add(uiSceneId, newScene);
            }
            else
            {
                loadedScenes[uiSceneId] = newScene;
            }
        }
    }

    IEnumerator ProcessMessages(Queue<QueuedSceneMessage> queue)
    {
        while (true)
        {
            float startTime = Time.realtimeSinceStartup;

            float prevDeltaTime = Time.deltaTime;

            float timeBudget = DCL.Configuration.MessageThrottlingSettings.GLOBAL_FRAME_THROTTLING_TIME;

            while (Time.realtimeSinceStartup - startTime < timeBudget && queue.Count > 0)
            {
                QueuedSceneMessage m = queue.Peek();

                switch (m.type)
                {
                    case QueuedSceneMessage.Type.NONE:
                        break;
                    case QueuedSceneMessage.Type.SCENE_MESSAGE:
                        ProcessMessage(m.sceneId, m.message);
                        break;
                    case QueuedSceneMessage.Type.LOAD_PARCEL:
                        yield return LoadParcelScenesExecute_Spread(m.message);
                        break;
                    case QueuedSceneMessage.Type.UNLOAD_SCENES:
                        UnloadAllScenes();
                        break;
                }

                queue.Dequeue();
            }

            yield return null;
        }
    }

    void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Input.GetMouseButtonDown(0))
            {
                LockCursor();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }
    }

    public void SetDebug()
    {
        isDebugMode = true;
    }

    public void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    ParcelScene GetDecentralandSceneOfGridPosition(Vector2Int gridPosition)
    {
        foreach (var estate in loadedScenes)
        {
            if (estate.Value.sceneData.basePosition.Equals(gridPosition))
            {
                return estate.Value;
            }

            foreach (var iteratedParcel in estate.Value.sceneData.parcels)
            {
                if (iteratedParcel == gridPosition)
                {
                    return estate.Value;
                }
            }
        }
        return null;
    }

    private LoadParcelScenesMessage loadParcelScenesMessage = new LoadParcelScenesMessage();

    public IEnumerator LoadParcelScenesExecute_Spread(string decentralandSceneJSON)
    {
        string[] jsons = decentralandSceneJSON.Split(new string[] { "}{" }, StringSplitOptions.None);

        foreach (string json in jsons)
        {
            float tb = Time.realtimeSinceStartup;

            LoadParcelScenesMessage.UnityParcelScene scene;

            scene = JsonUtility.FromJson<LoadParcelScenesMessage.UnityParcelScene>(json);

            if (scene == null)
                continue;

            if (Time.realtimeSinceStartup - tb > DCL.Configuration.MessageThrottlingSettings.LOAD_PARCEL_SCENES_THROTTLING_TIME)
                yield return null;

            var sceneToLoad = scene;

#if UNITY_EDITOR
            if (sceneBlacklist.Any((x) => { return sceneToLoad.id == x; }))
                continue;
#endif

            if (!loadedScenes.ContainsKey(sceneToLoad.id))
            {
                var newGameObject = new GameObject("New Scene");

                var newScene = newGameObject.AddComponent<ParcelScene>();
                newScene.SetData(sceneToLoad);
                newScene.ownerController = this;
                loadedScenes.Add(sceneToLoad.id, newScene);
            }
        }
    }

    [Obsolete("We should use the spread variant now")]
    public IEnumerator LoadParcelScenesExecute(string decentralandSceneJSON)
    {
        JsonUtility.FromJsonOverwrite(decentralandSceneJSON, this.loadParcelScenesMessage);

        var scenesToLoad = loadParcelScenesMessage.parcelsToLoad;
        var completeListOfParcelsThatShouldBeLoaded = new List<string>();

        // LOAD MISSING SCENES
        for (int i = 0; i < scenesToLoad.Count; i++)
        {
            var sceneToLoad = scenesToLoad[i];

            completeListOfParcelsThatShouldBeLoaded.Add(sceneToLoad.id);

            if (!loadedScenes.ContainsKey(sceneToLoad.id))
            {
                if (VERBOSE)
                    Debug.Log($"Creating scene {sceneToLoad.id}");

                var newGameObject = new GameObject("New Scene");

                var newScene = newGameObject.AddComponent<ParcelScene>();
                newScene.SetData(sceneToLoad);
                newScene.ownerController = this;

                if (!loadedScenes.ContainsKey(sceneToLoad.id))
                {
                    loadedScenes.Add(sceneToLoad.id, newScene);
                }
                else
                {
                    loadedScenes[sceneToLoad.id] = newScene;
                }
            }
        }

        yield break;
    }


    public void UnloadScene(string sceneKey)
    {
        if (loadedScenes.ContainsKey(sceneKey))
        {
            if (VERBOSE)
                Debug.Log($"Destroying scene {sceneKey}");

            var scene = loadedScenes[sceneKey];

            loadedScenes.Remove(sceneKey);

            if (scene)
            {
                Utils.SafeDestroy(scene.gameObject);
            }
        }
    }

    public void UnloadAllScenes()
    {
        var list = loadedScenes.ToArray();
        for (int i = 0; i < list.Length; i++)
        {
            UnloadScene(list[i].Key);
        }
    }

    public void LoadParcelScenes(string decentralandSceneJSON)
    {
        pendingMessages.Enqueue(new QueuedSceneMessage() { type = QueuedSceneMessage.Type.LOAD_PARCEL, message = decentralandSceneJSON });
    }

    public void UnloadAllScenesQueued()
    {
        pendingMessages.Enqueue(new QueuedSceneMessage() { type = QueuedSceneMessage.Type.UNLOAD_SCENES });
    }

    public void SendSceneMessage(string payload)
    {
        var chunks = payload.Split('\n');

        for (int i = 0; i < chunks.Length; i++)
        {
            if (chunks[i].Length > 0)
            {
                var separatorPosition = chunks[i].IndexOf('\t');

                if (separatorPosition == -1) continue;

                var sceneId = chunks[i].Substring(0, separatorPosition);
                var message = chunks[i].Substring(separatorPosition + 1);

#if UNITY_EDITOR
                if (sceneBlacklist.Any((x) => { return sceneId == x; }))
                    continue;
#endif

                pendingMessages.Enqueue(new QueuedSceneMessage() { type = QueuedSceneMessage.Type.SCENE_MESSAGE, sceneId = sceneId, message = message });
            }
        }
    }



    public bool ProcessMessage(string sceneId, string message)
    {
#if UNITY_EDITOR
        if (sceneBlacklist.Any((x) => { return sceneId == x; }))
            return true;
#endif

        ParcelScene scene;

        if (loadedScenes.TryGetValue(sceneId, out scene))
        {
            if (!scene.gameObject.activeInHierarchy)
                return true;

            var separatorPosition = message.IndexOf('\t');
            var method = message.Substring(0, separatorPosition);
            var payload = message.Substring(separatorPosition + 1);

            if (VERBOSE)
                Debug.Log("SceneController ProcessMessage: \nMethod: " + method + "\nPayload: " + payload);

            switch (method)
            {
                case "CreateEntity": scene.CreateEntity(payload); break;
                case "SetEntityParent": scene.SetEntityParent(payload); break;

                //NOTE(Brian): EntityComponent messages
                case "UpdateEntityComponent": scene.EntityComponentCreate(payload); break;
                case "ComponentRemoved": scene.EntityComponentRemove(payload); break;

                //NOTE(Brian): SharedComponent messages
                case "AttachEntityComponent": scene.SharedComponentAttach(payload); break;
                case "ComponentCreated": scene.SharedComponentCreate(payload); break;
                case "ComponentDisposed": scene.SharedComponentDispose(payload); break;
                case "ComponentUpdated": scene.SharedComponentUpdate(payload); break;
                case "RemoveEntity": scene.RemoveEntity(payload); break;
                default: Debug.LogError($"Unknown method {method}"); return true;
            }

            return true;
        }
        else
        {
            //Debug.LogError($"Scene not found {sceneId}");
            return false;
        }
    }

    public ParcelScene CreateTestScene(LoadParcelScenesMessage.UnityParcelScene data=null)
    {
        if (data == null)
            data = new LoadParcelScenesMessage.UnityParcelScene();

        if (data.basePosition == null)
        {
            data.basePosition = new Vector2Int(0, 0);
        }

        if (data.parcels == null)
        {
            data.parcels = new Vector2Int[] { data.basePosition };
        }

        if (string.IsNullOrEmpty(data.id))
        {
            data.id = $"(test):{data.basePosition.x},{data.basePosition.y}";
        }

        var go = new GameObject();
        var newScene = go.AddComponent<ParcelScene>();
        newScene.SetData(data);
        newScene.ownerController = this;
        newScene.isTestScene = true;

        if (!loadedScenes.ContainsKey(data.id))
        {
            loadedScenes.Add(data.id, newScene);
        }
        else
        {
            throw new Exception($"Scene {data.id} is already loaded.");
        }

        return newScene;
    }
}
