using UnityEngine;

/// <summary>
/// Enumerado que representa los estados principales del juego.
/// </summary>
public enum GameState { FreeRoam, Battle }

/// <summary>
/// Controlador principal del juego que coordina las transiciones entre
/// el modo de exploración libre y el sistema de combate.
/// Actúa como mediador entre el PlayerController y el BattleSystem.
/// </summary>
public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    GameState state;

    /// <summary>
    /// Suscribe los eventos de batalla al iniciar el juego.
    /// </summary>
    private void Start()
    {
        playerController.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;
    }

    /// <summary>
    /// Inicia una batalla salvaje al detectar un encuentro.
    /// Desactiva la cámara del mundo y activa el sistema de combate.
    /// </summary>
    void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();

        battleSystem.StartBattle(playerParty, wildPokemon);
    }

    /// <summary>
    /// Finaliza la batalla y devuelve al jugador al modo de exploración.
    /// </summary>
    /// <param name="won">Indica si el jugador ganó la batalla</param>
    void EndBattle(bool won)
    {
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    /// <summary>
    /// Método Update que delega las actualizaciones al sistema correspondiente
    /// según el estado actual del juego.
    /// </summary>
    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
    }
}