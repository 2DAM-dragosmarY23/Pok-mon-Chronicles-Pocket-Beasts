using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Controlador de la pantalla que muestra el equipo de Pokémon del jugador.
/// Permite visualizar y seleccionar entre los Pokémon disponibles durante la batalla.
/// </summary>
public class PartyScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;

    PartyMemberUI[] memberSlots;
    List<Pokemon> pokemons;

    /// <summary>
    /// Inicializa la pantalla obteniendo todos los slots de miembros del equipo.
    /// </summary>
    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>();
    }

    /// <summary>
    /// Configura los datos del equipo para su visualización.
    /// Activa los slots necesarios y oculta los sobrantes si el equipo tiene menos de 6 Pokémon.
    /// </summary>
    /// <param name="pokemons">Lista de Pokémon del equipo del jugador</param>
    public void SetPartyData(List<Pokemon> pokemons)
    {
        this.pokemons = pokemons;

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < pokemons.Count)
            {
                memberSlots[i].SetData(pokemons[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        messageText.text = "Elige un Pokémon";
    }

    /// <summary>
    /// Actualiza el resaltado visual para indicar qué miembro del equipo está seleccionado actualmente.
    /// </summary>
    /// <param name="selectedMember">Índice del miembro seleccionado</param>
    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i == selectedMember)
            {
                memberSlots[i].SetSelected(true);
            }
            else
            {
                memberSlots[i].SetSelected(false);
            }
        }
    }

    /// <summary>
    /// Establece el mensaje que se muestra en la parte inferior de la pantalla del equipo.
    /// Útil para mostrar errores o información contextual al jugador.
    /// </summary>
    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}