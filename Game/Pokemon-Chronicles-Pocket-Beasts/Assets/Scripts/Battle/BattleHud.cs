using UnityEngine;
using TMPro;
using System.Collections;

public class BattleHud : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] HpBar hpBar;

    Pokemon _pokemon;

    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;

        nameText.text = pokemon.Base.name;
        levelText.text = "Lvl " + pokemon.Level;
        hpBar.SetHp((float)pokemon.HP / pokemon.MaxHP);
    }

    public IEnumerator UpdateHP()
    {
        yield return hpBar.SetHpSmooth((float)_pokemon.HP / _pokemon.MaxHP);
    }
}
