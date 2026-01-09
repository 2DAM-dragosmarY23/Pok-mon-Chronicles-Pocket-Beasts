using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Controlador principal del personaje del jugador.
/// Gestiona el movimiento, detección de colisiones y encuentros con Pokémon salvajes.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public LayerMask solidObjectsLayer;
    public LayerMask grassLayer;

    // Evento que se dispara cuando ocurre un encuentro con Pokémon salvaje
    public event Action OnEncountered;

    private bool isMoving;
    private Vector2 input;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Método principal que procesa las entradas del jugador cada frame.
    /// Debe ser invocado desde el GameController cuando el estado es FreeRoam.
    /// </summary>
    public void HandleUpdate()
    {
        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            // Eliminar movimiento diagonal priorizando horizontal
            if (input.x != 0) input.y = 0;

            if (input != Vector2.zero)
            {
                // Actualizar parámetros del animador para reflejar la dirección
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);

                // Calcular posición objetivo en el grid
                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                if (IsWalkable(targetPos))
                    StartCoroutine(Move(targetPos));
            }
        }

        animator.SetBool("isMoving", isMoving);
    }

    /// <summary>
    /// Corrutina que mueve suavemente al jugador hacia la posición objetivo.
    /// Implementa movimiento tipo grid característico de los juegos Pokémon.
    /// </summary>
    /// <param name="targetPos">Posición de destino en el grid</param>
    private IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;

        // Mover progresivamente hasta alcanzar la posición objetivo
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        isMoving = false;

        // Verificar si se produce un encuentro tras moverse
        CheckForEncounters();
    }

    /// <summary>
    /// Verifica si una posición es transitable comprobando colisiones con objetos sólidos.
    /// </summary>
    /// <param name="targetPos">Posición a verificar</param>
    /// <returns>True si la posición es accesible</returns>
    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer) != null)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Comprueba si el jugador está en hierba alta y determina aleatoriamente
    /// si ocurre un encuentro con Pokémon salvaje (10% de probabilidad).
    /// </summary>
    private void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) != null)
        {
            if (UnityEngine.Random.Range(1, 101) <= 10)
            {
                animator.SetBool("isMoving", false);
                OnEncountered?.Invoke();
            }
        }
    }
}