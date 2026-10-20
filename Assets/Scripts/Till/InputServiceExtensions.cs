using System.Collections;

public static class InputServiceExtensions
{
    public static IEnumerator WaitForKeyE(this InputService inputService)
    {
        // Пропускаем кадр, чтобы не засчитать нажатие, которое запустило ожидание
        yield return null;

        while (inputService.EIsPressed == false)
        {
            yield return null;
        }
    }
}
