using System.Linq;
using loaforcsSoundAPI.Core.Util.Extensions;
using loaforcsSoundAPI.SoundPacks.Data;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;
using UnityEngine;
using UnityEngine.Pool;

namespace loaforcsSoundAPI.Core;

public sealed class AudioSourcePoolManager : MonoBehaviour {
    public static Transform? RootPool { get; private set; }

    public static IObjectPool<AudioSourcePool> SourcePool {
        get {
            if(field == null) {
                SpawnManager();

                AudioConfiguration audioConfig = AudioSettings.GetConfiguration();
                int numVoices = audioConfig.numRealVoices;

                field = new ObjectPool<AudioSourcePool>(CreatePooledSource, GetPooledSource, ReleasePooledSource, DestroyPooledSource, false, numVoices / 4, numVoices);
            }
            return field;
        }
    }

    private static AudioSourcePool CreatePooledSource() {
        GameObject sourceContainer = new("_PooledSource", [typeof(AudioSource), typeof(AudioSourcePool)]);
        sourceContainer.transform.SetParent(RootPool);
        sourceContainer.SetActive(false);

        return sourceContainer.GetComponent<AudioSourcePool>();
    }

    private static void GetPooledSource(AudioSourcePool pool) {
        pool.gameObject.SetActive(true);
    }

    private static void ReleasePooledSource(AudioSourcePool pool) {
        pool.assignedSource = null;
        pool.assignedAdditionalData = null;
        pool.gameObject.SetActive(false);
    }

    private static void DestroyPooledSource(AudioSourcePool pool) {
        Destroy(pool.gameObject);
    }

    private static void SpawnManager() {
        loaforcsSoundAPI.Logger.LogInfo("Starting PoolManager.");
        GameObject manager = new("SoundAPI_PoolManager");
        DontDestroyOnLoad(manager);
        RootPool = manager.AddComponent<AudioSourcePoolManager>().transform;
    }
}

[RequireComponent(typeof(AudioSource))]
public sealed class AudioSourcePool : MonoBehaviour {
    public AudioSource? assignedSource;
    public AudioSource pooledSource;
    public AudioSourceAdditionalData? assignedAdditionalData;
    public AudioSourceAdditionalData pooledAdditionalData;

    private void Awake() {
        pooledSource = GetComponent<AudioSource>();
        pooledAdditionalData = pooledSource.GetAdditionalData();
        pooledAdditionalData.IsPooled = true;
    }

    private void Update() {
        if(pooledAdditionalData.ReplacedWith?.UpdateEveryFrame != true) return;

        // Debuggers.UpdateEveryFrame?.Log($"success: updating every frame for {pooledSource.name}");

        SoundInstance? sound = pooledAdditionalData.ReplacedWith?.Sounds.FirstOrDefault(x => x.Evaluate(pooledAdditionalData.CurrentContext));

        if(sound == null) return;
        if(sound.Parent?.Volume.HasValue == true)
            pooledSource.volume = sound.Parent.Volume.Value;

        if(sound.Clip == pooledSource.clip) return;
        Debuggers.UpdateEveryFrame?.Log($"new clip found, swapping off of {sound.Clip.name}!!");

        float currentTime = pooledSource.time;
        if(currentTime >= sound.Clip.length) {
            pooledSource.Stop(); // TODO: Condition to remember playback time.
            return;
        }
        pooledSource.clip = sound.Clip;

        pooledSource.Play();
        pooledSource.time = currentTime;

        Debuggers.UpdateEveryFrame?.Log($"new clip found, swapped to {sound.Clip.name}");
    }

    private void LateUpdate() {
        if(assignedSource == null || assignedAdditionalData == null || !pooledSource.enabled || !pooledSource.isPlaying) {
            AudioSourcePoolManager.SourcePool.Release(this);
            return;
        }
        assignedSource.transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
        transform.SetPositionAndRotation(position, rotation); // TODO: Parenting the pooled AudioSource is probably better.
    }
}