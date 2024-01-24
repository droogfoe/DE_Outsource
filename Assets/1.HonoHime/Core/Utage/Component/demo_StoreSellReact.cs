using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utage;

public class demo_StoreSellReact : MonoBehaviour
{
    public void PlaySellReactAnim() 
    {
        Utage.CharacterCommandContext context = new PerformContext("Merchant", "Hello", true);
        UtageCharaterCommandHandler.Inst.OnDoCommand(context);
    }
}
