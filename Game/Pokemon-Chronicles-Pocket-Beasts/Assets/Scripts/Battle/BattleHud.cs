using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Controlador del HUD (interfaz de usuario) que muestra la información del Pokémon en batalla.
/// Presenta el nombre, nivel y barra de salud del Pokémon asociado.
/// </summary>
public class BattleHud : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] HpBar hpBar;

    Pokemon _pokemon;

    /// <summary>
    /// Inicializa el HUD con los datos de un Pokémon específico.
    /// Establece el nombre, nivel y porcentaje de salud inicial.
    /// </summary>
    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;

        nameText.text = pokemon.Base.name;
        levelText.text = "Lvl " + pokemon.Level;
        hpBar.SetHp((float)pokemon.HP / pokemon.MaxHP);
    }

    /// <summary>
    /// Actualiza la barra de salud de forma animada para reflejar el HP actual del Pokémon.
    /// Utiliza una corrutina para crear una transición suave en el cambio de salud.
    /// </summary>
    public IEnumerator UpdateHP()
    {
        yield return hpBar.SetHpSmooth((float)_pokemon.HP / _pokemon.MaxHP);
    }
}