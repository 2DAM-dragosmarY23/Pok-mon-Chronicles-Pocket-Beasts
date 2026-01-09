using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Representa una unidad de combate en batalla (jugador o enemigo).
/// Gestiona la visualización del Pokémon, sus animaciones y su interfaz HUD.
/// </summary>
public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;

    public bool IsPlayerUnit { get { return isPlayerUnit; } }
    public BattleHud Hud { get { return hud; } }
    public Pokemon Pokemon { get; set; }

    Image image;
    Vector3 originalPosition;
    Color originalColor;

    private void Awake()
    {
        image = GetComponent<Image>();
        originalPosition = image.transform.localPosition;
        originalColor = image.color;
    }

    /// <summary>
    /// Configura la unidad con un Pokémon específico.
    /// Establece el sprite correcto (frontal o trasero) y actualiza el HUD.
    /// </summary>
    public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;

        // Los Pokémon del jugador se muestran de espaldas
        if (isPlayerUnit)
            image.sprite = Pokemon.Base.BackSprite;
        else
            image.sprite = Pokemon.Base.FrontSprite;

        hud.SetData(pokemon);

        image.color = originalColor;
        PlayEnterAnimation();
    }

    /// <summary>
    /// Reproduce la animación de entrada del Pokémon al campo de batalla.
    /// Los Pokémon entran desde los laterales de la pantalla.
    /// </summary>
    public void PlayEnterAnimation()
    {
        if (isPlayerUnit)
            image.transform.localPosition = new Vector3(-500f, originalPosition.y);
        else
            image.transform.localPosition = new Vector3(500f, originalPosition.y);

        image.transform.DOLocalMoveX(originalPosition.x, 1f);
    }

    /// <summary>
    /// Reproduce la animación de ataque: el Pokémon avanza y retrocede.
    /// </summary>
    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if (isPlayerUnit)
        {
            sequence.Append(image.transform.DOLocalMoveX(originalPosition.x + 50f, 0.25f));
        }
        else
        {
            sequence.Append(image.transform.DOLocalMoveX(originalPosition.x - 50f, 0.25f));
        }

        sequence.Append(image.transform.DOLocalMoveX(originalPosition.x, 0.25f));
    }

    /// <summary>
    /// Reproduce la animación de recibir daño: parpadeo de color gris.
    /// </summary>
    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));
    }

    /// <summary>
    /// Reproduce la animación de debilitamiento: el Pokémon cae y desaparece.
    /// </summary>
    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(originalPosition.y - 150f, 0.5f));
        sequence.Join(image.DOFade(0f, 0.5f));
    }
}