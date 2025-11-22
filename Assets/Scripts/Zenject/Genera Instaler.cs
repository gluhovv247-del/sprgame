using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GeneraInstaler : MonoInstaller
{
    [SerializeField] private DialoguesInstaller dialoguesInstaller;
    public override void InstallBindings()
    {
        BindDialoguesInstaller();
    }

    public void BindDialoguesInstaller()
    {
        Container.Bind<DialoguesInstaller>().FromInstance(dialoguesInstaller).AsSingle();
    }
    
}
