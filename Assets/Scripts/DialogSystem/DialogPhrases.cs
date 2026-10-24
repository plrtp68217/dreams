using System.Collections;
using System.Collections.Generic;

public static class DialogPhrases
{
    // Показывает фразы по очереди и листает их по E, затем закрывает диалог
    public static IEnumerator Play(Dialog dialog, InputService inputService, IEnumerable<string> phraseIds)
    {
        foreach (string phraseId in phraseIds)
        {
            dialog.Enable(LocalizedDialogueSystem.Instance.GetDialogueText(phraseId));
            yield return inputService.WaitForKeyE();

            // Если фраза ещё печаталась, E только допечатывает её — ждём ещё одно нажатие
            while (dialog.TryCompleteText())
            {
                yield return inputService.WaitForKeyE();
            }
        }

        dialog.Disable();
    }
}
