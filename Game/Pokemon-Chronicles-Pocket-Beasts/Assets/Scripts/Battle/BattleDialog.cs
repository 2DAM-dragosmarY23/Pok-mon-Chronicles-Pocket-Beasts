using UnityEngine;
using TMPro;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;

/// <summary>
/// Gestiona la interfaz de diálogo y selección durante las batallas.
/// Controla la visualización de texto, menús de acción y detalles de movimientos.
/// </summary>
public class BattleDialog : MonoBehaviour
{
    [SerializeField] int lettersPerSecond;
    [SerializeField] Color highlightedColor;

    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;

    [SerializeField] List<TMPro.TMP_Text> actionTexts;
    [SerializeField] List<TMPro.TMP_Text> moveTexts;

    [SerializeField] TMPro.TMP_Text ppText;
    [SerializeField] TMPro.TMP_Text typeText;

    /// <summary>
    /// Establece inmediatamente el texto del diálogo sin efecto de escritura.
    /// </summary>
    public void SetDialog(string message)
    {
        dialogText.text = message;
    }

    /// <summary>
    /// Muestra el mensaje letra por letra, simulando una máquina de escribir.
    /// Añade una pausa al finalizar para dar tiempo al jugador a leer.
    /// </summary>
    public IEnumerator TypeDialog(string message)
    {
        dialogText.text = "";
        foreach (var letter in message.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// Habilita o deshabilita la visualización del texto de diálogo.
    /// </summary>
    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }

    /// <summary>
    /// Muestra u oculta el selector de acciones principales.
    /// </summary>
    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }

    /// <summary>
    /// Muestra u oculta el selector de movimientos y sus detalles.
    /// </summary>
    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    /// <summary>
    /// Actualiza el resaltado visual de la acción seleccionada actualmente.
    /// </summary>
    /// <param name="selectedAction">Índice de la acción seleccionada (0-3)</param>
    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < actionTexts.Count; i++)
        {
            if (i == selectedAction)
                actionTexts[i].color = highlightedColor;
            else
                actionTexts[i].color = Color.black;
        }
    }

    /// <summary>
    /// Actualiza el resaltado del movimiento seleccionado y muestra sus detalles (PP y tipo).
    /// </summary>
    /// <param name="selectedMove">Índice del movimiento seleccionado</param>
    /// <param name="move">Datos del movimiento para mostrar detalles</param>
    public void UpdateMoveSelection(int selectedMove, Move move)
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (i == selectedMove)
                moveTexts[i].color = highlightedColor;
            else
                moveTexts[i].color = Color.black;
        }
        ppText.text = $"PP {move.PP}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString();
    }

    /// <summary>
    /// Establece los nombres de los movimientos disponibles en el selector.
    /// Si hay menos de 4 movimientos, rellena los espacios vacíos con guiones.
    /// </summary>
    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (i < moves.Count)
                moveTexts[i].text = moves[i].Base.MoveName;
            else
                moveTexts[i].text = "-";
        }
    }
}