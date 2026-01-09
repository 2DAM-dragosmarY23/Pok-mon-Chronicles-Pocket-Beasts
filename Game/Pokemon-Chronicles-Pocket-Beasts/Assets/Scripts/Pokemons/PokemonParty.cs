using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Componente que representa el equipo de Pokémon del jugador.
/// Gestiona la lista de Pokémon disponibles y proporciona métodos para consultar su estado.
/// </summary>
public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    public List<Pokemon> Pokemons
    {
        get { return pokemons; }
    }

    /// <summary>
    /// Inicializa todos los Pokémon del equipo al comenzar el juego.
    /// Debe ejecutarse antes de iniciar cualquier batalla.
    /// </summary>
    private void Start()
    {
        foreach (var pokemon in pokemons)
        {
            pokemon.Init();
        }
    }

    /// <summary>
    /// Busca y devuelve el primer Pokémon del equipo que no esté debilitado.
    /// Útil para determinar qué Pokémon enviar automáticamente al campo.
    /// </summary>
    /// <returns>Primer Pokémon con HP > 0, o null si todos están debilitados</returns>
    public Pokemon GetHealthyPokemon()
    {
        return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }
}