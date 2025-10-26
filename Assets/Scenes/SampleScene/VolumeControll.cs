using UnityEngine;
using UnityEngine.UI;

public class GlobalVolume : MonoBehaviour
{
    public Slider slider;

    void Start()
    {
        // Устанавливаем начальное значение слайдера = текущая громкость
        slider.value = AudioListener.volume;

        // Подписываемся на изменение значения
        slider.onValueChanged.AddListener(SetVolume);
    }

    void SetVolume(float value)
    {
        // Меняем громкость всей игры
        AudioListener.volume = value;
    }
}