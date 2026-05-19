# TODO

### АНИМАЦИИ (Пиксельная графика покадрово)
1. ЕВА
  - Умирает
2. ЗАЯЦ
  - Умирает

### ЗВУКИ
- Ева умирает
- Заяц умирает
- Босс (звуки взмаха крыльев, возможно он издает какие то страшные звуки)
- Музыка для заднего фона первого уровня

### СКРИПТЫ, ШЕЙДЕРЫ
- Шейдер тени персонажей, юнитов
- Шейдер Outline для интерактивных предметов
- Дрожание камеры при нахождении возле глаза

### UI
- На время показа диалога отображать внизу уменьшающуюся линию, которавя показывает длительность показа диалогового окна.

---

# БАГИ И ПРЕДЛОЖЕНИЯ ПОСЛЕ БЕТА-ТЕСТА v0.0.1

## БАГИ

### ДВИЖЕНИЕ И КОЛЛИЗИИ

- ☑ Если персонаж находится на самом краю препятствия, то CircleRaycast не дотягивается, в итоге персонаж не может ходить.
- ☑ Если подойти вплотную к препятствию, то персонаж прыгает строго вверх и не может перепрыгнуть препятствие.

### ВОЗРОЖДЕНИЕ

- ☑ Если постоянно спамить пробел и умереть от прожектора, то персонаж возрождается с задержкой.

### ИНТЕРФЕЙС

- Когда Ева умирает, DialogUi над ней остается

---

## ПРЕДЛОЖЕНИЯ

### ДИАЛОГОВОЕ ОКНО

- Убрать звук появления диалогового окна.
- Добавить звук бормотания при появлении диалогового окна.
- Звук осмотра — для каждого предмета свой.
- Диалоговое окно появляется только один раз, после повторного входа в триггерную зону не запускать диалог.

### ИНТЕРФЕЙС И УПРАВЛЕНИЕ

- Добавить окно выхода из игры при нажатии `ESC`: выход в главное меню или начало уровня с последнего чекпоинта.
- Добавить версию для мобилок.

### ОБУЧЕНИЕ И ТУТОРИАЛ

- В начале добавить интро, в котором минималистично рассказана история (комикс из 5 кадров).
- Нужно пояснение, что персонаж спрятан за кустом, а не просто серый спрайт и кнопка `CTRL`.

### МЕХАНИКА ПРЯТОК (`CTRL`)

- Если зайти в препятствие и зажать `CTRL`, то можно прыгать и бегать. 
  Нужно сделать так, чтобы персонаж спрятался и сидел, пока зажат `CTRL`, а при отжатии выходил из препятствия.
    Изменить логику ShelterTrigger:
```csharp
// Прототип новой реализации ShelterTrigger;
// Возможны ошибки || слабая оптимизация;

[SerializeField] private Player _player;
[SerializeField] private Dialog _ctrlDialog;
[SerializeField] private Animator _shelterAnimator;

[SerializeField] private InputService _inputService;
[SerializeField] private SpriteService _spriteService;

private bool _isEntered;   // Игрок зашел в укрытие
private bool _isActivated; // Укрытие активирвано, игрок спрятался

private OnTriggerEnter2D(Collider2D collider)
{
	if (collider.TryGetComponent(out Player _))
	{
		_isEntered = true;
		_ctrlDialog.Open();
	}
}

private OnTriggerStay2D(Collider2D collider)
{
	if (collider.TryGetComponent(out Player _))
	{
		if (_inputService.ControlIsHolding == true && _isActivated == false)
		{
			_isActivated = true;
			_ctrlDialog.Close();
			// Думаю, в Block() нужно блокировать Direction && Jump, все остальное - доступно
			_inputService.Block();
			_shelterAnimator.SetTrigger(TriggerBool.Open.ToString());
			_spriteService.Fade(_player.SpriteRenderer, FadeDirection.In);
		}
		else if (_inputService.ControlIsHolding == false && _isActivated == true) 
		{
			_isActivated = false
			_ctrlDialog.Open();
			_inputService.Unlock();
			_shelterAnimator.SetTrigger(TriggerBool.Close.ToString());
			_spriteService.Fade(_player.SpriteRenderer, FadeDirection.Out);
		}
	}
}

private OnTriggerExit2D(Collider2D collider)
{
	if (collider.TryGetComponent(out Player _))
	{
		_isEntered = false;
		_ctrlDialog.Close();	
	}
}

/*
	1. Если игрок зашел в зону действия укрытия:
		- Показать CTRL
		[IsEntered = true; IsActivated = false]
	2. Если игрок зажал CTRL:
		- Спрятать игрока за укрытием (Спрайт игрока тускнеет, запускается анимация урытия 'Close')
		- Скрыть CTRL
		- Не давать ему двигаться.
		[IsEntered = true; IsActivated = true]
	3. Если игрок отпустил CTRL
		- Игрок выходит из укрытия (Спрайт светлеет, запускается анимация укрытия 'Open')
		- Управление возращается
		- Показать CTRL
		[IsEntered = true; IsActivated = false]
	4. Если игрок вышел из зоны действия укрытия:
		- Скрыть CTRL
	    [IsEntered = false; IsActivated = false]
*/
```

### ВЗАИМОДЕЙСТВИЕ С ЗАПИСКАМИ

- Если читаешь записку, сделать так, чтобы она закрывалась на `E`, а не выключалась сама.
- Если читаешь записку, отключать управление персонажем.

### ГЕЙМПЛЕЙНЫЕ НАСТРОЙКИ

- Убрать банихоп.

### АУДИО

- Музыка для заднего фона проигрывается всего один раз — сделать её цикличной.

### ВИЗУАЛЬНАЯ ЧЁТКОСТЬ

- Сделать границы луча прожектора более понятными.
