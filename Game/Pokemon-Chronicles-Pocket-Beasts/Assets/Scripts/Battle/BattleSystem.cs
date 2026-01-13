using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// Enumerado que representa los diferentes estados posibles durante una batalla
public enum BattleState { START, ACTIONSELECTION, MOVESELECTION, PERFORMMOVE, BUSY, PARTYSCREEN, BATTLEOVER }

/// <summary>
/// Controlador principal del sistema de combate por turnos.
/// Gestiona el flujo de batalla entre el jugador y Pokémon salvajes,
/// coordinando las acciones, turnos y transiciones de estado.
/// </summary>
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialog dialogBox;
    [SerializeField] PartyScreen partyScreen;

    // Evento que notifica cuando la batalla termina, indicando si el jugador ganó o perdió
    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;

    PokemonParty playerParty;
    Pokemon wildPokemon;

    /// <summary>
    /// Inicializa una batalla con el equipo del jugador y un Pokémon salvaje.
    /// </summary>
    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());
    }

    /// <summary>
    /// Configura el escenario de batalla y muestra el diálogo inicial.
    /// Prepara las unidades de combate y la interfaz de usuario.
    /// </summary>
    public IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetHealthyPokemon());
        enemyUnit.Setup(wildPokemon);

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        // Esperar un momento antes de iniciar el dialogo
        yield return dialogBox.TypeDialog("Un " + enemyUnit.Pokemon.Base.name + " salvaje ha aparecido!");

        ActionSelection();
    }

    /// <summary>
    /// Finaliza la batalla y notifica al sistema de juego el resultado.
    /// </summary>
    /// <param name="won">Indica si el jugador ganó la batalla</param>
    void BattleOver(bool won)
    {
        state = BattleState.BATTLEOVER;
        OnBattleOver(won);
    }

    /// <summary>
    /// Activa el menú de selección de acciones principales (Atacar, Mochila, Pokémon, Huir).
    /// </summary>
    void ActionSelection()
    {
        state = BattleState.ACTIONSELECTION;
        dialogBox.SetDialog("¿Qué acción debería tomar?");
        dialogBox.EnableActionSelector(true);
    }

    /// <summary>
    /// Abre la pantalla del equipo para permitir el cambio de Pokémon.
    /// </summary>
    void OpenPartyScreen()
    {
        state = BattleState.PARTYSCREEN;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    /// <summary>
    /// Activa el menú de selección de movimientos del Pokémon actual.
    /// </summary>
    void MoveSelection()
    {
        state = BattleState.MOVESELECTION;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    /// <summary>
    /// Ejecuta el movimiento seleccionado por el jugador.
    /// Tras la ejecución, cede el turno al enemigo si la batalla continúa.
    /// </summary>
    IEnumerator PlayerMove()
    {
        state = BattleState.PERFORMMOVE;

        var move = playerUnit.Pokemon.Moves[currentMove];

        yield return RunMove(playerUnit, enemyUnit, move);

        // Si el estado de la batalla no ha sido cambiado por RunMove, entonces va al siguiente turno
        if (state == BattleState.PERFORMMOVE)
            StartCoroutine(EnemyMove());
    }

    /// <summary>
    /// Ejecuta un movimiento aleatorio del Pokémon enemigo.
    /// Tras la ejecución, devuelve el control al jugador si la batalla continúa.
    /// </summary>
    IEnumerator EnemyMove()
    {
        state = BattleState.PERFORMMOVE;

        var move = enemyUnit.Pokemon.GetRandomMove();

        yield return RunMove(enemyUnit, playerUnit, move);

        if (state == BattleState.PERFORMMOVE)
            ActionSelection();
    }

    /// <summary>
    /// Ejecuta un movimiento de batalla entre dos unidades.
    /// Gestiona el daño, efectos de estado y animaciones correspondientes.
    /// </summary>
    /// <param name="sourceUnit">Unidad que ejecuta el movimiento</param>
    /// <param name="targetUnit">Unidad que recibe el movimiento</param>
    /// <param name="move">Movimiento a ejecutar</param>
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        move.PP--;
        yield return dialogBox.TypeDialog(sourceUnit.Pokemon.Base.name + " usa " + move.Base.MoveName + "!");

        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        targetUnit.PlayHitAnimation();

        // Los movimientos de estado modifican estadísticas en lugar de causar daño
        if (move.Base.Category == MoveCategory.Status)
        {
            var effects = move.Base.Effects;
            if (effects.Boosts != null)
            {
                if (move.Base.Target == MoveTarget.Self)
                {
                    sourceUnit.Pokemon.ApplyBoosts(effects.Boosts);
                    yield return dialogBox.TypeDialog(sourceUnit.Pokemon.Base.name + " ha aumentado sus estadísticas.");
                }
                else
                {
                    targetUnit.Pokemon.ApplyBoosts(effects.Boosts);
                    yield return dialogBox.TypeDialog(targetUnit.Pokemon.Base.name + " ha visto sus estadísticas disminuir.");
                }
            }
        }
        else
        {
            var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
            yield return targetUnit.Hud.UpdateHP();
            yield return ShowDamageDetails(damageDetails);
        }

        // Verificar si el Pokémon objetivo ha sido debilitado
        if (targetUnit.Pokemon.HP <= 0)
        {
            yield return dialogBox.TypeDialog("El " + targetUnit.Pokemon.Base.name + " se ha debilitado.");
            targetUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(targetUnit);
        }
    }

    /// <summary>
    /// Verifica si la batalla debe finalizar tras el debilitamiento de un Pokémon.
    /// Abre la pantalla de cambio si el jugador tiene más Pokémon disponibles.
    /// </summary>
    /// <param name="faintedUnit">Unidad que ha sido debilitada</param>
    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else
            {
                dialogBox.TypeDialog("¡No tienes más Pokémons!");
                BattleOver(false);
            }
        }
        else
        {
            BattleOver(true);
        }
    }

    /// <summary>
    /// Muestra mensajes informativos sobre los detalles del daño causado
    /// (golpe crítico, efectividad de tipo).
    /// </summary>
    IEnumerator ShowDamageDetails(DamageDetails details)
    {
        if (details.Critical > 1f)
        {
            yield return dialogBox.TypeDialog("¡Un golpe crítico!");
        }
        if (details.TypeEffectiveness > 1f)
        {
            yield return dialogBox.TypeDialog("¡Es súper efectivo!");
        }
        else if (details.TypeEffectiveness < 1f)
        {
            yield return dialogBox.TypeDialog("No es muy efectivo...");
        }
    }

    /// <summary>
    /// Intenta escapar de la batalla salvaje.
    /// La probabilidad de éxito depende de la diferencia de velocidad entre ambos Pokémon.
    /// </summary>
    IEnumerator TryToEscape()
    {
        state = BattleState.BUSY;
        dialogBox.EnableActionSelector(false);

        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        // Si el jugador es más rápido, el escape es automático
        if (playerSpeed >= enemySpeed)
        {
            yield return dialogBox.TypeDialog("¡Has escapado con éxito!");
            yield return new WaitForSeconds(1f);
            BattleOver(false);
        }
        else
        {
            // Cálculo de probabilidad basado en la fórmula clásica de Pokémon
            float escapeChance = (playerSpeed * 128f) / enemySpeed + 30f;

            if (UnityEngine.Random.Range(0, 256) < escapeChance)
            {
                yield return dialogBox.TypeDialog("¡Has escapado con éxito!");
                yield return new WaitForSeconds(1f);
                BattleOver(false);
            }
            else
            {
                yield return dialogBox.TypeDialog("¡No has podido escapar!");
                yield return new WaitForSeconds(1f);
                StartCoroutine(EnemyMove());
            }
        }
    }

    /// <summary>
    /// Método principal que gestiona las entradas del usuario según el estado actual de la batalla.
    /// Debe ser invocado cada frame desde el GameController.
    /// </summary>
    public void HandleUpdate()
    {
        if (state == BattleState.ACTIONSELECTION)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MOVESELECTION)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PARTYSCREEN)
        {
            HandlePartyScreen();
        }
    }

    /// <summary>
    /// Procesa las entradas del teclado durante la selección de acción principal.
    /// Permite navegar entre las cuatro opciones disponibles y confirmar la selección.
    /// Reproduce sonidos de navegación y confirmación.
    /// </summary>
    void HandleActionSelection()
    {
        int previousAction = currentAction;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentAction++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentAction--;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentAction -= 2;

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        // Reproducir sonido solo si la selección cambió
        if (currentAction != previousAction)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySelectionSound();
            }
        }

        dialogBox.UpdateActionSelection(currentAction);

        // Tecla Z confirma la selección
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // Reproducir sonido de confirmación
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySelectionSound();
            }

            if (currentAction == 0)
            {
                // Atacar
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                // Mochila (no implementada)
                dialogBox.EnableActionSelector(false);
                StartCoroutine(dialogBox.TypeDialog("¡No hay objetos disponibles!"));
            }
            else if (currentAction == 2)
            {
                // Cambiar Pokémon
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                // Huir
                StartCoroutine(TryToEscape());
            }
        }
    }

    /// <summary>
    /// Procesa las entradas del teclado durante la selección de movimiento.
    /// Permite navegar entre los movimientos disponibles del Pokémon.
    /// Reproduce sonidos de navegación y confirmación.
    /// </summary>
    void HandleMoveSelection()
    {
        int previousMove = currentMove;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentMove++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentMove--;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMove -= 2;

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        // Reproducir sonido solo si la selección cambió
        if (currentMove != previousMove)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySelectionSound();
            }
        }

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        // Z confirma el movimiento seleccionado
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // Reproducir sonido de confirmación
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySelectionSound();
            }

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PlayerMove());
        }
        // X cancela y vuelve al menú de acciones
        else if (Input.GetKeyDown(KeyCode.X))
        {
            // Reproducir sonido de cancelación
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySelectionSound();
            }

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    /// <summary>
    /// Procesa las entradas del teclado en la pantalla del equipo.
    /// Valida que el Pokémon seleccionado pueda entrar en combate.
    /// Reproduce sonidos de navegación y confirmación.
    /// </summary>
    void HandlePartyScreen()
    {
        int previousMember = currentMember;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentMember++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentMember--;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMember += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMember -= 2;

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);

        // Reproducir sonido solo si la selección cambió
        if (currentMember != previousMember)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySelectionSound();
            }
        }

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Pokemons[currentMember];

            // Validaciones antes de permitir el cambio
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("No puedes enviar a un Pokémon debilitado.");
                return;
            }
            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("Ese Pokémon ya está en batalla.");
                return;
            }

            // Reproducir sonido de confirmación
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySelectionSound();
            }

            partyScreen.gameObject.SetActive(false);
            state = BattleState.BUSY;
            StartCoroutine(SwitchPokemon(selectedMember));
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            // Reproducir sonido de cancelación
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySelectionSound();
            }

            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    /// <summary>
    /// Realiza el cambio de Pokémon en batalla.
    /// Retira al Pokémon actual (si sigue consciente) y envía al nuevo.
    /// </summary>
    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        // Solo mostrar animación de retirada si el Pokémon actual sigue en pie
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"¡Vuelve {playerUnit.Pokemon.Base.name}!");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newPokemon);
        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return dialogBox.TypeDialog($"¡Ve {newPokemon.Base.name}!");

        // El enemigo ataca después del cambio
        StartCoroutine(EnemyMove());
    }
}