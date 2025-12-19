using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum BattleState { START, PLAYERACTION, PLAYERMOVE, ENEMYMOVE, BUSY }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialog dialogBox;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;

    public void StartBattle()
    {
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup();
        enemyUnit.Setup();
        playerHud.SetData(playerUnit.Pokemon);
        enemyHud.SetData(enemyUnit.Pokemon);

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        //Esperar un momento antes de iniciar el dialogo
        yield return dialogBox.TypeDialog("Un " + enemyUnit.Pokemon.Base.name + " salvaje ha aparecido!");

        PlayerAction();
    }

    void PlayerAction()
    {
        state = BattleState.PLAYERACTION;
        StartCoroutine(dialogBox.TypeDialog("¿Qué acción debería tomar?"));
        dialogBox.EnableActionSelector(true);
    }

    void PlayerMove()
    {
        state = BattleState.PLAYERMOVE;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);

    }

    IEnumerator PerformPlayerMove()
    {
        state = BattleState.BUSY;

        var move = playerUnit.Pokemon.Moves[currentAction];
        move.PP--;
        yield return dialogBox.TypeDialog(playerUnit.Pokemon.Base.name + " usa " + move.Base.MoveName + "!");

        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        enemyUnit.PlayHitAnimation();

        var damageDetails = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
        yield return enemyHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog("El " + enemyUnit.Pokemon.Base.name + " se ha debilitado.");
            enemyUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            OnBattleOver(true);
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.ENEMYMOVE;

        var move = enemyUnit.Pokemon.GetRandomMove();
        move.PP--;
        yield return dialogBox.TypeDialog("El " + enemyUnit.Pokemon.Base.name + " usa " + move.Base.MoveName + "!");

        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        playerUnit.PlayHitAnimation();

        var damageDetails = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
        yield return playerHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog("Tu " + playerUnit.Pokemon.Base.name + " se ha debilitado.");
            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            OnBattleOver(false);
        }
        else
        {
            PlayerAction();
        }
    }

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



    public void HandleUpdate()
    {
        if (state == BattleState.PLAYERACTION)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PLAYERMOVE)
        {
            HandleMoveSelection();
        }
    }

    void HandleActionSelection()
    {
        // Lógica para manejar la selección de acciones del jugador
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 1)
                currentAction++;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 0)
                currentAction--;
        }

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                // Atacar
                PlayerMove();
            }
            else if (currentAction == 1)
            {
                // Huir
                dialogBox.EnableActionSelector(false);
                StartCoroutine(dialogBox.TypeDialog("¡No puedes huir de esta batalla!"));
            }
        }
    }

    void HandleMoveSelection()
    {
        // Lógica para manejar la selección de movimientos del jugador
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentAction < playerUnit.Pokemon.Moves.Count - 1)
                currentAction++;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentAction > 0)
                currentAction--;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < playerUnit.Pokemon.Moves.Count - 2)
                currentAction += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 1)
                currentAction -= 2;
        }

        dialogBox.UpdateMoveSelection(currentAction, playerUnit.Pokemon.Moves[currentAction]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
    }



}
