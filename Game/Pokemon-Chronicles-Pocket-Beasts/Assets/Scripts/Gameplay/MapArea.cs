using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Componente que define un área del mapa con encuentros de Pokémon salvajes.
/// Contiene una lista de Pokémon que pueden aparecer en esa zona específica.
/// </summary>
public class MapArea : MonoBehaviour
{
    [SerializeField] List<Pokemon> wildPokemons;

    /// <summary>
    /// Selecciona aleatoriamente un Pokémon salvaje de los disponibles en esta área.
    /// Inicializa el Pokémon antes de devolverlo para asegurar que esté listo para batalla.
    /// </summary>
    /// <returns>Pokémon salvaje inicializado y listo para combate</returns>
    public Pokemon GetRandomWildPokemon()
    {
        var wildPokemon = wildPokemons[Random.Range(0, wildPokemons.Count)];
        wildPokemon.Init();
        return wildPokemon;
    }
}