"""Add first-guest strings to the existing RU/EN dialogue database."""
from pathlib import Path
import json

entries={
'order_title':('ЗАКАЗ №1','ORDER #1'),
'order_served':('ЗАКАЗ ВЫДАН','ORDER SERVED'),
'fridge_code':('Код холодильника:','Fridge code:'),
'tray':('ПОДНОС','TRAY'), 'pocket':('КАРМАН','POCKET'),
'ready':('ЗАКАЗ ГОТОВ · Отнесите его гостю','ORDER READY · Bring it to the guest'),
'complete':('Заказ выдан. Медицинская запись в кармане · I — прочитать','Order served. Medical record in pocket · I — read'),
'close':('ЗАКРЫТЬ','CLOSE'),
'station_0':('Фритюрница','Deep fryer'), 'station_1':('Блок предохранителей','Fuse box'),
'station_2':('Холодильник','Refrigerator'), 'station_3':('Автомат с напитками','Drinks machine'),
'station_4':('Зеркало','Mirror'), 'station_5':('Мужчина без лица','Faceless man'),
'item_0':('Бургер без булки','Burger without a bun'), 'item_1':('Картошка без соли','Fries without salt'),
'item_2':('Напиток без вкуса','Drink without taste'), 'item_3':('Игрушка без лица','Toy without a face'),
'item_description_0':('Никакой булки. Только то, что осталось на дне.\nПредмет заказа, слот 1 подноса.','No bun. Only what remained at the bottom.\nOrder item, tray slot 1.'),
'item_description_1':('Картошка из запертого холодильника. Ни крупинки соли.\nПредмет заказа, слот 2 подноса.','Fries from the locked refrigerator. Not a grain of salt.\nOrder item, tray slot 2.'),
'item_description_2':('Прозрачный напиток. Рецепт соблюдён, вкуса нет.\nПредмет заказа, слот 3 подноса.','A clear drink. The recipe is correct, but there is no taste.\nOrder item, tray slot 3.'),
'item_description_3':('На её лице ничего нет. Почему кажется, что она смотрит?\nПредмет заказа, слот 4 подноса.','There is nothing on its face. Why does it seem to be watching?\nOrder item, tray slot 4.'),
'fryer_powered':('От масла идёт дым. Сначала нужно отключить питание.','Smoke rises from the oil. Turn the power off first.'),
'find_fuse':('Питание выключено. В блоке на левой стене кухни ещё остался предохранитель. Найдите и выньте его, прежде чем сливать масло.','Power is off. A fuse remains in the box on the left kitchen wall. Find and remove it before draining the oil.'),
'drain_hint':('Предохранитель вынут. Теперь можно открыть сливной кран. На дне что-то осталось.','The fuse is removed. You can open the drain valve now. Something remains at the bottom.'),
'fryer_empty':('Старое масло слито. Бургер без булки лежит на дне фритюрницы.','The old oil is drained. The bunless burger lies at the bottom.'),
'power_off':('ОТКЛЮЧИТЬ ПИТАНИЕ','TURN OFF POWER'),
'find_fuse_button':('НАЙТИ ПРЕДОХРАНИТЕЛЬ','FIND THE FUSE'),
'drain':('СЛИТЬ СТАРОЕ МАСЛО','DRAIN OLD OIL'), 'take_burger':('ВЗЯТЬ БУРГЕР','TAKE BURGER'),
'fuse_safe':('Питание выключено. Выньте предохранитель фритюрницы, чтобы безопасно открыть слив.','Power is off. Remove the fryer fuse to make the drain safe.'),
'fuse_danger':('Не трогайте блок под напряжением. Сначала выключите питание на фритюрнице.','Do not touch the live fuse box. Switch off the fryer first.'),
'remove_fuse':('ВЫНУТЬ ПРЕДОХРАНИТЕЛЬ','REMOVE FUSE'),
'fridge_open':('Замок открыт. На полке лежит картошка без соли.','The lock is open. Unsalted fries sit on the shelf.'),
'fridge_locked':('Дверь заперта. Четыре цифры напечатаны на вашем чеке. Введите их на панели.','The door is locked. Four digits are printed on your receipt. Enter them on the panel.'),
'take_fries':('ВЗЯТЬ КАРТОШКУ','TAKE FRIES'),
'recipe':('Рецепт прозрачного напитка\nКрасный / янтарный / зелёный:','Clear drink recipe\nRed / amber / green:'),
'syrup_0':('Красный','Red'), 'syrup_1':('Янтарный','Amber'), 'syrup_2':('Зелёный','Green'),
'pour':('ПРИГОТОВИТЬ','PREPARE'), 'take_drink':('ВЗЯТЬ НАПИТОК','TAKE DRINK'),
'mirror_dark':('В отражении пусто. Посмотрите внимательнее.','There is nothing in the reflection. Look closer.'),
'mirror_toy':('Отражение исчезло. На столике появилась игрушка без лица.','The reflection is gone. A faceless toy has appeared on the table.'),
'look':('ПОСМОТРЕТЬ В ЗЕРКАЛО','LOOK IN THE MIRROR'), 'take_toy':('ВЗЯТЬ ИГРУШКУ','TAKE TOY'),
'deliver_hint':('Все четыре предмета на подносе. Гость всё ещё ждёт.','All four items are on the tray. The guest is still waiting.'),
'missing_items':('На подносе должны быть все четыре предмета. Недостающие отмечены на чеке.','All four items must be on the tray. Missing items are marked on the receipt.'),
'deliver':('ВЫДАТЬ ЗАКАЗ','SERVE ORDER'),
'code_ok':('Замок щёлкнул.','The lock clicks open.'), 'code_wrong':('Неверный код. Проверьте чек.','Wrong code. Check your receipt.'),
'recipe_wrong':('Не те пропорции. Сверьте все три значения с рецептом.','Wrong proportions. Match all three values to the recipe.'),
'power_safe':('Питание отключено. Найдите блок предохранителей.','Power is off. Find the fuse box.'),
'fuse_removed':('Предохранитель вынут. Слив безопасен.','Fuse removed. The drain is safe.'),
'added':('Предмет добавлен на поднос.','Item added to the tray.'),
'draining':('Сливаем старое масло…','Draining old oil…'), 'pouring':('Наполняем стакан…','Filling the cup…'),
'looking':('Отражение меняется…','The reflection changes…'), 'action_done':('Готово.','Done.'),
'record_title':('Фрагмент медицинской записи','Medical record fragment'),
'record_text':('МЕДИЦИНСКАЯ ЗАПИСЬ · ФРАГМЕНТ\n\nПациент: Ева.\nНаблюдение 01.\n\nРеакция на отсутствие лица: узнавание.\nПамять о событии не восстановлена.\n\nНа обороте: «Ты уже обслуживала его».\n\nНаграда хранится в кармане.','MEDICAL RECORD · FRAGMENT\n\nPatient: Eva.\nObservation 01.\n\nResponse to the absence of a face: recognition.\nMemory of the event has not returned.\n\nOn the back: "You have served him before."\n\nThe reward is kept in your pocket.'),
'tension':('НАПРЯЖЕНИЕ','TENSION'),
'clown_0':('Клоун: «Наш гость уже заждался».','Clown: "Our guest has been waiting."'),
'clown_1':('Клоун: «Улыбайся. Время идёт».','Clown: "Smile. Time is passing."'),
'clown_2':('Клоун: «Не заставляй его вспоминать».','Clown: "Do not make him remember."'),
'bathroom':('УБОРНАЯ →','RESTROOM →'), 'exit_bathroom':('В ЗАЛ →','HALL →'),
}
path=Path('Assets/Resources/dialogues.json')
database=json.loads(path.read_text(encoding='utf-8-sig'))
for key,(ru,en) in entries.items():
    identity='service_'+key
    existing=next((d for d in database['dialogues'] if d['id']==identity),None)
    value={'id':identity,'russian':ru,'english':en}
    if existing is not None:existing.update(value)
    else:database['dialogues'].append(value)
for identity,ru,en in [
('fastfood_faceless_complete_1','Всё на месте. Даже то, чего нет.','Everything is here. Even what is missing.'),
('fastfood_faceless_complete_2','Это было у меня в кармане. Кажется, здесь написано ваше имя.','This was in my pocket. I think it has your name on it.'),
('fastfood_service_admin_1','Первый гость уже идёт. Чек появится сам. На кухню проходите налево.','The first guest is coming. The receipt will appear on its own. The kitchen is to the left.'),
('fastfood_service_admin_2','Соберите заказ на поднос. Полученные от гостей вещи держите в кармане.','Collect the order on the tray. Keep anything the guests give you in your pocket.'),
]:
    existing=next((d for d in database['dialogues'] if d['id']==identity),None)
    value={'id':identity,'russian':ru,'english':en}
    if existing is not None:existing.update(value)
    else:database['dialogues'].append(value)
path.write_text(json.dumps(database,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('Service translations prepared:',len(entries)+4)
