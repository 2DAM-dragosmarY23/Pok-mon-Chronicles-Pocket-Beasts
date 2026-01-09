using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Componente de interfaz que representa un único Pokémon en la pantalla del equipo.
/// Muestra el nombre, nivel y barra de salud del Pokémon.
/// </summary>
public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] HpBar hpBar;

    [SerializeField] Color highlightedColor;

    Pokemon _pokemon;

    /// <summary>
    /// Establece los datos del Pokémon que este slot representa.
    /// Actualiza todos los elementos visuales con la información correspondiente.
    /// </summary>
    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;

        nameText.text = pokemon.Base.name;
        levelText.text = "Lvl " + pokemon.Level;
        hpBar.SetHp((float)pokemon.HP / pokemon.MaxHP);
    }

    /// <summary>
    /// Cambia el color del nombre para indicar si este Pokémon está seleccionado.
    /// Proporciona retroalimentación visual al usuario durante la navegación.
    /// </summary>
    /// <param name="selected">True si el Pokémon debe aparecer resaltado</param>
    public void SetSelected(bool selected)
    {
        if (selected)
            nameText.color = highlightedColor;
        else
            nameText.color = Color.black;
    }
}