-- EvaController: управление персонажем в окне-превью.
-- Берёт анимации из тегов активного спрайта (Idle, Walking, Running, Jump,
-- Crouching, Sitting, WakingUp, Interact) и использует длительности кадров
-- из таймлайна, так что после правок достаточно нажать "Обновить".

local spr = app.sprite
if not spr then
  app.alert("Сначала откройте EvaDress.aseprite или EvaFastFood.aseprite")
  return
end

local TICK = 1 / 60
local WALK_SPEED = 45   -- пикселей спрайта в секунду
local RUN_SPEED = 110
local SNEAK_SPEED = 25
local JUMP_HEIGHT = 22

local anims = {}
local frameW, frameH = spr.width, spr.height
local feet = 0      -- пустые строки под ногами в кадре Idle
local canvasW = 720

-- фон: ищем рядом со скриптом, затем в папке над спрайтом
local BG_FILE = "Ночная пиксельная кухня закусочной.png"
local bgSrc, bgScaled

local function loadBackground()
  bgSrc, bgScaled = nil, nil
  local dirs = {}
  local src = debug.getinfo(1, "S").source
  if src:sub(1, 1) == "@" then dirs[#dirs + 1] = app.fs.filePath(src:sub(2)) end
  local sprDir = app.fs.filePath(spr.filename)
  dirs[#dirs + 1] = sprDir
  dirs[#dirs + 1] = app.fs.filePath(sprDir)
  for _, d in ipairs(dirs) do
    local path = app.fs.joinPath(d, BG_FILE)
    if app.fs.isFile(path) then
      bgSrc = Image{ fromFile = path }
      return
    end
  end
end

-- масштабируем фон под холст один раз (cover: заполняет, лишнее обрезается)
local function background(W, H)
  if not bgSrc then return nil end
  if not bgScaled or bgScaled.W ~= W or bgScaled.H ~= H then
    local k = math.max(W / bgSrc.width, H / bgSrc.height)
    local img = bgSrc:clone()
    img:resize(math.ceil(bgSrc.width * k), math.ceil(bgSrc.height * k))
    bgScaled = { img = img, W = W, H = H }
  end
  return bgScaled.img
end

loadBackground()

local function flipped(img)
  local out = Image(img.spec)
  local w = img.width
  for y = 0, img.height - 1 do
    for x = 0, w - 1 do
      out:drawPixel(w - 1 - x, y, img:getPixel(x, y))
    end
  end
  return out
end

local function loadAnims()
  anims = {}
  frameW, frameH = spr.width, spr.height
  for _, tag in ipairs(spr.tags) do
    local list = {}
    for f = tag.fromFrame.frameNumber, tag.toFrame.frameNumber do
      local img = Image(spr.spec)
      img:drawSprite(spr, f)
      list[#list + 1] = { img = img, flip = flipped(img), dur = spr.frames[f].duration }
    end
    anims[tag.name] = list
  end
  feet = 0
  local ref = (anims.Idle or {})[1]
  if ref then
    local img = ref.img
    for y = img.height - 1, 0, -1 do
      local empty = true
      for x = 0, img.width - 1 do
        if app.pixelColor.rgbaA(img:getPixel(x, y)) > 0 then empty = false break end
      end
      if not empty then break end
      feet = feet + 1
    end
  end
end

loadAnims()

-- состояние персонажа
local st = {
  anim = "WakingUp", frame = 1, t = 0, done = false,
  x = 0, y = 0, vy = 0, g = 0, facing = 1,
}
local keys = {}
local shift = false
local ctrl = false
local dlg

local LOOPING = { Idle = true, Walking = true, Running = true, Crouching = true, Sitting = true }

local function play(name)
  if st.anim == name or not anims[name] then return end
  st.anim, st.frame, st.t, st.done = name, 1, 0, false
end

local function totalDur(name)
  local s = 0
  for _, fr in ipairs(anims[name] or {}) do s = s + fr.dur end
  return s
end

local function startJump()
  local T = totalDur("Jump")
  if T <= 0 then return end
  -- время в воздухе = длительность тега Jump
  st.vy = 4 * JUMP_HEIGHT / T
  st.g = 8 * JUMP_HEIGHT / (T * T)
  st.y = 0
  play("Jump")
end

local function held(...)
  for _, k in ipairs({ ... }) do
    if keys[k] then return true end
  end
  return false
end

local function update(dt)
  local speedMul = dlg.data.speed / 100
  local adt = dt * speedMul
  local dir = 0
  if held("ArrowLeft", "KeyA") then dir = dir - 1 end
  if held("ArrowRight", "KeyD") then dir = dir + 1 end
  local busy = (st.anim == "WakingUp" or st.anim == "Interact") and not st.done

  if st.anim == "Jump" then
    if dir ~= 0 then
      st.facing = dir
      st.x = st.x + dir * (shift and RUN_SPEED or WALK_SPEED) * adt
    end
    st.y = st.y + st.vy * adt
    st.vy = st.vy - st.g * adt
    if st.y <= 0 then
      st.y, st.vy = 0, 0
      st.done = true
      st.anim = nil
    end
  elseif not busy then
    if dir ~= 0 then
      st.facing = dir
    end
    if ctrl and dir ~= 0 then
      -- Ctrl + движение: крадётся
      st.x = st.x + dir * SNEAK_SPEED * adt
      play("Crouching")
    elseif ctrl then
      -- Ctrl на месте: сидит
      play("Sitting")
    elseif dir ~= 0 then
      st.x = st.x + dir * (shift and RUN_SPEED or WALK_SPEED) * adt
      play(shift and "Running" or "Walking")
    else
      play("Idle")
    end
  end

  -- заворачиваем по краям поля
  local worldW = canvasW / dlg.data.zoom
  if st.x < -frameW / 2 then st.x = worldW + frameW / 2 end
  if st.x > worldW + frameW / 2 then st.x = -frameW / 2 end

  -- продвигаем кадры
  local list = anims[st.anim]
  if list and #list > 0 and not st.done then
    st.t = st.t + adt
    while st.t >= list[st.frame].dur do
      st.t = st.t - list[st.frame].dur
      if st.frame < #list then
        st.frame = st.frame + 1
      elseif LOOPING[st.anim] then
        st.frame = 1
      else
        st.done = true
        if st.anim ~= "Jump" then st.anim = nil end
        break
      end
    end
  end
  if st.anim == nil then play("Idle") end
end

local function isCtrl(code) return code == "ControlLeft" or code == "ControlRight" end

local function onKeyDown(ev)
  shift = ev.shiftKey or ev.code == "ShiftLeft" or ev.code == "ShiftRight"
  ctrl = ev.ctrlKey or isCtrl(ev.code)
  local wasHeld = keys[ev.code]
  keys[ev.code] = true
  if not wasHeld then
    local grounded = st.anim ~= "Jump"
    if (ev.code == "Space" or ev.code == "ArrowUp" or ev.code == "KeyW") and grounded then
      startJump()
    elseif ev.code == "KeyE" and grounded then
      play("Interact")
      st.frame, st.t, st.done = 1, 0, false
    elseif ev.code == "KeyZ" and grounded then
      play("WakingUp")
      st.frame, st.t, st.done = 1, 0, false
    elseif ev.code == "KeyR" then
      loadAnims()
    end
  end
  ev:stopPropagation()
end

local function onKeyUp(ev)
  keys[ev.code] = nil
  if ev.code == "ShiftLeft" or ev.code == "ShiftRight" then shift = false
  else shift = ev.shiftKey end
  if isCtrl(ev.code) then ctrl = false
  else ctrl = ev.ctrlKey end
  ev:stopPropagation()
end

local function onPaint(ev)
  local gc = ev.context
  gc.antialias = false
  local W, H = gc.width, gc.height
  canvasW = W
  local z = dlg.data.zoom
  local bg = dlg.data.showBg and background(W, H)
  local groundY = bg and math.floor(H * (1 - dlg.data.floor / 100)) or H - 30

  if bg then
    gc:drawImage(bg, math.floor((W - bg.width) / 2), math.floor((H - bg.height) / 2))
  else
    gc.color = Color{ r = 58, g = 62, b = 74 }
    gc:fillRect(Rectangle(0, 0, W, H))
    gc.color = Color{ r = 40, g = 42, b = 50 }
    gc:fillRect(Rectangle(0, groundY, W, H - groundY))
    gc.color = Color{ r = 110, g = 116, b = 130 }
    gc:fillRect(Rectangle(0, groundY, W, 1))
  end

  local list = anims[st.anim]
  if list and list[st.frame] then
    local fr = list[st.frame]
    local img = st.facing < 0 and fr.flip or fr.img
    local dx = math.floor(st.x * z - frameW * z / 2)
    local dy = math.floor(groundY - (frameH - feet + st.y) * z)
    gc:drawImage(img, Rectangle(0, 0, frameW, frameH), Rectangle(dx, dy, frameW * z, frameH * z))
  end

  -- подложки под текст, чтобы читался поверх фона
  gc.color = Color{ r = 0, g = 0, b = 0, a = 150 }
  gc:fillRect(Rectangle(0, 0, W, 22))
  gc:fillRect(Rectangle(0, H - 28, W, 28))

  gc.color = Color{ r = 230, g = 230, b = 230 }
  gc:fillText(string.format("%s  кадр %d/%d", st.anim or "-", st.frame, list and #list or 0), 8, 8)
  gc.color = Color{ r = 160, g = 165, b = 175 }
  gc:fillText("Стрелки или A/D — ходьба, +Shift — бег, Space/W — прыжок", 8, H - 22)
  gc:fillText("Ctrl — сидеть, Ctrl+ходьба — красться, E — взаимодействие, Z — пробуждение, R — обновить из файла", 8, H - 10)
end

local timer
dlg = Dialog{
  title = "Eva Controller — " .. app.fs.fileTitle(spr.filename),
  onclose = function() if timer then timer:stop() end end,
}
dlg:canvas{
  id = "view", width = 720, height = 405, focus = true, autoscaling = false,
  onpaint = onPaint, onkeydown = onKeyDown, onkeyup = onKeyUp,
  onmousedown = function() keys = {} end,
}
dlg:slider{ id = "zoom", label = "Масштаб", min = 1, max = 6, value = 3 }
dlg:slider{ id = "speed", label = "Скорость анимации %", min = 10, max = 200, value = 100 }
dlg:check{ id = "showBg", text = "Фон" .. (bgSrc and "" or " (файл не найден)"), selected = bgSrc ~= nil }
dlg:slider{ id = "floor", label = "Уровень пола %", min = 0, max = 50, value = 12 }
dlg:button{ text = "Обновить из файла", onclick = function() loadAnims(); loadBackground(); dlg:repaint() end }
dlg:button{ text = "Закрыть", onclick = function() dlg:close() end }

st.x = canvasW / 2 / 3
timer = Timer{
  interval = TICK,
  ontick = function()
    update(TICK)
    dlg:repaint()
  end,
}
dlg:show{ wait = false }
timer:start()
