using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dispatcher que permite ejecutar acciones en el hilo principal de Unity desde otros hilos.
/// Esencial para ejecutar callbacks de Firebase (que vienen de hilos secundarios)
/// de forma segura en el contexto de Unity.
/// Se debe colocar en un GameObject root y marcarlo como DontDestroyOnLoad.
/// </summary>
public class UnityMainThreadDispatcher : MonoBehaviour
{
    static readonly Queue<Action> _executionQueue = new Queue<Action>();
    static UnityMainThreadDispatcher _instance;

    /// <summary>
    /// Obtiene o crea la instancia singleton del dispatcher.
    /// </summary>
    public static UnityMainThreadDispatcher Instance()
    {
        if (_instance == null)
        {
            var go = new GameObject("UnityMainThreadDispatcher");
            _instance = go.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }
        return _instance;
    }

    /// <summary>
    /// Inicializa el singleton y asegura persistencia entre escenas.
    /// </summary>
    void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this) Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Encola una acción para ser ejecutada en el hilo principal durante el próximo Update.
    /// </summary>
    /// <param name="action">Acción a ejecutar en el hilo principal</param>
    public void Enqueue(Action action)
    {
        if (action == null) return;
        lock (_executionQueue) { _executionQueue.Enqueue(action); }
    }

    /// <summary>
    /// Ejecuta todas las acciones encoladas durante cada frame.
    /// Los errores se capturan y registran sin detener la ejecución.
    /// </summary>
    void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                try { _executionQueue.Dequeue().Invoke(); }
                catch (Exception ex) { Debug.LogException(ex); }
            }
        }
    }
}