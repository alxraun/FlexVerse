// parents: @.frostline/graph/m-flexverse-root.xml

using UnityEngine;
using Verse;

namespace FlexVerse
{
    /// <summary>Hosts the global runtime services as a persistent Unity object. (M-FLEXVERSE-ROOT, REQ-001)</summary>
    [StaticConstructorOnStartup]
    public class FlexVerseRoot : MonoBehaviour
    {
        static FlexVerseRoot()
        {
            GameObject rootObject = new("FlexVerse");
            rootObject.AddComponent<FlexVerseRoot>();
            DontDestroyOnLoad(rootObject);
        }

        /// <summary>Gets the single live root instance. (REQ-001)</summary>
        public static FlexVerseRoot Instance { get; private set; }

        /// <summary>Gets the global behavior updater. (REQ-002)</summary>
        public BehaviorUpdater BehaviorUpdater { get; private set; }

        /// <summary>Gets the global behavior registry.</summary>
        public BehaviorRegistry BehaviorRegistry { get; private set; }

        void Awake()
        {
            if (Instance != null)
            {
                Log.Error("M-FLEXVERSE-ROOT", "root.duplicate", ("gameObject", gameObject));
                Destroy(gameObject);
                return;
            }

            Instance = this;
            BehaviorUpdater = new BehaviorUpdater();
            BehaviorRegistry = new BehaviorRegistry();
        }

        void Update()
        {
            BehaviorUpdater.Update(Time.deltaTime);
        }

        void OnGUI() { }

        void OnDestroy()
        {
            if (!ReferenceEquals(Instance, this))
                return;

            AppearanceMasks.DisposeCaches();
            GradientTextures.ClearCache();
            Instance = null;
        }
    }
}
