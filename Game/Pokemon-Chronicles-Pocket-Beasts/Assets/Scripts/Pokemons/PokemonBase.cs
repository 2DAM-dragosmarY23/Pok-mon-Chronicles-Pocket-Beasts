using UnityEngine;

[CreateAssetMenu(fileName = "PokemonBase", menuName = "Pokemons/New Pokemon")]

public class PokemonBase : ScriptableObject
{
    [SerializeField] string pokemonName;

    [TextArea]
    [SerializeField] string pokemonDescription;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    // Base Stats
    [SerializeField] int maxHP;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int speed;
    [SerializeField] int specialAttack;
    [SerializeField] int specialDefense;

}

public enum PokemonType
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon
}
