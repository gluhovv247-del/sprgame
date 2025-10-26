using UnityEngine;
using UnityEngine.UI;

public class GlobalVolume : MonoBehaviour
{
    public Slider slider;

    void Start()
    {
        // ������������� ��������� �������� �������� = ������� ���������
        slider.value = AudioListener.volume;

        // ������������� �� ��������� ��������
        slider.onValueChanged.AddListener(SetVolume);
    }

    void SetVolume(float value)
    {
        // ������ ��������� ���� ����
        AudioListener.volume = value;
    }
}