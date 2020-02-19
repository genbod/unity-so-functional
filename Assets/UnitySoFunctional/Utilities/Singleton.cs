using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.Utilities
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T m_Instance;
        public static bool m_isQuitting;

        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    // Making sure that there's no other instances of the same type
                    m_Instance = FindObjectOfType<T>();

                    if (m_Instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name;
                        m_Instance = obj.AddComponent<T>();
                    }
                }
                return m_Instance;
            }
        }

        // Virtual Awake() that can be overriden in a derived class
        public virtual void Awake()
        {
            if (m_Instance == null)
            {
                // If null, this instance is now the Singleton instance
                m_Instance = this as T;

                // Making sure Singleton Instance persists across every scene
                if (Application.IsPlaying(m_Instance))
                {
                    DontDestroyOnLoad(this.gameObject);
                }
            }
            else
            {
                // Destroy current instance because it must be a duplicate
                Destroy(gameObject);
            }
        }
    }
}